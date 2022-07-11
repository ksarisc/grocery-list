using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
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
        private const string fileExt = ".json";
        private const string homeFile = "home" + fileExt;

        private readonly ILogger<S3DataService> logger;
        private readonly IAmazonS3 client;

        public S3DataService(ILogger<S3DataService> dataLogger, IOptions<Models.Config.DataServiceConfig> options)
        {
            // Need to consider rate limits
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
        public async Task<Models.Home> GetHomeAsync(string homeId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("GetHomeAsync Start: (Home:{homeId})", homeId);
            }
            return await ParseAsync<Models.Home>(homeId, "home");
        }

        private async Task<T> ParseAsync<T>(string filePath, string fileName)
        {
            try
            {
                using var response = await client.GetObjectAsync(filePath, $"{fileName}{fileExt}");
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
            var fname = $"{storeName}{fileExt}";
            // ?? backup/archive ??
            var meta = await client.GetObjectMetadataAsync(homeId, fname);
            if (meta.ContentLength > 0)
            {
                var bakPath = GetPath(homeId, "bak");
                var bakName = $"{storeName}{Utils.GetNewId()}{fileExt}";
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
                request.StoreName += Utils.GetNewId();
                var path = GetPath(request);
                if (!(await AmazonS3Util.DoesS3BucketExistV2Async(client, path)))
                {
                    var bucket = await client.PutBucketAsync(path);
                }
                // ?? backup/archive ??
                var rqst = new PutObjectRequest
                {
                    BucketName = path,
                    Key = $"{request.StoreName}{fileExt}",
                };
                await JsonSerializer.SerializeAsync(rqst.InputStream, data);
                var response = await client.PutObjectAsync(rqst);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Set () Error", request, data);
            }
        } // END SetAsync

        private static string Combine(string path1, string path2)
        {
            var endsWith = path1.EndsWith('/');
            var startsWith = path2.StartsWith('/');
            if (endsWith && startsWith)
                return path1 + path2[..(path2.Length - 1)];
            if (endsWith || startsWith)
                return path1 + path2;
            return path1 + "/" + path2;
        }

        public Task<List<Models.DataRequestInfo>> ListAsync(string homeId, string actionName, int maxResults = 0) =>
                                            ListAsync(new Models.DataRequest { HomeId = homeId, ActionName = actionName, }, maxResults);
        public async Task<List<Models.DataRequestInfo>> ListAsync(Models.DataRequest request, int maxResults = 0)
        {
            if (maxResults < 0) maxResults = 0;
            var list = new List<Models.DataRequestInfo>();

            var path = Combine(request.HomeId, request.ActionName);
            if (!(await AmazonS3Util.DoesS3BucketExistV2Async(client, path)))
            {
                return list;
            }

            //client.ListBucketsAsync()
            var listRqst = new ListObjectsRequest
            {
                BucketName = path,
                //Delimiter = "",
            };
            //listRqst.Encoding
            var files = await client.ListObjectsAsync(listRqst);
            //files.HttpStatusCode
            foreach (var f in files.S3Objects)
            {
                // ignore non-data files
                if (!f.Key.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase)) continue;

                var rqst = new Models.DataRequestInfo
                {
                    HomeId = request.HomeId,
                    ActionName = request.ActionName,
                    StoreName = f.Key[..(f.Key.Length - fileExt.Length)],
                    CreatedTime = f.LastModified,
                };
                list.Add(rqst);

                if (maxResults != 0 && list.Count >= maxResults) break;
            }

            return list;
        } // END ListAsync

        #region cleanup
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            client.Dispose();
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
