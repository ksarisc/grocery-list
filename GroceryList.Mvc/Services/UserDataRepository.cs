using Dapper;
using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public interface IUserDataRepository<T> : IDisposable
    {
        public Task<IEnumerable<T>> GetAsync(object parameters = null);
        public Task<int> InsertAsync(T item);
        public Task<int> UpdateAsync(T item);
        public Task<int> DeleteAsync(T item);
    }

    public class UserDataRepository<T> : IUserDataRepository<T>
    {
        private readonly string table, create, select, insert, update, delete;

        protected readonly IDataService data;
        protected readonly AppUser user;
        protected readonly DbConnection conn;

        public UserDataRepository(IDataService dataService, AppUser appUser,
                                UserDataConfig config)
        {
            data = dataService;
            user = appUser;
            table = config.Table;
            create = config.Create;
            select = config.Select;
            insert = config.Insert;
            update = config.Update;
            delete = config.Delete;
            conn = data.GetConnection(user);
        }

        protected async Task Open()
        {
            if (conn.State == ConnectionState.Open) return;
            await conn.OpenAsync();
        }

        public async Task<IEnumerable<T>> GetAsync(object parameters)
        {
            await Open();
            await data.TableExists(conn, table, create);
            return (await conn.QueryAsync<T>(select, parameters)).AsList();
        } // END GetListAsync

        public async Task<int> InsertAsync(T item)
        {
            await Open();
            return await conn.ExecuteAsync(insert, item);
        } // END InsertAsync

        public async Task<int> UpdateAsync(T item)
        {
            await Open();
            return await conn.ExecuteAsync(update, item);
        } // END UpdateAsync

        public async Task<int> DeleteAsync(T item)
        {
            await Open();
            return await conn.ExecuteAsync(delete, item);
        } // END DeleteAsync


        #region cleanup
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            conn.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~UserDataRepository()
        {
            Dispose(false);
        }
        #endregion cleanup
    }
}
