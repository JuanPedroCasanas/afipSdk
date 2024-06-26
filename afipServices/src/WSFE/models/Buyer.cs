using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.models
{
    public class Buyer
    {
        public int DocumentType { get; set; }
        public long DocumentNumber { get; set; }
        public double OwnershipPercentage { get; set; }
    }
}