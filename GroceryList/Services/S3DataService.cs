using Amazon.S3;
using Amazon.S3.Model;
using GroceryList.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace GroceryList.Services
{
    public class S3DataService : IDataService
    {
        private const string homeFile = "home.json";

        private readonly ILogger<S3DataService> logger;
        private readonly AmazonS3Client client;

        public S3DataService(ILogger<S3DataService> dataLogger, IOptions<Models.Config.DataServiceConfig> options)
        {
            logger = dataLogger;
            //dataPath = options.Value.DataPath
            // setup AWS_PROFILE & AWS_REGION in Environment
            client = new AmazonS3Client();
        }

        public async Task<bool> HomeExistsAsync(string homeId)
        {
            try
            {
                using var response = await client.GetObjectAsync(homeId, homeFile);
                return response.ContentLength != 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "HomeExistsAsync ({homeId} Error)", homeId);
            }
            return false;
        }
        public async Task<Home> AddHomeAsync(Home home) // should the home be defined prior to add?
        {
            if (string.IsNullOrWhiteSpace(home.Id)) throw new ArgumentNullException("ID required");

            // this should NOT be returned to the user even though it should never actually happen
            if (await HomeExistsAsync(home.Id)) throw new ArgumentOutOfRangeException("Home already exists");

            await client.PutBucketAsync(home.Id);
            await client.PutBucketAsync(home.Id + "/bak");

            var request = new PutObjectRequest
            {
                BucketName = home.Id,
                Key = homeFile,
            };
            await JsonSerializer.SerializeAsync(request.InputStream, home);
            var response = await client.PutObjectAsync(request);

            return response.HttpStatusCode == System.Net.HttpStatusCode.OK ? home : null;
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
            using var file = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
            return await JsonSerializer.DeserializeAsync<T>(file); //, jsonOptions, cancel);
        } // END GetAsync

        public async Task SetAsync(string homeId, string storeName, object data)
        {
            var path = GetFilePath(homeId, storeName);
            // ?? backup/archive ??
            if (File.Exists(path))
            {
                var bak = GetTypePath(new Models.DataRequest
                {
                    HomeId = homeId,
                    StoreName = storeName + GetNewId(),
                    ActionName = "bak"
                });
                using var readFile = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 8192, true);
                using var bakFile = new FileStream(bak.FullName, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, true);
                await readFile.CopyToAsync(bakFile);
                // reset position?
                readFile.SetLength(0);
                //File.Move(path, bak);
            }
            if (data != null)
            {
                using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, true);
                await JsonSerializer.SerializeAsync(file, data); //, jsonOptions, cancel);
            }
            else
            {
                File.Delete(path);
            }
        } // END SetAsync

        public async Task SetAsync(Models.DataRequest request, object data)
        {
            request.StoreName += GetNewId();
            var info = GetTypePath(request);
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }
            // ?? backup/archive ??
            using var file = new FileStream(info.FullName, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, true);
            await JsonSerializer.SerializeAsync(file, data); //, jsonOptions, cancel);
        } // END SetAsync

        #region cleanup
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                client.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~S3DataService()
        {
            Dispose(false);
        }
        #endregion cleanup
    }
}
