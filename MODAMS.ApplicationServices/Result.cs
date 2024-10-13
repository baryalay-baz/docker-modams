using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public bool IsFailure => !IsSuccess;
        protected Result(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
        public static Result Success()
        {
            return new Result(true, string.Empty);
        }
        public static Result Failure(string errorMessage)
        {
            return new Result(false, errorMessage);
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; private set; }
        protected Result(T value, bool isSuccess, string errorMessage) : base(isSuccess, errorMessage)
        {
            Value = value;
        }
        public static Result<T> Success(T value)
        {
            return new Result<T>(value, true, string.Empty);
        }
        public static Result<T> Failure(string errorMessage, T value = default(T))
        {
            return new Result<T>(value, false, errorMessage);
        }
    }
}
