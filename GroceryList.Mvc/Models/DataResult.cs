using System;

namespace GroceryList.Mvc.Models
{
    public class DataResult<T>
    {
        public string Path { get; }
        public T Value { get; }

        public DataResult(string path, T value)
        {
            Path = path;
            Value = value;
        }
    }
}
