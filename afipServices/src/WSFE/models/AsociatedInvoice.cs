using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.models
{
    public class AsociatedInvoice
    {
        //Invoice number
        public long Number { get; set; }
        public int Type { get; set; }
        //Punto de venta
        public int PtoVta { get; set; }
        public string? Cuit { get; set; }
        public DateTime? GenerationDate { get; set; }
    }
}