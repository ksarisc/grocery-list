using GroceryList.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public class RoleRepository : IRoleStore<AppRole>
    {
        private const string lookupFile = "lookup_role_data";

        private readonly Services.IDataService fileService;
        private readonly ILogger<RoleRepository> logger;

        private readonly string folder;

        public RoleRepository(Services.IDataService dataService, ILogger<RoleRepository> roleLogger, IConfiguration configuration)
        {
            fileService = dataService;
            logger = roleLogger;
            folder = configuration.GetValue<string>("Role::BasePath");
        }

        private async Task<AppRole> GetAsync(string roleId)
        {
            var role = await fileService.GetAsync<AppRole>(folder, roleId);
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

            await fileService.SetAsync(folder, role.Id, dataRole);

            return IdentityResult.Success;
        } // END UpdateAsync

        public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dataRole = await GetAsync(role.Id);
            // back it up & delete
            await fileService.SetAsync(folder, role.Id, null);

            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(AppRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(AppRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.FromResult(0);
        }

        public async Task<AppRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await GetAsync(roleId);
        }

        public async Task<AppRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // find role's ID via lookup
            var list = await fileService.GetAsync<AppRoleLookup[]>(folder, lookupFile);
            var role = list?.FirstOrDefault(u => u.Name.Equals(normalizedRoleName, StringComparison.OrdinalIgnoreCase));

            if (role == null || string.IsNullOrEmpty(role.Id)) return AppRole.Empty;

            return await GetAsync(role.Id);
        }

        public void Dispose() { }
    }
}
