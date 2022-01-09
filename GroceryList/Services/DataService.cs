using GroceryList.Models.Config;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GroceryList.Services
{
    /// <summary>
    /// Principle: connect to a "file system" to retrieve files based on home indicated in route
    /// </summary>
    public interface IDataService
    {
        public Task<bool> HomeExistsAsync(string homeId);
        public Task<Models.Home> AddHomeAsync(Models.Home home);

        public Task<T> GetAsync<T>(string homeId, string storeName);
        public Task SetAsync(string homeId, string storeName, object data);
    }

    public class DataService : IDataService
    {
        private readonly string dataPath;

        public DataService(IOptions<DataServiceConfig> options)
        {
            dataPath = options.Value.DataPath;
        }

        private string GetFilePath(in string homeId, in string fileName)
        {
            return Path.Combine(dataPath, homeId, $"{fileName}.json");
        }

        public Task<bool> HomeExistsAsync(string homeId)
        {
            return Task.FromResult(Directory.Exists(Path.Combine(dataPath, homeId)));
        }
        public async Task<Models.Home> AddHomeAsync(Models.Home home) // should the home be defined prior to add?
        {
            if (string.IsNullOrWhiteSpace(home.Id)) throw new ArgumentNullException("ID required");

            var path = Path.Combine(dataPath, home.Id);
            // this should NOT be returned to the user even though it should never actually happen
            if (Directory.Exists(path)) throw new ArgumentOutOfRangeException("Home already exists");

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "bak"));
            Directory.CreateDirectory(Path.Combine(path, "trip"));

            var hfile = Path.Combine(path, "home.json");
            using var file = new FileStream(hfile, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, true);
            await JsonSerializer.SerializeAsync(file, home); //, jsonOptions, cancel);

            return home;
        } // END AddHomeAsync

        public async Task<T> GetAsync<T>(string homeId, string storeName)
        {
            var path = GetFilePath(homeId, storeName);
            if (!File.Exists(path))
            {
                return default(T);
            }
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
            return await JsonSerializer.DeserializeAsync<T>(file); //, jsonOptions, cancel);
        }

        public async Task SetAsync(string homeId, string storeName, object data)
        {
            var path = GetFilePath(homeId, storeName);
            // ?? backup/archive ??
            if (File.Exists(path))
            {
                File.Move(path, path + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            }
            using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, true);
            await JsonSerializer.SerializeAsync(file, data); //, jsonOptions, cancel);
        }
    }
}
