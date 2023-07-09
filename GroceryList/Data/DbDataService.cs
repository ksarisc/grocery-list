using Amazon.S3.Model.Internal.MarshallTransformations;
using Dapper;
using GroceryList.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using static Humanizer.In;

namespace GroceryList.Data
{
    public sealed class DbDataService: Services.IDataService
    {
        private readonly string connect;
        private readonly DbProviderFactory factory;
        //private readonly IResourceMapper map;
        private readonly ILogger<DbDataService> logger;

        public DbDataService(DbProviderFactory dbProviderFactory, IConfiguration configuration, ILogger<DbDataService> dataLogger) //IResourceMapper resourceMapper, 
        {
            factory = dbProviderFactory;
            connect = configuration.GetConnectionWithSecrets("Main");
            //map = resourceMapper;
            logger = dataLogger;
        }

        private DbConnection GetConnection()
        {
            var conn = factory.CreateConnection();
            if (conn == null)
                throw new NullReferenceException("Invalid DbConnection Factory result");
            conn.ConnectionString = connect;
            return conn;
        }

        private const string sqlExists = @"SELECT `home_id` FROM `homes` WHERE `home_id` = @HomeId OR `identifier` = @Identifier;";
        public async Task<bool> HomeExistsAsync(string homeId)
        {
            int found = 0;
            try
            {
                var conn = GetConnection();
                found = await conn.QueryFirstOrDefaultAsync<int?>(sqlExists, new { HomeId = homeId, Identifier = homeId }) ?? 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Home Exists ({homeId}) ERRORED", homeId);
            }
            return found != 0;
        }

        private const string sqlInsert = @"INSERT INTO `homes`
    (`home_id`, `identifier`, `name`, `created_by`, `created_time`, `created_by_meta`, `description`, `primary_user`)
VALUES (@Identity, @Id, @Title, @CreatedBy, @CreatedTime, @CreatedByMeta, @Description, @PrimaryUser);";
        public async Task<Models.Home?> AddHomeAsync(Models.Home home)
        {
            try
            {
                var conn = GetConnection();
                var result = await conn.ExecuteAsync(sqlInsert, home);

                return await conn.QueryFirstOrDefaultAsync<Models.Home?>(sqlGet, new { HomeId = home.Id, Identifier = home.Id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Home Exists ({homeId}) ERRORED", home.Id);
            }
            return null;
        }

        private const string sqlGet = @"SELECT `home_id` Identity, `identifier` Id, `name` Title, `` CreatedBy, `` CreatedTime, `` CreatedByMeta
-- `description` VARCHAR(2000), `primary_user` VARCHAR(50)
FROM `homes`
WHERE `home_id` = @HomeId OR `identifier` = @Identifier;";
        public async Task<Models.Home?> GetHomeAsync(string homeId)
        {
            try
            {
                var conn = GetConnection();
                return await conn.QueryFirstOrDefaultAsync<Models.Home?>(sqlGet, new { HomeId = homeId, Identifier = homeId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Home ({homeId}) ERRORED", homeId);
            }
            throw new KeyNotFoundException($"Home ({homeId}) NOT Found");
        }

        public Task<T?> GetAsync<T>(string homeId, string storeName) => throw new NotImplementedException();
        public Task<T?> GetAsync<T>(Models.DataRequest request) => throw new NotImplementedException();
        public Task SetAsync(string homeId, string storeName, object? data) => throw new NotImplementedException();
        public Task SetAsync(Models.DataRequest request, object? data) => throw new NotImplementedException();

        public Task<List<Models.DataRequestInfo>> ListAsync(string homeId, string actionName, int maxResults = 0) => throw new NotImplementedException();
        public Task<List<Models.DataRequestInfo>> ListAsync(Models.DataRequest request, int maxResults = 0) => throw new NotImplementedException();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
