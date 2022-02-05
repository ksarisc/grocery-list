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
    public interface IDataService : IDisposable
    {
        public Task<bool> HomeExistsAsync(string homeId);
        public Task<Models.Home> AddHomeAsync(Models.Home home);
        public Task<Models.Home> GetHomeAsync(string homeId);
        
        public Task<T> GetAsync<T>(string homeId, string storeName);
        public Task<T> GetAsync<T>(Models.DataRequest request);
        public Task SetAsync(string homeId, string storeName, object data);
        public Task SetAsync(Models.DataRequest request, object data);
    }

    public class DataService : IDataService
    {
        private const int bufferSize = 8192;
        private readonly string dataPath;

        public DataService(IOptions<DataServiceConfig> options)
        {
            // need to be able to define the base (for different types of data)
            // also need to have a better locking strategy for updates (none right now)
            dataPath = options.Value.DataPath;
            if (Utils.IsLinux){
                dataPath = options.Value.DataPathLinux;
            }
        }

        private string GetFilePath(in string homeId, in string fileName)
        {
            return Path.Combine(dataPath, homeId, $"{fileName}.json");
        }
        private FileInfo GetTypePath(in Models.DataRequest request)
        {
            return new FileInfo(
                Path.Combine(dataPath, request.HomeId, request.ActionName, $"{request.StoreName}.json")
            );
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
            //Directory.CreateDirectory(Path.Combine(path, "trip"));

            var hfile = Path.Combine(path, "home.json");
            using var file = new FileStream(hfile, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize, true);
            await JsonSerializer.SerializeAsync(file, home); //, jsonOptions, cancel);

            return home;
        } // END AddHomeAsync
        public async Task<Models.Home> GetHomeAsync(string homeId)
        {
            var path = Path.Combine(dataPath, homeId, "home.json");
            if (!File.Exists(path))
            {
                return default(Models.Home);
            }
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            return await JsonSerializer.DeserializeAsync<Models.Home>(file);
        } // END GetHomeAsync

        public async Task<T> GetAsync<T>(string homeId, string storeName)
        {
            var path = GetFilePath(homeId, storeName);
            if (!File.Exists(path))
            {
                return default(T);
            }
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            return await JsonSerializer.DeserializeAsync<T>(file); //, jsonOptions, cancel);
        }
        public async Task<T> GetAsync<T>(Models.DataRequest request)
        {
            var info = GetTypePath(request);
            if (!info.Directory.Exists)
            {
                throw new ArgumentOutOfRangeException($"Request path ({info.FullName}) NOT valid");
            }
            if (!File.Exists(info.FullName))
            {
                return default(T);
            }
            using var file = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            return await JsonSerializer.DeserializeAsync<T>(file); //, jsonOptions, cancel);
        } // END GetAsync

        public async Task SetAsync(string homeId, string storeName, object data)
        {
            var path = GetFilePath(homeId, storeName);
            var exists = File.Exists(path);

            if (exists)
            {
                // ?? backup/archive ??
                var bak = GetTypePath(new Models.DataRequest
                {
                    HomeId = homeId,
                    StoreName = storeName + Utils.GetNewId(),
                    ActionName = "bak"
                });
                using var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, bufferSize, true);
                using var bakFile = new FileStream(bak.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
                await file.CopyToAsync(bakFile);
                // reset position?
                file.SetLength(0);
                //File.Move(path, bak);
                await file.FlushAsync();
            }
            if (data != null)
            {
                using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);
                await JsonSerializer.SerializeAsync(file, data); //, jsonOptions, cancel);
            }
            else
            {
                File.Delete(path);
            }
        } // END SetAsync

        public async Task SetAsync(Models.DataRequest request, object data)
        {
            request.StoreName += Utils.GetNewId();
            var info = GetTypePath(request);
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }
            // ?? backup/archive ??
            using var file = new FileStream(info.FullName, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize, true);
            await JsonSerializer.SerializeAsync(file, data); //, jsonOptions, cancel);
        } // END SetAsync

        #region cleanup
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
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