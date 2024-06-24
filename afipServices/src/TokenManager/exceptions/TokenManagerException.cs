using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.TokenManager.exceptions
{
    public class TokenManagerException : Exception
    {
        public TokenManagerException(string? message) : base(message)
        {
        }
    }
}