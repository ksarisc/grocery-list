using Dapper;
using GroceryList.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Data.Common;
using System.Net.Mail;
using Amazon.Runtime;

namespace GroceryList.Data
{
    public sealed class DbUserRepository : IUserStore<AppUser>, IUserEmailStore<AppUser>,
        IUserPhoneNumberStore<AppUser>, IUserTwoFactorStore<AppUser>, IUserPasswordStore<AppUser>, IDisposable
    {
                //dataUser.UserName = user.UserName;
                //dataUser.NormalizedUserName = user.NormalizedUserName;
                //dataUser.Email = user.Email;
                //dataUser.NormalizedEmail = user.NormalizedEmail;
                //dataUser.EmailConfirmed = user.EmailConfirmed;
                //dataUser.PasswordHash = user.PasswordHash;
                //dataUser.PhoneNumber = user.PhoneNumber;
                //dataUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                //dataUser.TwoFactorEnabled = user.TwoFactorEnabled;
        private const string sqlGet = @"SELECT `user_id`, `user_name`, `email_address`
FROM `{0}_users`
WHERE `is_active` = 1
    AND (@UserId IS NULL OR (@UserId IS NOT NULL AND `user_id` = @UserId))
    AND (@UserName IS NULL OR (@UserName IS NOT NULL AND `user_name` = @UserName))
    AND (@Email IS NULL OR (@Email IS NOT NULL AND `email_address` = @Email))
;";
        private const string sqlSet = @"-- insert if missing, otherwise update

;";
        private const string sqlDelete = @"UPDATE `{0}_users`
SET `is_active` = 0, `modified_at` = @ModifiedTime, `modified_by` = @ModifiedBy
WHERE `user_id` = @UserId;";

        //private readonly DbProviderFactory provider;
        private readonly DbConnection conn;

        public DbUserRepository(DbProviderFactory providerFactory)
        {
            //provider = providerFactory;
            conn = providerFactory.CreateConnection();
            conn.ConnectionString = conf.ConnectionString;
        }
        private class GetParams
        {
            public string? UserId { get; set; }
            public string? UserName { get; set; }
            public string? Email { get; set; }
            public bool SearchActiveOnly { get; set; } = false;
        }

        private static string FormatSql(string query, AppUser user)
        {
            // handle missing HomeId
            return string.Format(query, user.HomeId);
        }

        //private async Task UpdateLookups(AppUserLookup lookup)
        //{
        //    // remove lookups when Email & UserName are NULL
        //    if (lookup.Email == null && lookup.UserName == null)
        //    {
        //        list.RemoveAll(l => l.Id.Equals(lookup.Id, StringComparison.Ordinal));
        //    }
        //    else
        //    {
        //        int i = list.FindIndex(l => l.Id.Equals(lookup.Id, StringComparison.Ordinal));
        //        if (i != -1)
        //        {
        //            list[i].Email = lookup.Email;
        //            list[i].UserName = lookup.UserName;
        //        }
        //        else
        //        {
        //            list.Add(lookup);
        //        }
        //    }

        //    await fileService.SetAsync(folder, lookupFile, list);
        //} // END UpdateLookups

        public Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken)
        {
            return UpdateAsync(user, cancellationToken);
        } // END CreateAsync

        public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken)
        {
            bool changedEmail = false, changedName = false;
            cancellationToken.ThrowIfCancellationRequested();

            // ID value MUST be set, why not throw if not set rather than create a new user?
            if (string.IsNullOrWhiteSpace(user.Id))
            {
                user.Id = Utils.GetNewUuid();
            }

            // is get needed at all, since we're just setting is at the end
            var dataUser = await GetAsync(new GetParams { UserId = user.Id });
            if (dataUser != null)
            {
                dataUser.UserName = user.UserName;
                dataUser.NormalizedUserName = user.NormalizedUserName;
                dataUser.Email = user.Email;
                dataUser.NormalizedEmail = user.NormalizedEmail;
                dataUser.EmailConfirmed = user.EmailConfirmed;
                dataUser.PasswordHash = user.PasswordHash;
                dataUser.PhoneNumber = user.PhoneNumber;
                dataUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                dataUser.TwoFactorEnabled = user.TwoFactorEnabled;
                // don't change Created Time
            }
            else
            {
                dataUser = user;
                dataUser.CreatedTime = DateTimeOffset.UtcNow;
            }
            dataUser.EditedTime = DateTimeOffset.UtcNow;

            await fileService.SetAsync(folder, user.Id, dataUser);

            // update the lookups
            if (changedEmail || changedName)
            {
                await UpdateLookups(new AppUserLookup
                {
                    Id = dataUser.Id,
                    Email = dataUser.NormalizedEmail,
                    UserName = dataUser.NormalizedUserName,
                });
            }

            return IdentityResult.Success;
        } // END UpdateAsync

        public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // handle missing HomeId
            var sql = FormatSql(sqlDelete, user);
            var result = await conn.ExecuteAsync(sql, new { UserId = user.Id });
            return result == 1 ?
                IdentityResult.Success :
                IdentityResult.Failed(new IdentityError { Code = "DefaultError", Description = $"ID `{user.Id}` NOT Found!" });
        }

        private async Task<AppUser> GetAsync(GetParams parms) //string userId)
        {
            //if (conn.State != System.Data.ConnectionState.Open)
            //    await conn.OpenAsync();

            var user = await conn.QuerySingleOrDefaultAsync<AppUser>(sqlGet, parms);
            return user ?? AppUser.Empty;
        } // END GetAsync

        public async Task<AppUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await GetAsync(new GetParams { UserId = userId });
        }

        // this is a problem, as you have to search a bunch of files
        // probably need to create some lookup files for these
        public async Task<AppUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await GetAsync(new GetParams { UserName = normalizedUserName });
        } // END FindByNameAsync

        public async Task<AppUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await GetAsync(new GetParams { Email = normalizedEmail });
        } // END FindByEmailAsync

        public Task<string> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(AppUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(AppUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(AppUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public async Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            await UpdateAsync(user);
            //return Task.FromResult(0);
        }

        public Task<string> GetNormalizedEmailAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(AppUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            await UpdateAsync(user);
            //return Task.FromResult(0);
        }

        public Task SetPhoneNumberAsync(AppUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            await UpdateAsync(user);
            //return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            await UpdateAsync(user);
            //return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(AppUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            await UpdateAsync(user);
            //return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            await UpdateAsync(user);
            //return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public void Dispose()
        {
            conn.Dispose();
        }
    }
}
