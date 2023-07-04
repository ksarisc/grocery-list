//using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace GroceryList.Services;

public class SqlResourceBuilder
{
    private const string homeIdField = "{{homeId}}";
    private readonly ILogger logger;
    private readonly IResourceMapper map;
    private readonly StringBuilder sql;
    private readonly string id;
    private readonly bool isDebug;

    public SqlResourceBuilder(IResourceMapper resourceMapper, string name, int homeId, ILogger sqlLogger)
        : this(resourceMapper, name, homeId.ToString(), sqlLogger) { }
    public SqlResourceBuilder(IResourceMapper resourceMapper, string name, string homeId, ILogger sqlLogger)
    {
        map = resourceMapper;
        logger = sqlLogger;
        isDebug = logger.IsEnabled(LogLevel.Debug);
        id = homeId;
        if (!string.IsNullOrWhiteSpace(name))
        {
            var value = map.GetSql(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = value.Replace(homeIdField, homeId.ToString());
            }
            if (isDebug) logger.LogDebug("New Builder (Home:{homeId}) SQL `{name}` Default: {query}", homeId, name, value);
            sql = CreateBuilder(value);
        }
        else
        {
            if (isDebug) logger.LogDebug("New Builder (Home:{homeId}) SQL `{name}` Default: N/A", homeId, name);
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

    //public SqlResourceBuilder Create(string name){
    //    var value = map.GetSql(name);
    //    sql = CreateBuilder(value);
    //    return this;
    //}

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

    public SqlResourceBuilder Append(string value)
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
