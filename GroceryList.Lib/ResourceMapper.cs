﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace GroceryList.Lib
{
    //internal class ResourceMapper
    public interface IResourceMapper : IDisposable
    {
        public Stream? this[string fileName] => Get(fileName);
        public Stream? Get(string fileName);
        public string GetSql(string name);
        public Task<string> GetSqlAsync(string name);

        public string SetSql(string name, string sql);
    }
}
