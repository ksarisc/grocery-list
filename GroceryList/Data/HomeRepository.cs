using GroceryList.Models;
using GroceryList.Models.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public interface IHomeRepository
    {
        public Task<bool> ExistsAsync(string homeId);
        public Task<Home> FindAsync(string homeId);
        public Task<Home> FindAsync(GroceryUser user);
        public Task<Home> AddAsync(Home home);
        public Task AddApproveeAsync(GroceryUser approvee, string homeId);
        public Task<Home> ApproveAsync(GroceryUser approver, GroceryUser approvee);
    }

    public class HomeRepository : IHomeRepository
    {
        private readonly GroceryContext db;

        public HomeRepository(GroceryContext context)
        {
            db = context;
        }

        public async Task<bool> ExistsAsync(string homeId)
        {
            if (string.IsNullOrWhiteSpace(homeId)) return false;

            return await db.Homes.AnyAsync(h => h.Id == homeId);
        } // END FindAsync

        public async Task<Home> FindAsync(string homeId)
        {
            if (string.IsNullOrWhiteSpace(homeId)) return null;

            return await db.Homes.FirstOrDefaultAsync(h => h.Id == homeId);
        } // END FindAsync
        public async Task<Home> FindAsync(GroceryUser user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.HomeId)) return null;

            return await db.Homes.FirstOrDefaultAsync(h => h.Id == user.HomeId);
        } // END FindAsync

        public async Task<Home> AddAsync(Home home)
        {
            // check if currently exists and/or current user allowed

            // add
        } // END AddAsync

        public Task AddApproveeAsync(GroceryUser approvee, string homeId)
        {
        }

        public async Task<Home> ApproveAsync(GroceryUser approver, GroceryUser approvee)
        {
            // is approver part of home currently? // maybe check more permissions?

            // approve user
        } // END ApproveAsync
    }
}
