using System;

namespace PCShop_Backend.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "You do not have permission to access this resource.") 
            : base(message)
        {
        }
    }
}
