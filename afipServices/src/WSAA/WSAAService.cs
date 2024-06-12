using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using afipServices.src.Common.enums;
using afipServices.src.Encryption.interfaces;
using afipServices.src.WSAA.exceptions;
using afipServices.src.WSAA.interfaces;
using afipServices.src.WSAA.models;

namespace afipServices.src.WSAA
{
    public class WSAAService(
        ILogger<WSAAService> logger, 
        IEncryptionManager encryptionManager,
        IHttpClientFactory httpClientFactory
        ) : IWSAAService
    {
        public async Task<WSAAAuthToken> GetAuthenticationToken(AfipService service)
        {
            try
            {
                string encryptedLoginTicket = encryptionManager.GetEncryptedLoginTicketRequest(service) ?? throw new WSAAServiceException("Failed to fetch a WSAA Authentication token: Failed to encrypt LoginRequest");
                string request =  GetLoginCmsRequest(encryptedLoginTicket) ?? throw new WSAAServiceException("Error generating LoginCmsRequest");
                
                using HttpClient client = httpClientFactory.CreateClient();
                

                //await client.PostAsync    
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string? GetLoginCmsRequest(string encryptedLoginTicket)
        {
            try
            {
                logger.LogInformation("Reading RequestLoginCms.xml template...");
                var xmlRequestLoginCms = new XmlDocument();
                xmlRequestLoginCms.Load("./src/WSAA/xmlModels/RequestLoginCms.xml");
                logger.LogInformation("Successfully loaded RequestLoginCms.xml");
                
                XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlRequestLoginCms.NameTable);
                nsManager.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsManager.AddNamespace("wsaa", "http://wsaa.view.sua.dvadac.desein.afip.gov");

                logger.LogInformation("Getting RequestLoginCms.xml node: { node }", "in0");
                var xmlNodeIn0 = xmlRequestLoginCms.SelectSingleNode("//wsaa:in0", nsManager);
                xmlNodeIn0!.InnerText = encryptedLoginTicket;
                logger.LogInformation("Successfully generated RequestLoginCms.xml");
                
                return  xmlRequestLoginCms.OuterXml;
            }
            catch(Exception e)
            {
                logger.LogError("Failed to generate LoginCmsRequest: " + e);
                return null;
            }
        }
    }
}