using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace afipServices.src
{

    [XmlRoot(ElementName = "FECAEDetResponse" ,  Namespace = "http://ar.gov.afip.dif.FEV1/"),]
    public class test
    {
        public int Concepto { get; set; }
        public int DocTipo { get; set; }
        public long DocNro { get; set; }
        public int CbteDesde { get; set; }
        public int CbteHasta { get; set; }
        public string CbteFch { get; set; }
        public string Resultado { get; set; }
        public string CAE { get; set; }
        public string CAEFchVto { get; set; }
    }
    
}