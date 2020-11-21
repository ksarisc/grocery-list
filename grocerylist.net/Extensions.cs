using Microsoft.Extensions.Configuration;

namespace grocerylist.net
{
    internal static class Extensions
    {
        public static T GetConf<T>(this IConfiguration configuration, string sectionKey) where T : new()
        {
            var obj = new T();
            configuration.Bind(sectionKey, obj);
            return obj;
        }
    }
}
