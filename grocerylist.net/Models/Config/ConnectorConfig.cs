using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace grocerylist.net.Models.Config
{
    public class ConnectorConfig
    {
        public string Default { get;set; }
        //try out System.Collections.Specialized.OrderedDictionary maybe?
        public Dictionary<string, string> Connections { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string GetString(string name = null)
        {
            if (String.IsNullOrWhiteSpace(name) || name.Equals("default", StringComparison.OrdinalIgnoreCase)) {
                return Default;
            }
            return Connections[name];
        } // END GetString

        public static ConnectorConfig Get(IConfiguration configuration)
        {
            var section = configuration.GetSection("Connector");
            if (!section.Exists()) {
                throw new ArgumentNullException("Connector configuration REQUIRED");
            }
            var conf = new ConnectorConfig();
            Console.WriteLine("Connector");
            foreach (var child in section.GetChildren()) {
                Console.WriteLine("Key: {0} | Value: {1}", child.Key, child.Value);
                if (String.IsNullOrWhiteSpace(child.Key) || child.Key.Equals("default", StringComparison.OrdinalIgnoreCase)) {
                    conf.Default = child.Value;
                    continue;
                }
                conf.Connections[child.Key] = child.Value;
            }
            if (String.IsNullOrWhiteSpace(conf.Default) && conf.Connections.Keys.Count > 0) {
                conf.Default = conf.Connections.First().Value;
            }
            return conf;
        } // END Get
    }
}
