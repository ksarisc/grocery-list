using Dapper;
using GroceryList.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
//using static Humanizer.In;

namespace GroceryList.Data
{
    public sealed class MySQLDataService: Services.IDataService
    {
        private readonly string _connect;
        private readonly ILogger<MySQLDataService> _log;

        public MySQLDataService(IConfiguration configuration, ILogger<MySQLDataService> dataLogger) //IResourceMapper resourceMapper, 
        {
            _connect = configuration.GetConnectionWithSecrets("Main");
            //map = resourceMapper;
            _log = dataLogger;
        }

        private const string _sqlExists = @"SELECT `home_id` FROM `home` WHERE `home_id` = @HomeId OR `identifier` = @Identifier;";
        public async Task<bool> HomeExistsAsync(string homeId, CancellationToken cancel = default)
        {
            int found = 0;
            try
            {
                var def = new CommandDefinition(_sqlExists, new { HomeId = homeId, Identifier = homeId }, cancellationToken: cancel);
                await using var conn = new MySqlConnection(_connect);
                await conn.OpenAsync(cancel);
                found = await conn.QueryFirstOrDefaultAsync<int?>(def) ?? 0;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Home Exists ({homeId}) ERRORED", homeId);
            }
            return found != 0;
        }

        //`home_id`, `identifier`, `name`, `created_by`, `created_time`, `created_by_meta`, `description`, `primary_user` //, @PrimaryUser
        private const string _sqlAdd = @"INSERT INTO `home`
    (`slug`, `label`, `email`, `timezone`, `created_by`, `created_on`, `created_by_meta`, `notes`)
VALUES (@Id, @Title, @Email, @Timezone, @CreatedBy, @CreatedTime, @CreatedByMeta, @Description);";
        public async Task<Models.Home?> AddHomeAsync(Models.Home home, CancellationToken cancel = default)
        {
            try
            {
                await using var conn = new MySqlConnection(_connect);
                var result = await conn.ExecuteAsync(_sqlAdd, home);

                // create the current & history list after home record is created
                return await conn.QueryFirstOrDefaultAsync<Models.Home?>(_sqlGet, new { HomeId = home.Id, Identifier = home.Id });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Home Exists ({homeId}) ERRORED", home.Id);
            }
            return null;
        }

        private const string _sqlGet = @"SELECT `home_id` Identity, `identifier` Id, `name` Title, `` CreatedBy, `` CreatedTime, `` CreatedByMeta
-- `description` VARCHAR(2000), `primary_user` VARCHAR(50)
FROM `home`
WHERE `home_id` = @HomeId OR `identifier` = @Identifier;";
        public async Task<Models.Home?> GetHomeAsync(string homeId, CancellationToken cancel = default)
        {
            try
            {
                await using var conn = new MySqlConnection(_connect);
                return await conn.QueryFirstOrDefaultAsync<Models.Home?>(_sqlGet, new { HomeId = homeId, Identifier = homeId });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Home ({homeId}) ERRORED", homeId);
            }
            throw new KeyNotFoundException($"Home ({homeId}) NOT Found");
        }

        public Task<T?> GetAsync<T>(string homeId, string storeName, CancellationToken cancel = default) => throw new NotImplementedException();
        public Task<T?> GetAsync<T>(Models.DataRequest request, CancellationToken cancel = default) => throw new NotImplementedException();
        public Task SetAsync(string homeId, string storeName, object? data, CancellationToken cancel = default) => throw new NotImplementedException();
        public Task SetAsync(Models.DataRequest request, object? data, CancellationToken cancel = default) => throw new NotImplementedException();

        public Task<List<Models.DataRequestInfo>> ListAsync(string homeId, string actionName, int maxResults = 0, CancellationToken cancel = default) => throw new NotImplementedException();
        public Task<List<Models.DataRequestInfo>> ListAsync(Models.DataRequest request, int maxResults = 0, CancellationToken cancel = default) => throw new NotImplementedException();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
