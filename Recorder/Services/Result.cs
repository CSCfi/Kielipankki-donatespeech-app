using System;

namespace Recorder.Services
{
    public class Result <T>
    {
        public bool Succeeded { get; private set; }
        public T Data { get; private set; }

        protected Result(bool succeeded, T data)
        {
            Succeeded = succeeded;
            Data = data;
        }

        protected Result(bool succeeded)
        {
            Succeeded = succeeded;
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>(succeeded: true, data: data);
        }

        public static Result<T> Failure()
        {
            return new Result<T>(succeeded: false);
        }
    }
}
