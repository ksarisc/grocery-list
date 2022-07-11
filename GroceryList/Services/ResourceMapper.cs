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
    public interface IResourceMapper : IDisposable
    {
        public Stream this[string fileName] => Get(fileName);
        public Stream Get(string fileName);
        public string GetSql(string name);
        public Task<string> GetSqlAsync(string name);
    }
    public sealed class ResourceMapper : IResourceMapper
    {
        private readonly SemaphoreSlim sqlLocker = new(1, 1);
        private readonly Dictionary<string, string> sqlMap = new(StringComparer.OrdinalIgnoreCase);
        private readonly Assembly me;

        public ResourceMapper()
        {
            me = typeof(ResourceMapper).Assembly;
        }

        public Stream Get(string fileName)
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

                // retrieve if not currently loaded
                using var stream = Get($"{name}.sql");
                using var reader = new StreamReader(stream);
                var sql = reader.ReadToEnd();
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

                // retrieve if not currently loaded
                using var stream = Get($"{name}.sql");
                using var reader = new StreamReader(stream);
                var sql = await reader.ReadToEndAsync();
                sqlMap[name] = sql;
                return sql;
            }
            catch (Exception) { throw; }
            finally { sqlLocker.Release(); }
        } // END GetSqlAsync

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
