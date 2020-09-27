using System;
using System.Security.Claims;
using System.Security.Principal;
using grocerylist.net.Models.Security;

namespace grocerylist.net
{
    public static class MyClaimTypes
    {
        public static string Location= "http://schemas.xmlsoap.org/ws/2020/09/identity/claims/location";
        public static string LocationHash= "http://schemas.xmlsoap.org/ws/2020/09/identity/claims/location-hash";
    }

    public static class IdentityExtensions
    {
        public static void AddClaim(this ClaimsPrincipal principal, string claimType, string value)
        {
            ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim(claimType, value) });
        }

        public static void AddHome(this ClaimsPrincipal principal, HomeUser user)
        {
            if (user.HomeId != 0 && user.HomeId != -1) {
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

        public static string GetValue(this IIdentity identity, string field)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst(field);
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string FirstName(this IIdentity identity)
        {
            return GetValue(identity, ClaimTypes.GivenName);
        }
 
 
        public static string LastName(this IIdentity identity)
        {
            
            return GetValue(identity, ClaimTypes.Surname);
        }
    }
}
