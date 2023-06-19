using System;
using System.Text;

namespace GroceryList.Services;

public class SqlResourceBuilder
{
    private const int defaultCapacity = 1000;
    private readonly IResourceMapper map;
    [ThreadStatic]
    private static StringBuilder sql = new StringBuilder(defaultCapacity / 2);

    public SqlResourceBuilder(IResourceMapper resourceMapper, string name, int homeId)
    {
        map = resourceMapper;
        if (!string.IsNullOrWhiteSpace(name))
        {
            var value = map.GetSql(name);
            sql = CreateBuilder(value);
        }
        else
        {
            sql = new StringBuilder(defaultCapacity);
        }
    }

    private static StringBuilder CreateBuilder(string value)
    {
        // check size as well, or just always re-use
        if (sql == null || value.Length > sql.Capacity)
        {
            var sb = new StringBuilder(Math.Max(value.Length, defaultCapacity));
            sb.Append(value);
            return sb;
        }

        sql.Clear();
        sql.Append(value);
        return sql;
    }

    public SqlResourceBuilder Create(string name)
    {
        var value = map.GetSql(name);
        sql = CreateBuilder(value);
        return this;
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

    public SqlResourceBuilder Append(string value)
    {
        sql.Append(value);
        return this;
    }

    public override string ToString()
    {
        if (sql.Length > 0) sql.ToString();

        return base.ToString() ?? "typeof(SqlResourceBuilder)";
    }
}
