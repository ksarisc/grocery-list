using Dapper;
using GroceryList.Mvc.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public class UserRepository : IUserStore<AppUser>
    {
        private const string table = "app_user";
        private const string create = "CREATE TABLE " + table + " (name VARCHAR(255) NOT NULL, brand VARCHAR(255), requested_at DATETIME, created_at VARCHAR(255) NOT NULL, created_by DATETIME NOT NULL);";

        private const string whereId = "id = @Id";
        private const string select = "SELECT id Id, home_id HomeId, email Email, first_name FirstName, last_name LastName, created_at CreatedTime FROM " + table + " /**where**/";
        private const string insert = "INSERT INTO " + table + " (id, home_id, email, first_name, last_name, created_at, confirmed) VALUES (@Id, @HomeId, @Email, @FirstName, @LastName, @CreatedTime, @Confirmed);";
        private const string update = "UPDATE " + table + " SET home_id = @HomeId, first_name = @FirstName, last_name = @LastName, confirmed = @Confirmed WHERE " + whereId + ";";
        private const string delete = "DELETE FROM " + table + " WHERE " + whereId + ";";

        private readonly DataService data;

        public UserRepository(DataService dataService)
        {
            data = dataService;
        }

        public async Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            if (!user.Id.HasValue) {
                user.Id = Guid.NewGuid();
            }
            if (!user.CreatedTime.HasValue) {
                user.CreatedTime = DateTime.UtcNow;
            }

            using (var conn = data.GetConnection())
            {
                await conn.OpenAsync(cancel);
                await conn.ExecuteAsync(insert, user);
            }

            return IdentityResult.Success;
        } // END CreateAsync

        public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            using (var conn = data.GetConnection())
            {
                await conn.OpenAsync(cancel);
                await conn.ExecuteAsync(update, user);
            }

            return IdentityResult.Success;
        } // END UpdateAsync

        public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            using (var conn = data.GetConnection())
            {
                await conn.OpenAsync(cancel);
                await conn.ExecuteAsync(delete, user);
            }

            return IdentityResult.Success;
        } // END DeleteAsync

        private async Task<AppUser> Find(string where, DynamicParameters parameters, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            using (var conn = data.GetConnection())
            {
                await conn.OpenAsync(cancel);
                var builder = new SqlBuilder();
                builder.Where(where, parameters);
                var template = builder.AddTemplate(select);;
                return await conn.QuerySingleOrDefaultAsync<AppUser>(
                    template.RawSql, template.Parameters);
            }
        } // END Find

        public async Task<AppUser> FindByIdAsync(string userId, CancellationToken cancel)
        {
            var parms = new DynamicParameters();
            parms.Add("@Id", userId);
            return await Find(whereId, parms, cancel);
        } // END FindByIdAsync
        public async Task<AppUser> FindByIdAsync(Guid userId, CancellationToken cancel)
        {
            return await FindByIdAsync(userId.ToId(), cancel);
        } // END FindByIdAsync

        public async Task<AppUser> FindByEmailAsync(string email, CancellationToken cancel)
        {
            var parms = new DynamicParameters();
            parms.Add("@Email", email);
            return await Find("email = @Email", parms, cancel);
        } // END FindByEmailAsync

        // normalizedUserName
        public async Task<AppUser> FindByNameAsync(string userName, CancellationToken cancel)
        {
            return await FindByEmailAsync(userName, cancel);
        } // END FindByEmailAsync

        public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancel)
        {
            return Task.FromResult(user.GetId());
        }

        public Task<string> GetUserNameAsync(AppUser user, CancellationToken cancel)
        {
            return Task.FromResult(user.Email); //.UserName);
        }

        public Task SetUserNameAsync(AppUser user, string userName, CancellationToken cancel)
        {
            //user.UserName = userName;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancel)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(AppUser user, string normalizedName, CancellationToken cancel)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(AppUser user, string email, CancellationToken cancel)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(AppUser user, CancellationToken cancel)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken cancel)
        {
            return Task.FromResult(user.Confirmed);
        }

        public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancel)
        {
            user.Confirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedEmailAsync(AppUser user, CancellationToken cancel)
        {
            return Task.FromResult(user.Email); // NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(AppUser user, string normalizedEmail, CancellationToken cancel)
        {
            //user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }

        // public Task<string> GetPhoneNumberAsync(AppUser user, CancellationToken cancel)
        // {
        //     return Task.FromResult(user.PhoneNumber);
        // }

        // public Task SetPhoneNumberAsync(AppUser user, string phoneNumber, CancellationToken cancel)
        // {
        //     user.PhoneNumber = phoneNumber;
        //     return Task.FromResult(0);
        // }

        // public Task<bool> GetPhoneNumberConfirmedAsync(AppUser user, CancellationToken cancel)
        // {
        //     return Task.FromResult(user.PhoneNumberConfirmed);
        // }

        // public Task SetPhoneNumberConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancel)
        // {
        //     user.PhoneNumberConfirmed = confirmed;
        //     return Task.FromResult(0);
        // }

        // public Task SetTwoFactorEnabledAsync(AppUser user, bool enabled, CancellationToken cancel)
        // {
        //     user.TwoFactorEnabled = enabled;
        //     return Task.FromResult(0);
        // }

        // public Task<bool> GetTwoFactorEnabledAsync(AppUser user, CancellationToken cancel)
        // {
        //     return Task.FromResult(user.TwoFactorEnabled);
        // }

        // public Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken cancel)
        // {
        //     user.PasswordHash = passwordHash;
        //     return Task.FromResult(0);
        // }

        // public Task<string> GetPasswordHashAsync(AppUser user, CancellationToken cancel)
        // {
        //     return Task.FromResult(user.PasswordHash);
        // }

        // public Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancel)
        // {
        //     return Task.FromResult(user.PasswordHash != null);
        // }

        public void Dispose() { }
    }
}
