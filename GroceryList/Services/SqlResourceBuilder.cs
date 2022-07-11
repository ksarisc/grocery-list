using System;
using System.Text;

namespace GroceryList.Services;

public class SqlResourceBuilder
{
    private readonly IResourceMapper map;
    [ThreadStatic]
    private static StringBuilder sql;

    public SqlResourceBuilder(IResourceMapper resourceMapper, string? name = null)
    {
        map = resourceMapper;
        if (!string.IsNullOrWhiteSpace(name)) Create(name);
    }

    public SqlResourceBuilder Create(string name)
    {
        var value = map.GetSql(name);
        // check size as well, or just always re-use
        if (sql == null) sql = new StringBuilder(value);
        else
        {
            sql.Clear();
            sql.Append(value);
        }

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
        if (sql == null) return base.ToString();

        return sql.ToString();
    }
}
