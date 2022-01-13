using GroceryList.Models;
using GroceryList.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    //public interface IUserRepository { }
    //public class UserRepository : IUserRepository { }
    public class UserRepository : IUserStore<AppUser>, IUserEmailStore<AppUser>,
        IUserPhoneNumberStore<AppUser>, IUserTwoFactorStore<AppUser>, IUserPasswordStore<AppUser>
    {
        private const string lookupFile = "lookup_user_data";

        private readonly IDataService fileService;
        private readonly string folder, userStore;

        public UserRepository(IDataService dataService, IConfiguration configuration)
        {
            fileService = dataService;
            folder = configuration.GetValue<string>("User::BasePath");
            //userStore = configuration.GetValue<string>("User::FileName");
            userStore = "app_user";
        }

        private async Task UpdateLookups(AppUserLookup lookup)
        {
            var list = await fileService.GetAsync<List<AppUserLookup>>(folder, lookupFile);

            // remove lookups when Email & UserName are NULL
            if (lookup.Email == null && lookup.UserName == null)
            {
                list.RemoveAll(l => l.Id.Equals(lookup.Id, StringComparison.Ordinal));
            }
            else
            {
                int i = list.FindIndex(l => l.Id.Equals(lookup.Id, StringComparison.Ordinal));
                if (i != -1)
                {
                    list[i].Email = lookup.Email;
                    list[i].UserName = lookup.UserName;
                }
                else
                {
                    list.Add(lookup);
                }
            }

            await fileService.SetAsync(folder, lookupFile, list);
        } // END UpdateLookups

        public Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken)
        {
            return UpdateAsync(user, cancellationToken);
        } // END CreateAsync

        public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken)
        {
            bool changedEmail = false, changedName = false;
            cancellationToken.ThrowIfCancellationRequested();

            // ID value MUST be set
            if (string.IsNullOrWhiteSpace(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }

            // is get needed at all, since we're just setting is at the end
            var dataUser = await fileService.GetAsync<AppUser>(folder, user.Id);
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

            var dataUser = await fileService.GetAsync<AppUser>(folder, user.Id);
            // back it up & delete
            await fileService.SetAsync(folder, user.Id, null);

            // update the lookups
            await UpdateLookups(new AppUserLookup
            {
                Id = dataUser.Id,
                Email = null,
                UserName = null,
            });

            return IdentityResult.Success;
        }

        private async Task<AppUser> GetAsync(string userId)
        {
            return await fileService.GetAsync<AppUser>(folder, userId); //userStore);
        } // END GetAsync

        public async Task<AppUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // should each user be 1 folder, or should the users be in a single folder?
            //var rqst = new DataRequest{
            //    HomeId = userId,
            //    StoreName = userStore,
            //    ActionName = folder,
            //};
            return await GetAsync(userId);
        }

        // this is a problem, as you have to search a bunch of files
        // probably need to create some lookup files for these
        public async Task<AppUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var list = await fileService.GetAsync<AppUserLookup[]>(folder, lookupFile);
            var user = list?.FirstOrDefault(u => u.UserName.Equals(normalizedUserName, StringComparison.OrdinalIgnoreCase));

            if (user == null || string.IsNullOrEmpty(user.Id)) return null;

            return await GetAsync(user.Id);
        } // END FindByNameAsync

        public async Task<AppUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var list = await fileService.GetAsync<AppUserLookup[]>(folder, lookupFile);
            var user = list?.FirstOrDefault(u => u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));

            if (user == null || string.IsNullOrEmpty(user.Id)) return null;

            return await GetAsync(user.Id);
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

        public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedEmailAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(AppUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberAsync(AppUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
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
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(AppUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public void Dispose() { }
    }
}
