using GroceryList.Lib;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroceryList.Db;

// IDEA: create a single SqlResourceBuilder instance per HomeID (as they are needed) and cache the query strings (could even cache connection strings there too)
public class SqlResourceBuilder
{
    private const string homeIdField = "{{homeId}}";
    private readonly ILogger logger;
    private readonly StringBuilder sql;
    private readonly string id;

    public SqlResourceBuilder(IResourceMapper mapper, string name, int homeId, ILogger sqlLogger)
        : this(mapper, name, homeId.ToString(), sqlLogger) { }
    public SqlResourceBuilder(IResourceMapper mapper, string name, string homeId, ILogger sqlLogger)
    {
        logger = sqlLogger;
        id = homeId;
        var debug = logger.IsEnabled(LogLevel.Debug);
        if (!string.IsNullOrWhiteSpace(name))
        {
            var value = mapper.GetSql(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = value.Replace(homeIdField, id);
            }
            if (debug) logger.LogDebug("New Builder (Home:{homeId}) SQL `{name}` Default: {query}", homeId, name, value);
            sql = CreateBuilder(value);
        }
        else
        {
            if (debug) logger.LogDebug("New Builder (Home:{homeId}) SQL `{name}` Default: N/A", homeId, name);
            sql = new StringBuilder(defaultCapacity);
        }
    }

    private const int defaultCapacity = 1000;
    [ThreadStatic]
    private static StringBuilder instance = new StringBuilder(defaultCapacity / 2);
    private static StringBuilder CreateBuilder(string value)
    {
        // check size as well, or just always re-use
        if (instance == null || value.Length > instance.Capacity)
        {
            var sb = new StringBuilder(Math.Max(value.Length, defaultCapacity));
            sb.Append(value);
            return sb;
        }

        instance.Clear();
        instance.Append(value);
        return instance;
    }

    public SqlResourceBuilder Replace(string keyName, string value)
    {
        // ?? throw ??
        //if (sql== null) return this;

        if (!keyName.StartsWith("{{", StringComparison.Ordinal))
        {
            keyName = "{{" + keyName + "}}";
        }

        sql.Replace(keyName, value);
        return this;
    }

    public SqlResourceBuilder ReplaceHome()
    {
        sql.Replace(homeIdField, id);
        return this;
    }

    public SqlResourceBuilder Append(string value)
    {
        sql.Append(value);
        return this;
    }
    public SqlResourceBuilder Append(char value)
    {
        sql.Append(value);
        return this;
    }

    public override string ToString()
    {
        if (sql.Length > 0)
        {
            var temp = sql.ToString();
            if (temp.Contains(homeIdField, StringComparison.Ordinal))
                return temp.Replace(homeIdField, id.ToString());
            return temp;
        }

        return base.ToString() ?? "typeof(SqlResourceBuilder)";
    }
}
