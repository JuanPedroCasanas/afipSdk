using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSAA.exceptions
{
    public class WSAAServiceException : Exception
    {
        public WSAAServiceException(string? message) : base(message)
        {
        }
        
    }
}