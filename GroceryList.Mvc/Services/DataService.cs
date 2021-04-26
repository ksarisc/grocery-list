using Dapper;
using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Config;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public interface IDataService : IDisposable
    {
        //public DbConnection GetConnection();
        public Task<T> GetDataAsync<T>(AppUser user, DateTime? dateTime = null);
        public Task SetDataAsync<T>(AppUser user, T data);
        public DbConnection GetConnection(AppUser user);
        public Task<bool> TableExists(AppUser user, string table, string createIfMissing = null);
        public Task<bool> TableExists(DbConnection connection, string table, string createIfMissing = null);

        public Task<int> ExecuteAsync(string query, object parameters, CancellationToken cancel);
    }

    public class DataService : IDataService
    {
        private const bool read = true;
        private const bool write = false;
        private const int bufferSize = 16384;
        private const string connect = "Data Source={0};Cache=Shared";
        // single sqlite database for main
        private const string dbfile = "grocerylist.sqlite";
        private const string current = "current.json";
        private const string dtformat = "yyyyMMdd_HHmmss";
        private const string exists = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = @TableName;";

        private readonly SqliteConnection conn;
        private readonly string dataPath;
        private readonly string userPath;
        private readonly Dictionary<string, string> homes =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public DataService(IOptions<DataConfig> options)
        {
            if (String.IsNullOrWhiteSpace(options.Value.Path))
            {
                throw new ArgumentNullException("DataConfig Path REQUIRED");
            }
            dataPath = Path.GetFullPath(options.Value.Path);
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            userPath = Path.Combine(dataPath, "user");
            if (!Directory.Exists(userPath))
            {
                Directory.CreateDirectory(userPath);
            }
            userPath = Path.Combine(userPath, "ses_{0}");
            // should we cache the path lookups?
            // TODO: need to determine how often a lookup is required
            conn = new SqliteConnection(String.Format(connect,
                Path.Combine(dataPath, dbfile)));
        }

        private static Stream GetFile(string path, bool read)
        {
            if (read)
            {
                return new FileStream(path, FileMode.Open,
                        FileAccess.Read, FileShare.Read, bufferSize, true);
            }
            return new FileStream(path, FileMode.OpenOrCreate,
                        FileAccess.Write, FileShare.None, bufferSize, true);
        } // END GetFile

        private bool IsAllowed(AppUser user, string path)
        {
            return path.IndexOf(user.GetHome(), StringComparison.Ordinal) != -1;
        }

        private string GetHomePath(AppUser user)
        {
            return Path.Combine(dataPath, user.GetHome());
        }

        private string GetDataPath(AppUser user, DateTime? dateTime)
        {
            var file = !dateTime.HasValue ? current : dateTime.Value.ToString(dtformat);
            return Path.Combine(GetHomePath(user), file);
        }

        private async Task<string> GetSession(AppUser user)
        {
            var userId = user.GetId();
            //await Console.Out.WriteLineAsync($"GetSession: {userPath}|{userId}");
            // using (var file = GetFile(String.Format(userPath, userId),read){
            //     JsonSerializer
            return await File.ReadAllTextAsync(String.Format(userPath, userId));
        } // END GetSession
        private async Task SetSession(AppUser user, string modelType, string modelPath)
        {
            // save the path somehow temporarily
            // probably should be saved to sqlite global or home specific later
            // probably should just use a real database

            // using (var curr = GetFile(String.Format(userPath, userId), write))
            // {
            //     // currently not setting the modelType, but if more models are needed, will save
            //     var bytes = Encoding.UTF8.GetBytes(modelPath);//$"{model}")
            //     await curr.WriteAsync(bytes, 0, bytes.Length);
            // }
            var userId = user.GetId();
            //await Console.Out.WriteLineAsync($"SetSession: {userPath}|{userId}");
            await File.WriteAllTextAsync(String.Format(userPath, userId), modelPath);
        } // END SetSession

        public async Task<T> GetDataAsync<T>(AppUser user, DateTime? dateTime = null)
        {
            var path = GetDataPath(user, dateTime);
            if (String.IsNullOrWhiteSpace(path))
            {
                await SetSession(user, typeof(T).FullName, String.Empty);
                throw new ArgumentException("No data found");
            }
            if (!File.Exists(path))
            {
                await SetSession(user, typeof(T).FullName, $"{path}|");
                return default(T);
            }
            using (var file = GetFile(path, read))
            {
                var value = await JsonSerializer.DeserializeAsync<T>(file);
                await SetSession(user, typeof(T).FullName, path);
                return value;
            }
        } // END GetDataAsync

        public async Task SetDataAsync<T>(AppUser user, T data)
        {
            // if (!IsAllowed(user, path))
            // {
            //     throw new Exception($"User ({user.Email}) does NOT have permission to save");
            // }
            var path = await GetSession(user);
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("No Session Found!");
            }
            if (path.EndsWith('|'))
            {
                var folder = GetHomePath(user);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                path = path.Substring(0, path.Length - 1);
            }
            using (var file = new FileStream(path, FileMode.OpenOrCreate,
                        FileAccess.Write, FileShare.None, 16384, true))
            {
                await JsonSerializer.SerializeAsync(file, data);
            }
        } // END SetDataAsync

        public DbConnection GetConnection(AppUser user)
        {
            //homePath = Path.Combine(dataPath, "{0}", dbfile);
            // recreate connect or share it?
            var homeId = user.GetHome();
            if (homes.TryGetValue(homeId, out var connStr))
            {
                return new SqliteConnection(connStr);
            }
            var homePath = Path.Combine(dataPath, homeId);
            if (!Directory.Exists(homePath))
            {
                Directory.CreateDirectory(homePath);
            }
            connStr = String.Format(connect, Path.Combine(homePath, dbfile));
            homes[homeId] = connStr;
            return new SqliteConnection(connStr);
        } // END GetConnection

        public async Task<bool> TableExists(AppUser user, string table, string createIfMissing = null)
        {
            using (var conn = GetConnection(user))
            {
                return await TableExists(conn, table, createIfMissing);
            }
        } // END TableExists

        public async Task<bool> TableExists(DbConnection connection, string table, string createIfMissing = null)
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = exists;
                var pname = cmd.CreateParameter();
                pname.ParameterName = "@TableName";
                pname.Value = table;
                cmd.Parameters.Add(pname);
                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    if (rdr.HasRows)
                    {
                        return true;
                    }
                }
            }
            if (String.IsNullOrWhiteSpace(createIfMissing))
            {
                return false;
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = createIfMissing;
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
        } // END TableExists

        public async Task<int> ExecuteAsync(string query, object parameters, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(cancel);
            }
            return await conn.ExecuteAsync(query, parameters);
        } // END ExecuteAsync

        public async Task<T> QuerySingleAsync<T>(SqlBuilder.Template template, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(cancel);
            }

            return await conn.QuerySingleOrDefaultAsync<T>(
                template.RawSql, template.Parameters);
        } // END QuerySingleAsync

        public async Task<IEnumerable<T>> QueryAsync<T>(SqlBuilder.Template template, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(cancel);
            }

            return await conn.QueryAsync<T>(template.RawSql, template.Parameters);
        } // END QueryAsync

        #region cleanup
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                conn.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~DataService()
        {
            Dispose(false);
        }
        #endregion cleanup
    }
}
