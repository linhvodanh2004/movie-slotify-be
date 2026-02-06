using System;

namespace BusinessLogic.Exceptions
{
    public class ForbiddenException : CustomException
    {
        public ForbiddenException(string message) : base(message, 403) { }
    }
}
