using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.models
{
    public class WSFEResponseError
    {        
        public string Code { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;


        public override string ToString()
        {
            return $"Error Code: { Code }\nError Description: { Description }";
        }
        
    }
}