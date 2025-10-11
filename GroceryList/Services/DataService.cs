using GroceryList.Models.Config;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Services
{
    /// <summary>
    /// Principle: connect to a "file system" to retrieve files based on home indicated in route
    /// </summary>
    public interface IDataService : IDisposable
    {
        public Task<bool> HomeExistsAsync(string homeId, CancellationToken cancel = default);
        public Task<Models.Home?> AddHomeAsync(Models.Home home, CancellationToken cancel = default);
        public Task<Models.Home?> GetHomeAsync(string homeId, CancellationToken cancel = default);

        public Task<T?> GetAsync<T>(string homeId, string storeName, CancellationToken cancel = default);
        public Task<T?> GetAsync<T>(Models.DataRequest request, CancellationToken cancel = default);
        public Task SetAsync(string homeId, string storeName, object? data, CancellationToken cancel = default);
        public Task SetAsync(Models.DataRequest request, object? data, CancellationToken cancel = default);

        public Task<List<Models.DataRequestInfo>> ListAsync(string homeId, string actionName, int maxResults = 0, CancellationToken cancel = default);
        public Task<List<Models.DataRequestInfo>> ListAsync(Models.DataRequest request, int maxResults = 0, CancellationToken cancel = default);
    }

    public class FileDataService : IDataService
    {
        private const string fileSearch = "*.json";
        private const string homeFile = "home.json";
        private const int bufferSize = 8192;
        private readonly string dataPath;
        private readonly Serilog.ILogger _log;

        public FileDataService(IOptions<DataServiceConfig> options) //ILogger<FileDataService> dataLogger, 
        {
            _log = Serilog.Log.Logger;
            // need to be able to define the base (for different types of data)
            // also need to have a better locking strategy for updates (none right now)
            var path = options.Value.DataPath;
            if (Utils.IsLinux)
            {
                path = options.Value.DataPathLinux;
            }
            dataPath = path ?? string.Empty;
        }

        private string GetFilePath(in string homeId, in string fileName)
        {
            // ?? should we ToLower the parameters to make misses less likely ??
            return Path.Combine(dataPath, homeId, $"{fileName}.json");
        }
        private FileInfo GetTypePath(in Models.DataRequest request)
        {
            return new FileInfo(
                Path.Combine(dataPath, request.HomeId, request.ActionName, $"{request.StoreName}.json")
            );
        }

        public Task<bool> HomeExistsAsync(string homeId, CancellationToken cancel = default)
        {
            return Task.FromResult(Directory.Exists(Path.Combine(dataPath, homeId)));
        }
        public async Task<Models.Home?> AddHomeAsync(Models.Home home, CancellationToken cancel = default) // should the home be defined prior to add?
        {
            var debug = _log.IsEnabled(Serilog.Events.LogEventLevel.Debug);
            if (debug)
            {
                _log.Debug("AddHome init: {@home}", home);
            }
            if (string.IsNullOrWhiteSpace(home.Id)) throw new ArgumentNullException("ID required");

            var path = Path.Combine(dataPath, home.Id);
            // this should NOT be returned to the user even though it should never actually happen
            if (Directory.Exists(path)) throw new ArgumentOutOfRangeException("Home already exists");

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "bak"));
            //Directory.CreateDirectory(Path.Combine(path, "trip"));

            var hfile = Path.Combine(path, homeFile);
            using var file = new FileStream(hfile, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize, true);
            await JsonSerializer.SerializeAsync(file, home); //, jsonOptions, cancel);

            if (debug)
            {
                _log.Debug("AddHome return: {@home}", home);
            }
            return home;
        } // END AddHomeAsync
        public async Task<Models.Home?> GetHomeAsync(string homeId, CancellationToken cancel = default)
        {
            var path = Path.Combine(dataPath, homeId, homeFile);
            if (!File.Exists(path))
            {
                return default(Models.Home);
            }
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            return await JsonSerializer.DeserializeAsync<Models.Home>(file);
        } // END GetHomeAsync

        public async Task<T?> GetAsync<T>(string homeId, string storeName, CancellationToken cancel = default)
        {
            var path = GetFilePath(homeId, storeName);
            if (!File.Exists(path))
            {
                return default(T);
            }
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            return await JsonSerializer.DeserializeAsync<T>(file); //, jsonOptions, cancel);
        }
        public async Task<T?> GetAsync<T>(Models.DataRequest request, CancellationToken cancel = default)
        {
            var info = GetTypePath(request);
            if (info.Directory == null || !info.Directory.Exists)
            {
                throw new ArgumentOutOfRangeException($"Request path ({info.FullName}) NOT valid");
            }
            if (!File.Exists(info.FullName))
            {
                return default;
            }
            using var file = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            return await JsonSerializer.DeserializeAsync<T>(file); //, jsonOptions, cancel);
        } // END GetAsync

        public async Task SetAsync(string homeId, string storeName, object? data, CancellationToken cancel = default)
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

        public async Task SetAsync(Models.DataRequest request, object? data, CancellationToken cancel = default)
        {
            request.StoreName += Utils.GetNewId();
            var info = GetTypePath(request);
            if (info.Directory == null)
            {
                var fullName = info.FullName;
                var pathName = Path.GetDirectoryName(fullName);
                if (pathName == null)
                    throw new ArgumentOutOfRangeException(nameof(request), $"Storage path ({fullName}) could NOT be found");
                Directory.CreateDirectory(pathName);
                info = new FileInfo(fullName);
                if (info.Directory == null)
                    throw new ArgumentOutOfRangeException(nameof(request), $"Storage path ({fullName}) could NOT be found");
            }
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }
            // ?? backup/archive ??
            using var file = new FileStream(info.FullName, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize, true);
            await JsonSerializer.SerializeAsync(file, data); //, jsonOptions, cancel);
        } // END SetAsync

        public Task<List<Models.DataRequestInfo>> ListAsync(string homeId, string actionName, int maxResults = 0, CancellationToken cancel = default) =>
                                            ListAsync(new Models.DataRequest { HomeId = homeId, ActionName = actionName, }, maxResults, cancel);
        public Task<List<Models.DataRequestInfo>> ListAsync(Models.DataRequest request, int maxResults = 0, CancellationToken cancel = default)
        {
            if (maxResults < 0) maxResults = 0;
            var list = new List<Models.DataRequestInfo>();

            var path = Path.Combine(dataPath, request.HomeId, request.ActionName);
            if (!Directory.Exists(path)) return Task.FromResult(list);

            var files = Directory.EnumerateFiles(path, fileSearch); //.GetFiles(fileSearch);
            foreach (var f in files)
            {
                var rqst = new Models.DataRequestInfo
                {
                    HomeId = request.HomeId,
                    ActionName = request.ActionName,
                    StoreName = Path.GetFileNameWithoutExtension(f),
                    CreatedTime = File.GetLastWriteTimeUtc(f),
                };
                list.Add(rqst);

                if (maxResults != 0 && list.Count >= maxResults) break;
            }

            return Task.FromResult(list);
        } // END ListAsync

        #region cleanup
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) { }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~FileDataService()
        {
            Dispose(false);
        }
        #endregion cleanup
    }
}
