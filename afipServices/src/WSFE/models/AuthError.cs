using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.models
{
    public class AuthError
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}