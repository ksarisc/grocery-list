using System;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace grocerylist.net.Services
{
    public interface IConnectionService
    {
        DbConnection NewConnection(string name = null);
    }

    public class ConnectionService : IConnectionService
    {
        private readonly string connect;
        private readonly IConfiguration conf;

        public ConnectionService(IConfiguration configuration)
        {
            connect = configuration.GetConnectionString("main");
            conf = configuration;
        }

        public DbConnection NewConnection(string name = null)
        {
            if (String.IsNullOrEmpty(name)) {
                return new MySqlConnection(connect);
            }
            return new MySqlConnection(conf.GetConnectionString(name));
        }
    }
}
