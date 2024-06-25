using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.model
{
    public class Tribute
    {
       public int Id { get; set; } 
       public string? Description { get; set; }
       //Base imponible
       public double TaxBase { get; set; }
       //Alicuota
       public double Aliquot { get; set; }
       public double Value { get; set; }
    }
}