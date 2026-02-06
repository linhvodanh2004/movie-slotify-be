using System;

namespace BusinessLogic.Exceptions
{
    public class ValidationException : CustomException
    {
        public ValidationException(string message) : base(message, 400) { }
    }
}
