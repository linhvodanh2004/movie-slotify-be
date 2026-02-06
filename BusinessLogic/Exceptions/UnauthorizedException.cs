using System;

namespace BusinessLogic.Exceptions
{
    public class UnauthorizedException : CustomException
    {
        public UnauthorizedException(string message) : base(message, 401) { }
    }
}
