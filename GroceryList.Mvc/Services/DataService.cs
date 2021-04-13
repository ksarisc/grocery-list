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
        public DbConnection GetConnection(AppUser user);
    }

    public class DataService : IDataService
    {
        private const bool read = true;
        private const bool write = false;
        private const int bufferSize = 16384;
        private const string connect = "Data Source={0};Cache=Shared";
        // single sqlite database for main
        private const string dbfile = "grocerylist.sqlite";

        private readonly SqliteConnection conn;
        private readonly string dataPath;
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
            // should we cache the path lookups?
            // TODO: need to determine how often a lookup is required
            conn = new SqliteConnection(String.Format(connect,
                Path.Combine(dataPath, dbfile)));
        }

        private bool IsAllowed(AppUser user, string path)
        {
            return path.IndexOf(user.GetHome(), StringComparison.Ordinal) != -1;
        }

        // private string GetHomePath(AppUser user)
        // {
        //     return Path.Combine(dataPath, user.GetHome());
        // }

        // private string GetDataPath(AppUser user, DateTime? dateTime)
        // {
        //     var file = !dateTime.HasValue ? current : dateTime.Value.ToString(dtformat);
        //     return Path.Combine(GetHomePath(user), file);
        // }

        // private async ValueTask<string> GetSession(AppUser user)
        // {
        //     var userId = user.GetId();
        //     if (!lists.TryGetValue(userId, out var path))
        //     {
        //         throw new ArgumentException("No session found");
        //     }
        //     //await Console.Out.WriteLineAsync($"GetSession: {userPath}|{userId}");
        //     return await File.ReadAllTextAsync(String.Format(userPath, userId));
        // } // END GetSession
        // private async ValueTask SetSession(AppUser user, string modelType, string modelPath)
        // {
        //     // save the path somehow temporarily
        //     // probably should be saved to sqlite global or home specific later
        //     // probably should just use a real database

        //     // using (var curr = GetFile(String.Format(userPath, userId), write))
        //     // {
        //     //     // currently not setting the modelType, but if more models are needed, will save
        //     //     var bytes = Encoding.UTF8.GetBytes(modelPath);//$"{model}")
        //     //     await curr.WriteAsync(bytes, 0, bytes.Length);
        //     // }
        //     var userId = user.GetId();
        //     lists[userId] = modelPath;
        //     //await Console.Out.WriteLineAsync($"SetSession: {userPath}|{userId}");
        //     await File.WriteAllTextAsync(String.Format(userPath, userId), modelPath);
        // } // END SetSession

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
