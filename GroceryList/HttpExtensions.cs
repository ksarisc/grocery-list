using Microsoft.AspNetCore.Http;

namespace GroceryList;

internal static class HttpExtensions
{
    public static string GetRemoteIp(this HttpContext self, string defaultValue = "NO-REMOTE-IP")
    {
        return self.Connection?.RemoteIpAddress?.ToString() ?? defaultValue;
    }
}
