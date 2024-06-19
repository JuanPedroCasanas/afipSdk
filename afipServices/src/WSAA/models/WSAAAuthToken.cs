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
        public string UniqueId { get; set; } = string.Empty; //Technically a UInt32 type, but it's not worth the trouble to treat it like that
        public DateTime GenerationTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;


        public override string ToString()
        {
            
            return $@"
                WSAA Auth Token No: { UniqueId }
                Source: { Source }
                Destination: { Destination }
                Generation Time: { GenerationTime }
                Expiration Time: { ExpirationTime }
                Token: { Token }
                Sign: { Sign }
                Expired?: { IsExpired() }
            ";
        }

        public bool IsExpired()
        {
            int result = ExpirationTime.CompareTo(DateTime.Now);
            return result == 0 || result < 0;
        }
    } 
}