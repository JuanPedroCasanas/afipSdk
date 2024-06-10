using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace afipServices.src
{
    public class deserialization
    {

        public void deserialize()
        {
            string xml = "./asd.xml";

            try
            {
                using (StringReader stringReader = new StringReader(File.ReadAllText(xml)))
                {
                    using (XmlReader xmlReader = XmlReader.Create(stringReader))
                    {
                        
                        // Navigate to the FECAESolicitarResult element
                        xmlReader.ReadToFollowing("FECAEDetResponse", "http://ar.gov.afip.dif.FEV1/");

                        // Deserialize the FECAESolicitarResult element
                        XmlSerializer serializer = new XmlSerializer(typeof(test));
                        var result = (test)serializer.Deserialize(xmlReader);

                        // Use the deserialized object
                        Console.WriteLine($"Cuit: {result.CAE == null}");
                        Console.WriteLine($"Estado: {result.Resultado}");

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}

