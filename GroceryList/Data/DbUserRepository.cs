using Dapper;
using GroceryList.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Data.Common;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GroceryList.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        private readonly string connect;
        private readonly DbProviderFactory factory;
        private readonly ILogger<DbUserRepository> logger;

        public DbUserRepository(DbProviderFactory providerFactory, IConfiguration configuration, ILogger<DbUserRepository> userLogger)
        {
            factory = providerFactory;
            //conn = providerFactory.CreateConnection();
            //conn.ConnectionString = conf.ConnectionString;
            connect = configuration.GetConnectionWithSecrets("Main");
            logger = userLogger;
        }

        private class GetParams
        {
            public string? UserId { get; set; }
            public string? UserName { get; set; }
            public string? Email { get; set; }
            public bool SearchActiveOnly { get; set; } = false;
        }

        //private static string FormatSql(string query, AppUser user)
        //{
        //    // handle missing HomeId
        //    return string.Format(query, user.HomeId);
        //}
        private DbConnection GetConnection()
        {
            var conn = factory.CreateConnection();
            if (conn == null) throw new NullReferenceException("DbProvider could NOT generate connection!");

            conn.ConnectionString = connect;
            return conn;
        }

        public Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken)
        {
            return UpdateAsync(user, cancellationToken);
        } // END CreateAsync

        public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken)
        {
            bool changedEmail = false, changedName = false;
            cancellationToken.ThrowIfCancellationRequested();

            // check if user exists by Id (if exists), Name(s), & Email/Other

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

            //await fileService.SetAsync(folder, user.Id, dataUser);

            //// update the lookups
            //if (changedEmail || changedName)
            //{
            //    await UpdateLookups(new AppUserLookup
            //    {
            //        Id = dataUser.Id,
            //        Email = dataUser.NormalizedEmail,
            //        UserName = dataUser.NormalizedUserName,
            //    });
            //}
            using (var conn = GetConnection())
            {
                var count = await conn.ExecuteAsync(sqlSet, user);
                if (count > 0)
                    return IdentityResult.Success;
            }
            return IdentityResult.Failed(new IdentityError { Description = "Could NOT edit user" });
        } // END UpdateAsync

        public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // handle missing HomeId
            var sql = string.Format(sqlDelete, user.HomeId);
            try
            {
                var conn = GetConnection();
                var result = await conn.ExecuteAsync(sql, new { UserId = user.Id });
                return result == 1 ?
                    IdentityResult.Success :
                    IdentityResult.Failed(new IdentityError { Code = "DefaultError", Description = $"ID `{user.Id}` NOT Found!" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "", user);
            }
            return IdentityResult.Failed(new IdentityError { Description = $"ID `{user.Id}` could NOT be deleted!" });
        }

        private async Task<AppUser> GetAsync(GetParams parms) //string userId)
        {
            var conn = GetConnection();
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
            await UpdateAsync(user, cancellationToken);
            //return Task.FromResult(0);
        }

        public Task<string> GetNormalizedEmailAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public async Task SetNormalizedEmailAsync(AppUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            await UpdateAsync(user, cancellationToken);
        }

        public async Task SetPhoneNumberAsync(AppUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            await UpdateAsync(user, cancellationToken);
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
            //user.PhoneNumberConfirmed = confirmed;
            //await UpdateAsync(user, cancellationToken);
            return Task.FromResult(0);
        }

        public async Task SetTwoFactorEnabledAsync(AppUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            await UpdateAsync(user, cancellationToken);
        }

        public Task<bool> GetTwoFactorEnabledAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public async Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            await UpdateAsync(user, cancellationToken);
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
            //conn.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
