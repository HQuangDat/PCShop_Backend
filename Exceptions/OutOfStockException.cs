using System;

namespace PCShop_Backend.Exceptions
{
    public class OutOfStockException : Exception
    {
        public OutOfStockException(string message) : base(message)
        {
        }
    }
}
