using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.models
{
    public class VariableTypeValue
    {
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GenerationDate { get; set; } = string.Empty;
        public string? ExpirationDate { get; set; }
        public override string ToString()
        {
            return @$"  
                        Type: { Type }
                        Id: { Id }
                        Description: { Description }
                        Generation Date: { GenerationDate }
                        Expiration Date: { ExpirationDate }
            ";
        }
    }
}