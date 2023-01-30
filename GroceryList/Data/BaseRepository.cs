using GroceryList.Models;
using GroceryList.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    //public abstract class BaseRepository<T> : IDisposable where T : IAuthModel
    //{
    //    private readonly IDataService fileService;
    //    private readonly string lookupFile;
    //    private readonly string folder;

    //    public BaseRepository(IDataService dataService, string lookupFileName, string configuredFolder)
    //    {
    //        fileService = dataService;
    //        lookupFile = lookupFileName;
    //        folder = configuredFolder;
    //    }

    //    private async Task UpdateLookups(AppUserLookup lookup)
    //    {
    //        var list = await fileService.GetAsync<List<AppUserLookup>>(folder, lookupFile);

    //        // remove lookups when Email & UserName are NULL
    //        if (lookup.Email == null && lookup.UserName == null)
    //        {
    //            list.RemoveAll(l => l.Id.Equals(lookup.Id, StringComparison.Ordinal));
    //        }
    //        else
    //        {
    //            int i = list.FindIndex(l => l.Id.Equals(lookup.Id, StringComparison.Ordinal));
    //            if (i != -1)
    //            {
    //                list[i].Email = lookup.Email;
    //                list[i].UserName = lookup.UserName;
    //            }
    //            else
    //            {
    //                list.Add(lookup);
    //            }
    //        }

    //        await fileService.SetAsync(folder, lookupFile, list);
    //    } // END UpdateLookups

    //    protected abstract void SetFields(ref T modelToSet, T model);

    //    public Task<IdentityResult> CreateAsync(T model, CancellationToken cancellationToken)
    //    {
    //        return UpdateAsync(model, cancellationToken);
    //    } // END CreateAsync

    //    public async Task<IdentityResult> UpdateAsync(T model, CancellationToken cancellationToken)
    //    {
    //        bool changedEmail = false, changedName = false;
    //        cancellationToken.ThrowIfCancellationRequested();

    //        // ID value MUST be set
    //        if (string.IsNullOrWhiteSpace(model.Id))
    //        {
    //            model.Id = Utils.GetNewUuid();
    //        }

    //        // is get needed at all, since we're just setting is at the end
    //        var dataModel = await fileService.GetAsync<T>(folder, model.Id);
    //        if (dataModel != null)
    //        {
    //            model.CreatedTime = dataModel.CreatedTime;
    //            // don't allow Created Time to change
    //        }
    //        else
    //        {
    //            model.CreatedTime = DateTimeOffset.UtcNow;
    //        }
    //        model.EditedTime = DateTimeOffset.UtcNow;

    //        await fileService.SetAsync(folder, model.Id, model);

    //        // update the lookups
    //        if (changedEmail || changedName)
    //        {
    //            await UpdateLookups(new AppUserLookup
    //            {
    //                Id = model.Id,
    //                Email = model.NormalizedEmail,
    //                UserName = model.NormalizedUserName,
    //            });
    //        }

    //        return IdentityResult.Success;
    //    } // END UpdateAsync

    //    public async Task<IdentityResult> DeleteAsync(T model, CancellationToken cancellationToken)
    //    {
    //        cancellationToken.ThrowIfCancellationRequested();

    //        var dataModel = await fileService.GetAsync<T>(folder, model.Id);
    //        // back it up & delete
    //        await fileService.SetAsync(folder, model.Id, null);

    //        // update the lookups
    //        await UpdateLookups(new AppUserLookup
    //        {
    //            Id = dataModel.Id,
    //            Email = null,
    //            UserName = null,
    //        });

    //        return IdentityResult.Success;
    //    }

    //    private async Task<T> GetAsync(string id)
    //    {
    //        return await fileService.GetAsync<T>(folder, id);
    //    } // END GetAsync

    //    public async Task<T> FindByIdAsync(string id, CancellationToken cancellationToken)
    //    {
    //        cancellationToken.ThrowIfCancellationRequested();

    //        // should each user be 1 folder, or should the users be in a single folder?
    //        //var rqst = new DataRequest{
    //        //    HomeId = id,
    //        //    StoreName = userStore,
    //        //    ActionName = folder,
    //        //};
    //        return await GetAsync(id);
    //    }

    //    // this is a problem, as you have to search a bunch of files
    //    // probably need to create some lookup files for these
    //    public async Task<T> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    //    {
    //        cancellationToken.ThrowIfCancellationRequested();

    //        var list = await fileService.GetAsync<AppUserLookup[]>(folder, lookupFile);
    //        var user = list?.FirstOrDefault(u => u.UserName.Equals(normalizedUserName, StringComparison.OrdinalIgnoreCase));

    //        if (user == null || string.IsNullOrEmpty(user.Id)) return null;

    //        return await GetAsync(user.Id);
    //    } // END FindByNameAsync

    //    public async Task<T> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    //    {
    //        cancellationToken.ThrowIfCancellationRequested();

    //        var list = await fileService.GetAsync<AppUserLookup[]>(folder, lookupFile);
    //        var user = list?.FirstOrDefault(u => u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));

    //        if (user == null || string.IsNullOrEmpty(user.Id)) return default(T);

    //        return await GetAsync(user.Id);
    //    } // END FindByEmailAsync

    //    public void Dispose() { }
    //}
}
