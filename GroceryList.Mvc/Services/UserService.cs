using GroceryList.Mvc.Models;
using System;

namespace GroceryList.Mvc.Services
{
    public interface IUserService
    {
        public AppUser GetUserByEmail(string email);
    }

    public class UserService : IUserService
    {
        public AppUser GetUserByEmail(string email)
        {
            return null;
        }
    }
}
