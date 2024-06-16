using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Lib
{
    //internal class ResourceMapper
    public interface IResourceMapper : IDisposable
    {
        public string? GetConnection(string name);
        public string? GetConnectionWithSecrets(string name);

        public Stream? this[string fileName] => Get(fileName);
        public Stream? Get(string fileName);
        public string GetSql(string name);
        public Task<string> GetSqlAsync(string name); //, CancellationToken cancel

		public string SetSql(string name, string sql);
        public Task<string> SetSqlAsync(string name, string sql, CancellationToken cancel);

	}
}
