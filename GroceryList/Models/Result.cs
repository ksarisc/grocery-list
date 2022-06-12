using System;

namespace GroceryList.Models
{
    public enum ResultState
    {
        Faulted,
        Success,
    }

    public readonly struct Result<T> //: IEquatable<Result<T>>, IComparable<Result<T>>
    {
        internal readonly ResultState State;
        internal readonly T Value;
        private readonly Exception exception;
        //internal Exception Exception => exception ?? BottomException.Default;

        public Result(T value)
        {
            State = ResultState.Success;
            Value = value;
            exception = null;
        }

        public Result(Exception ex)
        {
            State = ResultState.Faulted;
            exception = ex;
            Value = default(T);
        }
    }
}
