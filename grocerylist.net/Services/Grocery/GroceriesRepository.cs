using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using grocerylist.net.Models.Grocery;
using grocerylist.net.Models.Security;

namespace grocerylist.net.Services.Grocery
{
    public interface IGroceriesRepository : IDisposable
    {
        Task<Item> SaveAsync(HomeUser user, Item item);
        Task<IEnumerable<Item>> GetCurrentItemsAsync(HomeUser user);
        Task<IEnumerable<Item>> GetItemsAsync(HomeUser user, ItemsQuery query);
        Task<Item> GetItemByIdAsync(HomeUser user, uint itemId);
        Task<PurchasedItem> PurchaseAsync(HomeUser user, Item item);
    }

    public class GroceriesRepository : IGroceriesRepository
    {
        private static readonly string current = "current";
        private static readonly string archive = "archive";

        private readonly IConnectionService factory;
        private readonly IQueryRepository queryRepo;
        private readonly ILogger logger;

        public GroceriesRepository(IConnectionService connectionService,
                            IQueryRepository queryRepo, ILogger logger)
        {
            this.factory = connectionService;
            this.queryRepo = queryRepo;
            this.logger = logger;
        }

        public async Task<Item> SaveAsync(HomeUser user, Item item)
        {
            if (item.HomeId != user.HomeId) {
                item.HomeId = user.HomeId;
            }
            var query = queryRepo.Get((item.Id > 0) ?
                            "ItemUpdateQuery" : "ItemInsertQuery");
            using (var conn = factory.NewConnection(current))
            {
                await conn.OpenAsync();
                var result = await conn.QueryFirstOrDefaultAsync<Item>(query, item);
                if (result == null) {
                    throw new Exception("Item could not be saved");
                }
                return result;
            }
        } // END SaveAsync

        public async Task<IEnumerable<Item>> GetCurrentItemsAsync(HomeUser user)
        {
            var parm = new {
                HomeId = user.HomeId
            };
            using (var conn = factory.NewConnection(current))
            {
                await conn.OpenAsync();
                return await conn.QueryAsync<Item>(
                    queryRepo.Get("ItemsCurrentQuery"), parm);
            }
        } // END GetCurrentItemsAsync

        public async Task<IEnumerable<Item>> GetItemsAsync(HomeUser user, ItemsQuery query)
        {
            var qb = new StringBuilder(queryRepo.Get("ItemsHistoryQuery"));
            var parms = new Dapper.DynamicParameters();
            parms.Add("@HomeId", user.HomeId);
            qb.Append("home_id = @HomeId");
            if (!String.IsNullOrEmpty(query.Name)) {
                parms.Add("@Name", query.Name);
                qb.Append("AND name = @Name");
            }
            if (query.CreatedDate1.HasValue) {
                parms.Add("@CreatedDate1", query.CreatedDate1.Value);
                qb.Append("AND created_at >= @CreatedDate1");
            }
            if (query.CreatedDate2.HasValue) {
                parms.Add("@CreatedDate2", query.CreatedDate2.Value);
                qb.Append("AND created_at <= @CreatedDate2");
            }
            if (query.PurchasedDate1.HasValue) {
                parms.Add("@PurchasedDate1", query.PurchasedDate1.Value);
                qb.Append("AND purchased_at >= @PurchasedDate1");
            }
            if (query.PurchasedDate2.HasValue) {
                parms.Add("@PurchasedDate2", query.PurchasedDate2.Value);
                qb.Append("AND purchased_at <= @PurchasedDate2");
            }
            using (var conn = factory.NewConnection(archive))
            {
                await conn.OpenAsync();
                return await conn.QueryAsync<PurchasedItem>(
                    qb.ToString(), parms);
            }
        } // END GetItemsAsync

        private async Task<Item> GetItemByIdAsync(string query, string connect, uint homeId, object parameter)
        {
            using (var conn = factory.NewConnection(connect))
            {
                await conn.OpenAsync();
                var result = await conn.QueryFirstOrDefaultAsync<Item>(query, parameter);
                if (result != null) {
                    if (result.HomeId == homeId) {
                        return result;
                    }
                    throw new InvalidOperationException("Item NOT accessible by current user");
                }
            }
            return null;
        } // END GetItemByIdAsync

        public async Task<Item> GetItemByIdAsync(HomeUser user, uint itemId)
        {
            var parm = new { ItemId = itemId };
            var query = queryRepo.Get("ItemByIdQuery");

            var result = await GetItemByIdAsync(query, current, user.HomeId, parm);
            if (result != null) {
                return result;
            }
            result = await GetItemByIdAsync(query, archive, user.HomeId, parm);
            if (result != null) {
                return result;
            }
            throw new KeyNotFoundException($"Item ID ({itemId}) NOT Found!");
        } // END GetItemByIdAsync

        public async Task<PurchasedItem> PurchaseAsync(HomeUser user, Item item)
        {
            // check item & user HomeId compatible?

            // create item in archive context (sync will happen later to delete from current)
            using (var conn = factory.NewConnection(archive))
            {
                await conn.OpenAsync();
                return await conn.QueryFirstOrDefaultAsync<PurchasedItem>(
                    queryRepo.Get("ItemPurchaseQuery"), item);
            }

            // var timestamp = DateTime.Now;
            // return new ArchivedItem{
            //     Id = item.Id,
            //     HomeId = item.HomeId,
            //     Name = item.Name,
            //     Brand = item.Brand,
            //     CreatedTime = item.CreatedTime,
            //     CreatedBy = item.CreatedBy,
            //     RequestedTime = item.RequestedTime,
            //     Notes = item.Notes,

            //     PurchasedTime = timestamp,
            //     PurchasedBy = user.Id
            // };
        } // END PurchaseAsync

        #region clean-up
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) {
                return;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~GroceriesRepository()
        {
            Dispose(false);
        }
        #endregion clean-up
    }
}
