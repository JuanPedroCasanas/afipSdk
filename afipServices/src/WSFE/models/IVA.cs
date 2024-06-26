using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.models
{
    public class IVA
    {
        public int Id { get; set; } 
       //Base imponible
       public double TaxBase { get; set; }
       public double Value { get; set; }
    }
}