using GroceryList.Models.Config;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GroceryList.Services
{
    /// <summary>
    /// Principle: connect "a" file system to retrieve files based on home indicated in route
    /// </summary>
    public interface IDataService
    {
        public Task<T> GetAsync<T>(string homeId, string fileName);
    }

    public class DataService : IDataService
    {
        private readonly string dataPath;

        public DataService(IOptions<DataServiceConfig> options)
        {
            dataPath = options.Value.DataPath;
        }

        public async Task<T> GetAsync<T>(string homeId, string fileName)
        {
            var path = Path.Combine(dataPath, homeId, fileName);
            if (!File.Exists(path))
            {
                return default(T);
            }
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
            return await JsonSerializer.DeserializeAsync<T>(file, options, cancel);
        }
    }
}
