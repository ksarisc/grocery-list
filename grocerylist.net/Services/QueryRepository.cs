using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace grocerylist.net.Services
{
    public interface IQueryRepository
    {
        string Get(string key);
        // Task<string> GetAsync(string key);
    }

    public class QueryRepository : IQueryRepository
    {
        //private readonly IConfiguration config;
        private readonly IConfigurationSection section;

        public QueryRepository(IConfiguration configuration)
        {
            //config = configuration;
            section = configuration.GetSection("queries");
        }

        public string Get(string key)
        {
            return section[key];
        } // END Get

        // public async Task<string> GetAsync(string key)
        // {
        //     return section[key];
        // } // END GetAsync
    }
}
