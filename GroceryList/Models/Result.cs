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

    public readonly struct ApiResult<T>
    {
        public ResultState State { get; }
        public T Value { get; }
        public string Message { get; }

        public ApiResult(T value)
        {
            State = ResultState.Success;
            Value = value;
            Message = string.Empty;
        }

        public ApiResult(string message)
        {
            State = ResultState.Faulted;
            Value = default(T);
            Message = message ?? string.Empty;
        }

        public ApiResult(T value, string message)
        {
            State = !string.IsNullOrWhiteSpace(message) ? ResultState.Success : ResultState.Faulted;
            Value = value;
            Message = message ?? string.Empty;
        }
    }
}
