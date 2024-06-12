using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSAA.models
{
    public class WSAAAuthToken
    {
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public UInt32 UniqueId { get; set; }
        public DateTime GenerationTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string token { get; set; } = string.Empty;
        public string sign { get; set; } = string.Empty;
    } 
}