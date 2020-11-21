using System;
using System.Data.Common;
using grocerylist.net.Models.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace grocerylist.net.Services
{
    public interface IConnector
    {
        DbConnection Create(string name = null);
    }

    public class Connector : IConnector
    {
        private readonly ConnectorConfig conf;

        public Connector(ConnectorConfig config)
        {
            conf = config;
        }

        public DbConnection Create(string name = null)
        {
            return new MySqlConnection(conf.GetString(name));
        }
    }

    internal static class ConnectorExtensions
    {
        public static IServiceCollection AddConnector(this IServiceCollection services, IConfiguration configuration)
        {
            var conf = ConnectorConfig.Get(configuration);
            services.AddSingleton<ConnectorConfig>(conf);
            services.AddSingleton<IConnector, Connector>();
            return services;
        }
    }
}
