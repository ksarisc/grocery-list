using Dapper;
using GroceryList.Mvc.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public class RoleRepository : IRoleStore<AppRole>
    {
        private const string table = "app_role";
        private const string create = "CREATE TABLE " + table + " (name VARCHAR(255) NOT NULL, brand VARCHAR(255), requested_at DATETIME, created_at VARCHAR(255) NOT NULL, created_by DATETIME NOT NULL);";

        private const string whereId = "id = @Id";
        // Id, Name, Normalization
        private const string select = "SELECT id Id, home_id HomeId, email Email, first_name FirstName, last_name LastName, created_at CreatedTime FROM " + table + " /**where**/";
        private const string insert = "INSERT INTO " + table + " (id, home_id, email, first_name, last_name, created_at, confirmed) VALUES (@Id, @HomeId, @Email, @FirstName, @LastName, @CreatedTime, @Confirmed);";
        private const string update = "UPDATE " + table + " SET home_id = @HomeId, first_name = @FirstName, last_name = @LastName, confirmed = @Confirmed WHERE " + whereId + ";";
        private const string delete = "DELETE FROM " + table + " WHERE " + whereId + ";";

        private readonly DataService data;

        public RoleRepository(DataService dataService)
        {
            data = dataService;
        }

        public async Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancel)
        {
            await data.ExecuteAsync(insert, role, cancel);

            return IdentityResult.Success;
        } // END CreateAsync

        public async Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancel)
        {
            await data.ExecuteAsync(update, role, cancel);

            return IdentityResult.Success;
        } // END UpdateAsync

        public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancel)
        {
            await data.ExecuteAsync(delete, role, cancel);

            return IdentityResult.Success;
        } // END DeleteAsync

        public async Task<AppRole> FindByIdAsync(string roleId, CancellationToken cancel)
        {
            var parms = new DynamicParameters();
            parms.Add("@Id", roleId);
            var builder = new SqlBuilder();
            builder.Where(whereId, parms);
            var template = builder.AddTemplate(select);
            return await data.QuerySingleAsync<AppRole>(template, cancel);
        }

        public async Task<AppRole> FindByNameAsync(string roleName, CancellationToken cancel)
        {
            var parms = new DynamicParameters();
            parms.Add("@Name", roleName);
            var builder = new SqlBuilder();
            builder.Where("name = @Name", parms);
            var template = builder.AddTemplate(select);
            return await data.QuerySingleAsync<AppRole>(template, cancel);
        }

        public Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancel)
        {
            return Task.FromResult(role.Id.ToString());
        }
        public Task<string> GetRoleNameAsync(AppRole role, CancellationToken cancel)
        {
            return Task.FromResult(role.Name);
        }
        public Task SetRoleNameAsync(AppRole role, string roleName, CancellationToken cancel)
        {
            role.Name = roleName;
            return Task.FromResult(0);
        }
        public Task<string> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancel)
        {
            return Task.FromResult(String.Empty);
        }
        public Task SetNormalizedRoleNameAsync(AppRole role, string normalName, CancellationToken cancel)
        {
            //role.Name = normalName;
            return Task.FromResult(0);
        }

        public void Dispose() { }
    }
}
