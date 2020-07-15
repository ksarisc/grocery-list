using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using grocerylist.net.Models.Security;

namespace grocerylist.net.Services.Security
{
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<HomeUser, IdentityRole>
    {
        public AppClaimsPrincipalFactory(
                UserManager<HomeUser> userManager,
                RoleManager<IdentityRole> roleManager,
                IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        public async override Task<ClaimsPrincipal> CreateAsync(HomeUser user)
        {
            var principal = await base.CreateAsync(user);
            principal.AddHome(user);
            principal.AddNames(user);
            return principal;
        }
    }
}
