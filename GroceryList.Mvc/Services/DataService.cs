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
        private readonly Dictionary<Guid, string> lists = new Dictionary<Guid, string>();

        public DataService(IOptions<DataConfig> options)
        {
            dataPath = options.Value.Path;
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            userPath = Path.Combine(dataPath, "user");
            if (!Directory.Exists(userPath))
            {
                Directory.CreateDirectory(userPath);
            }
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

        private string GetDataPath(AppUser user, DateTime? dateTime)
        {
            var file = !dateTime.HasValue ? current : dateTime.Value.ToString(dtformat);
            return Path.Combine(dataPath, user.GetHome(), file);
        }

        // public T GetDataFile<T>(AppUser user, bool getCurrent = true){
        //     var path = GetDataPath(user, getCurrent);
        //     return new T(path, JsonSerializer.Deserialize<T>(
        //             File.ReadAllText(path)));
        //     //return default;
        // }

        private async ValueTask<string> GetSession(Guid userId)
        {
            if (!lists.TryGetValue(userId, out var path))
            {
                throw new ArgumentException("No session found");
            }
            return await File.ReadAllTextAsync(String.Format(userPath, userId));
        } // END GetSession
        private async ValueTask SetSession(Guid userId, string modelType, string modelPath)
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
            lists[userId] = modelPath;
            await File.WriteAllTextAsync(String.Format(userPath, userId), modelPath);
        } // END SetSession

        public async ValueTask<T> GetDataAsync<T>(AppUser user, DateTime? dateTime = null)
        {
            var path = GetDataPath(user, dateTime);
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("No data found");
            }
            using (var file = GetFile(path, read))
            {
                var value = await JsonSerializer.DeserializeAsync<T>(file);
                await SetSession(user.Id, typeof(T).FullName, path);
                return value;
            }
        } // END GetDataAsync

        public async ValueTask SetDataAsync<T>(AppUser user, T data)
        {
            // if (!IsAllowed(user, path))
            // {
            //     throw new Exception($"User ({user.Email}) does NOT have permission to save");
            // }
            var path = await GetSession(user.Id);
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
