using System;

namespace BusinessLogic.Exceptions
{
    public abstract class CustomException : Exception
    {
        public int StatusCode { get; }

        protected CustomException(string message, int statusCode = 500) : base(message) 
        {
            StatusCode = statusCode;
        }
    }
}
