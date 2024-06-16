using GroceryList.Models.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Services
{
    public sealed class ResourceMapper : GroceryList.Lib.IResourceMapper
    {
        private readonly SemaphoreSlim sqlLocker = new(1, 1);
        // TODO: shouldn't this be Ordinal?
        private readonly Dictionary<string, string> sqlMap = new(StringComparer.OrdinalIgnoreCase);
        private readonly Assembly me;

        public ResourceMapper()
        {
            me = typeof(ResourceMapper).Assembly;
        }

        public Stream? Get(string fileName)
        {
            // check file name / extension
            return me.GetManifestResourceStream($"GroceryList.Resources.{fileName}");
        }

        public string GetSql(string name)
        {
            sqlLocker.Wait();
            try
            {
                if (sqlMap.ContainsKey(name)) return sqlMap[name];

                string sql;
                // retrieve if not currently loaded
                using var stream = Get($"{name}.sql");
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    sql = reader.ReadToEnd();
                }
                else sql = string.Empty;
                sqlMap[name] = sql;
                return sql;
            }
            catch (Exception) { throw; }
            finally { sqlLocker.Release(); }
        } // END GetSql
        public async Task<string> GetSqlAsync(string name)
        {
            await sqlLocker.WaitAsync();
            try
            {
                if (sqlMap.ContainsKey(name)) return sqlMap[name];

                string sql;
                // retrieve if not currently loaded
                using var stream = Get($"{name}.sql");
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    sql = await reader.ReadToEndAsync();
                }
                else sql = string.Empty;
                sqlMap[name] = sql;
                return sql;
            }
            catch (Exception) { throw; }
            finally { sqlLocker.Release(); }
        } // END GetSqlAsync

        public string SetSql(string name, string sql)
        {
        }

        #region cleanup
        private void Dispose(bool disposing)
        {
            if (!disposing) return;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~ResourceMapper()
        {
            Dispose(false);
        }
        #endregion cleanup
    }
}
