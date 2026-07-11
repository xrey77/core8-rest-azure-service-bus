using System;
using System.Globalization;

namespace core8_rest_azure_service_bus.Helpers
{
    public class AppException : Exception
    {
        public AppException(string message) : base(message) { }

        public AppException(string message, params object[] args) 
            : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}