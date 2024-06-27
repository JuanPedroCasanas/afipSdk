using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.exceptions
{
    public class WSFEServiceException : Exception
    {
        public WSFEServiceException(string? message) : base(message)
        {
        }
    }
}