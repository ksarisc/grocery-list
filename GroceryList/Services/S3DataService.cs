using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using GroceryList.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace GroceryList.Services
{
    public class S3DataService : IDataService
    {
        private const string homeFile = "home.json";

        private readonly ILogger<S3DataService> logger;
        private readonly IAmazonS3 client;

        public S3DataService(ILogger<S3DataService> dataLogger, IOptions<Models.Config.DataServiceConfig> options)
        {
            logger = dataLogger;
            //dataPath = options.Value.DataPath
            // setup AWS_PROFILE & AWS_REGION in Environment
            // otherwise configuration required here
            client = new AmazonS3Client();
        }

        private static string GetPath(Models.DataRequest request)
        {
            return GetPath(request.HomeId, request.ActionName);
        }
        private static string GetPath(string homeId, string actionName)
        {
            return homeId + "/" + actionName;
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

        private async Task<T> ParseAsync<T>(string filePath, string fileName)
        {
            try
            {
                using var response = await client.GetObjectAsync(filePath, $"{fileName}.json");
                return await JsonSerializer.DeserializeAsync<T>(response.ResponseStream);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get (Path:{filePath}|File: {fileName}) Error", filePath, fileName);
            }
            return default(T);
        }

        public async Task<T> GetAsync<T>(string homeId, string storeName)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Get Start: (Home:{homeId}|Store: {storeName})", homeId, storeName);
            }
            return await ParseAsync<T>(homeId, storeName);
        }
        public async Task<T> GetAsync<T>(Models.DataRequest request)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Get Start: (Request:{request})", request);
            }

            var path = GetPath(request);
            // check bucket
            if (await AmazonS3Util.DoesS3BucketExistV2Async(client, path))
            {
                return await ParseAsync<T>(path, request.StoreName);
            }
            throw new ArgumentOutOfRangeException($"Request path ({path}) NOT valid");
        } // END GetAsync

        private async Task MoveAsync(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey)
        {
            try
            {
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = sourceBucket,
                    SourceKey = sourceKey,
                    DestinationBucket = destinationBucket,
                    DestinationKey = destinationBucket
                };
                var response = await client.CopyObjectAsync(copyRequest);

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = sourceBucket,
                    Key = sourceKey
                };
                var delete = await client.DeleteObjectAsync(deleteRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Move (Source: {sourceBucket}|{sourceKey})(Destination: {destinationBucket}|{destinationKey}) Error",
                    sourceBucket, sourceKey, destinationBucket, destinationKey);
            }
        } // END MoveAsync

        public async Task SetAsync(string homeId, string storeName, object data)
        {
            //var path = GetFilePath(homeId, storeName);
            var fname = $"{storeName}.json";
            // ?? backup/archive ??
            var meta = await client.GetObjectMetadataAsync(homeId, fname);
            if (meta.ContentLength > 0)
            {
                var bakPath = GetPath(homeId, "bak");
                var bakName = $"{storeName}{DataService.GetNewId()}.json";
                await MoveAsync(homeId, fname, bakPath, bakName);
            }
            if (data != null)
            {
                var rqst = new PutObjectRequest
                {
                    BucketName = homeId,
                    Key = fname,
                };
                await JsonSerializer.SerializeAsync(rqst.InputStream, data);
                var response = await client.PutObjectAsync(rqst);
            }
        } // END SetAsync

        public async Task SetAsync(Models.DataRequest request, object data)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("", request, data);
            }
            try
            {
                request.StoreName += DataService.GetNewId();
                var path = GetPath(request);
                if (!(await AmazonS3Util.DoesS3BucketExistV2Async(client, path)))
                {
                    var bucket = await client.PutBucketAsync(path);
                }
                // ?? backup/archive ??
                var rqst = new PutObjectRequest
                {
                    BucketName = path,
                    Key = $"{request.StoreName}.json",
                };
                await JsonSerializer.SerializeAsync(rqst.InputStream, data);
                var response = await client.PutObjectAsync(rqst);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Set () Error", request, data);
            }
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
