using Amazon.Runtime.Internal.Util;
using Amazon.S3.Model;
using Dapper;
using GroceryList.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public class RoleRepository : IRoleStore<AppRole>, IDisposable
    {
        private const string _lookupFile = "lookup_role_data";

        private readonly Services.IDataService _data;
        private readonly Serilog.ILogger _log;

        private readonly string _folder;

        public RoleRepository(Services.IDataService dataService, IConfiguration configuration) //, ILogger<RoleRepository> roleLogger
        {
            _log = Serilog.Log.Logger;
            _data = dataService;
            var folder = configuration.GetValue<string>("Role::BasePath");
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentNullException(nameof(folder), "Role.BasePath configuration MISSING");
            }
            _folder = folder;
        }

        private async Task<AppRole> GetAsync(string roleId)
        {
            var role = await _data.GetAsync<AppRole>(_folder, roleId);
            return role ?? AppRole.Empty;
        }

        public Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancellationToken)
        {
            return UpdateAsync(role, cancellationToken);
        } // END CreateAsync

        public async Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ID value MUST be set
            if (string.IsNullOrWhiteSpace(role.Id))
            {
                role.Id = Utils.GetNewUuid();
            }

            // is get needed at all, since we're just setting is at the end
            var dataRole = await GetAsync(role.Id);
            if (dataRole != null)
            {
                dataRole.Name = role.Name;
                dataRole.NormalizedName = role.NormalizedName;
                // don't change Created Time
            }
            else
            {
                dataRole = role;
                dataRole.CreatedTime = DateTimeOffset.UtcNow;
            }
            dataRole.EditedTime = DateTimeOffset.UtcNow;

            await _data.SetAsync(_folder, role.Id, dataRole);

            return IdentityResult.Success;
        } // END UpdateAsync

        public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dataRole = await GetAsync(role.Id);
            // back it up & delete
            await _data.SetAsync(_folder, role.Id, null);

            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string?> GetRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(role.Name);
        }

        public Task SetRoleNameAsync(AppRole role, string? roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName ?? string.Empty;
            return Task.CompletedTask; //Task.FromResult(0);
        }

        public Task<string?> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(AppRole role, string? normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<AppRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await GetAsync(roleId);
        }

        public async Task<AppRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // find role's ID via lookup
            var list = await _data.GetAsync<AppRoleLookup[]>(_folder, _lookupFile);
            var role = list?.FirstOrDefault(u => u.Name.Equals(normalizedRoleName, StringComparison.OrdinalIgnoreCase));

            if (role == null || string.IsNullOrEmpty(role.Id)) return AppRole.Empty;

            return await GetAsync(role.Id);
        }

        public void Dispose() { GC.SuppressFinalize(this); }
    }

    public sealed class DbRoleRepository : IRoleStore<AppRole>, IDisposable
    {
        private readonly DbConnection _conn;
        private readonly Serilog.ILogger _log;

        public DbRoleRepository(DbProviderFactory providerFactory, IConfiguration configuration) //, ILogger<DbRoleRepository> userLogger)
        {
            _log = Serilog.Log.Logger;
            var connect = configuration.GetConnectionWithSecrets("Main");
            _conn = providerFactory.CreateConnection() ?? throw new NullReferenceException("Main factory generated NO valid connection");
            _conn.ConnectionString = connect;
        }

        private const string sqlGet = @"";
        private async Task<AppRole> GetAsync(string? roleId = null, string? normalName = null)
        {
            try
            {
                var role = await _conn.QueryFirstOrDefaultAsync(sqlGet, new { RoleId = roleId, NormalName = normalName, });
                if (role != null)
                    return role;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Get Role (ID:{roleId})(Name:{normalName}) ERR", roleId, normalName);
            }
            //throw new KeyNotFoundException($"Role `{roleId}` NOT Found")
            return AppRole.Empty;
        }

        private const string sqlInsert = @"INSERT INTO app";
        private const string sqlUpdate = @"UPDATE app";
        public async Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await conn.ExecuteAsync(sqlInsert, role);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Create Role ERR ({@role})", role);
            }
            return IdentityResult.Failed(new IdentityError { Description = $"Unable to create Role: {role.Name}" });
        } // END CreateAsync

        public async Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //// ID value MUST be set
            //if (string.IsNullOrWhiteSpace(role.Id)){
            //    role.Id = Utils.GetNewUuid();
            //}

            try
            {
                var result = await _conn.ExecuteAsync(sqlUpdate, role);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Update Role ERR ({@role})", role);
            }
            return IdentityResult.Failed(new IdentityError { Description = $"Unable to update Role: {role.Name}" });
        } // END UpdateAsync

        private const string sqlDelete = @"";
        public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await _conn.ExecuteAsync(sqlDelete, role);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Delete Role ERR ({@role})", role);
            }
            return IdentityResult.Failed(new IdentityError { Description = $"Unable to delete Role: {role.Name}" });
        } // END DeleteAsync

        public Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string?> GetRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(role.Name);
        }

        public Task SetRoleNameAsync(AppRole role, string? roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName ?? string.Empty;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(AppRole role, string? normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName ?? string.Empty;
            return Task.CompletedTask;
        }

        public async Task<AppRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await GetAsync(roleId: roleId);
        }

        public async Task<AppRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await GetAsync(normalName: normalizedRoleName);
        }

        public void Dispose() { GC.SuppressFinalize(this); }
    }
}
