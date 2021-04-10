using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Config;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System;
using System.Data.Common;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public interface IDataService
    {
        public DbConnection GetConnection();
        public string GetDataPath(AppUser user, bool getCurrent = true);

        public Task<T> GetDataFileAsync<T>(AppUser user, bool getCurrent = true);
    }

    public class DataService : IDataService
    {
        // single sqlite database for main
        private const string dbfile = "grocerylist.sqlite";
        // main list for given home
        private const string current = "current.json";

        private readonly DataConfig conf;
        private readonly SqliteConnection conn;

        public DataService(IOptions<DataConfig> options)
        {
            conf = options.Value;
            // should we cache the path lookups?
            // TODO: need to determine how often a lookup is required
            conn = new SqliteConnection(
                Path.Combine(conf.Path, dbfile));
        }

        public DbConnection GetConnection()
        {
            return conn;
        }

        private string GetFile(AppUser user, string file)
        {
            // to string uses `D` format
            return Path.Combine(conf.Path, user.HomeId.ToString(), file);
        }

        public string GetDataPath(AppUser user, bool getCurrent = true)
        {
            if (getCurrent)
            {
                return GetFile(user, current);
            }
            return null;
        }

        public T GetDataFile<T>(AppUser user, bool getCurrent = true)
        {
            return JsonSerializer.Deserialize<T>(
                File.ReadAllText(GetDataPath(user, getCurrent)));
            //return default;
        }

        public async Task<T> GetDataFileAsync<T>(AppUser user, bool getCurrent = true)
        {
            var path = GetDataPath(user, getCurrent);
            using (var file = new FileStream(path, FileMode.Open,
                        FileAccess.Read, FileShare.Read, 16384, true))
            {
                return await JsonSerializer.DeserializeAsync<T>(file);
            }
        } // END GetDataFileAsync

        // save current file
    }
}
