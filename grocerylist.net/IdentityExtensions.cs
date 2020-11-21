using grocerylist.net.Models.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace grocerylist.net
{
    public static class MyClaimTypes
    {
        public static string Location= "http://schemas.xmlsoap.org/ws/2020/09/identity/claims/location";
        public static string LocationHash= "http://schemas.xmlsoap.org/ws/2020/09/identity/claims/location-hash";
    }

    public static class IdentityExtensions
    {
        // public static IdentityOptions GetIdentityOptions(IConfiguration configuration)
        // {
        //     var section = configuration.GetSection("Identity");
        //     if (!section.Exists()) {
        //         return null;
        //     }
        //     var ident = new IdentityOptions();
        //     var passOpts = section.GetConf<PasswordOptions>("Password");
        // }
        public static TimeSpan GetIdTimeSpan(this IConfiguration configuration)
        {
            return TimeSpan.FromMinutes(5);
        } // END GetIdTimeSpan

        public static IServiceCollection AddGroceryIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // services.AddDbContext<ApplicationDbContext>(options =>
            //     options.UseMySql( //options.UseSqlite(
            //         Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<HomeUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true);
            //services.AddDefaultIdentity<HomeUser>(options => options.SignIn.RequireConfirmedAccount = true);
                //.AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<IdentityOptions>(options => {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = configuration.GetIdTimeSpan();
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options => {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = configuration.GetIdTimeSpan();

                options.LoginPath = "/User/Account/Login";
                options.AccessDeniedPath = "/User/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            return services;
        } // END AddGroceryIdentity

        public static void AddClaim(this ClaimsPrincipal principal, string claimType, string value)
        {
            ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim(claimType, value) });
        }

        public static void AddHome(this ClaimsPrincipal principal, HomeUser user)
        {
            if (user.HomeId != 0) {
                AddClaim(principal, MyClaimTypes.Location, user.HomeId.ToString());
            }
            if (!String.IsNullOrWhiteSpace(user.HomeIdHash)) {
                AddClaim(principal, MyClaimTypes.LocationHash, user.HomeIdHash);
            }
        } // END AddHome

        public static void AddNames(this ClaimsPrincipal principal, HomeUser user)
        {
            if (!String.IsNullOrWhiteSpace(user.FirstName)) {
                AddClaim(principal, ClaimTypes.GivenName, user.FirstName);
            }
            if (!String.IsNullOrWhiteSpace(user.LastName)) {
                AddClaim(principal, ClaimTypes.Surname, user.LastName);
            }
        } // END AddNames

        public static bool IsType(this Claim claim, string claimType)
        {
            return claim.Type.Equals(claimType, StringComparison.Ordinal);
        } // END IsType

        public static HomeUser GetHomeUser(this ClaimsPrincipal principal)
        {
            var user = new HomeUser();
            foreach(var claim in principal.Claims)
            {
                if (claim.IsType(ClaimTypes.Email)) {
                    user.Email = claim.Value;
                } else if (claim.IsType(ClaimTypes.GivenName)) {
                    user.FirstName = claim.Value;
                } else if (claim.IsType(ClaimTypes.Surname)) {
                    user.LastName = claim.Value;
                }
            }
            return user;
        } // END GetHomeUser

        // public static string GetValue(this IIdentity identity, string field)
        // {
        //     var claim = ((ClaimsIdentity)identity).FindFirst(field);
        //     // Test for null to avoid issues during local testing
        //     return (claim != null) ? claim.Value : string.Empty;
        // }

        // public static string FirstName(this IIdentity identity)
        // {
        //     return GetValue(identity, ClaimTypes.GivenName);
        // }
 
 
        // public static string LastName(this IIdentity identity)
        // {
        //     return GetValue(identity, ClaimTypes.Surname);
        // }
    }
}
