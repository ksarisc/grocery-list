using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Config;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public interface IDataService : IDisposable
    {
        //public DbConnection GetConnection();
        //public string GetDataPath(AppUser user, bool getCurrent = true);

        public ValueTask<T> GetDataAsync<T>(AppUser user, DateTime? dateTime = null);
        public ValueTask SetDataAsync<T>(AppUser user, T data);
    }

    public class DataService : IDataService
    {
        private const bool read = true;
        private const bool write = false;
        private const int bufferSize = 16384;
        private const string connect = "Data Source={0};Cache=Shared";
        // single sqlite database for main
        private const string dbfile = "grocerylist.sqlite";
        private const string dtformat = "yyyyMMdd_HHmmss";
        // main list for given home
        private const string current = "current.json";

        private readonly SqliteConnection conn;
        private readonly string dataPath;
        private readonly string userPath;
        private readonly Dictionary<string, string> lists =
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

        // public DbConnection GetConnection(){
        //     return conn;
        // }

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

        // public T GetDataFile<T>(AppUser user, bool getCurrent = true){
        //     var path = GetDataPath(user, getCurrent);
        //     return new T(path, JsonSerializer.Deserialize<T>(
        //             File.ReadAllText(path)));
        //     //return default;
        // }

        private async ValueTask<string> GetSession(AppUser user)
        {
            var userId = user.GetId();
            if (!lists.TryGetValue(userId, out var path))
            {
                throw new ArgumentException("No session found");
            }
            //await Console.Out.WriteLineAsync($"GetSession: {userPath}|{userId}");
            return await File.ReadAllTextAsync(String.Format(userPath, userId));
        } // END GetSession
        private async ValueTask SetSession(AppUser user, string modelType, string modelPath)
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
            lists[userId] = modelPath;
            //await Console.Out.WriteLineAsync($"SetSession: {userPath}|{userId}");
            await File.WriteAllTextAsync(String.Format(userPath, userId), modelPath);
        } // END SetSession

        public async ValueTask<T> GetDataAsync<T>(AppUser user, DateTime? dateTime = null)
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

        public async ValueTask SetDataAsync<T>(AppUser user, T data)
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

        #region cleanup
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            conn.Dispose();
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
