using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<WSAAAuthToken?> GetAuthenticationToken(AfipService service)
        {
            try
            {
                string encryptedLoginTicket = encryptionManager.GetEncryptedLoginTicketRequest(service) ?? throw new WSAAServiceException("Failed to fetch a WSAA Authentication token: Failed to encrypt LoginRequest");
                string request = GetLoginCmsRequest(encryptedLoginTicket) ?? throw new WSAAServiceException("Error generating LoginCmsRequest");
                using HttpClient client = httpClientFactory.CreateClient();
                     
                client.DefaultRequestHeaders.Add("SOAPAction", "loginCMS");
                client.DefaultRequestHeaders.Add("User-Agent", "Apache-HttpClient/4.1.1 (java 1.5)");

                var byteRequest = new StringContent(request, 
                                                    Encoding.UTF8,
                                                    "text/xml"
                                                    );

                byteRequest.Headers.ContentType!.CharSet = "utf-8";

                //C# Logs these actions automatically
                string requestUri = Environment.GetEnvironmentVariable("WSAALoginCmsUri")!;
                
                HttpResponseMessage response = await client.PostAsync(requestUri, byteRequest);
                string? responseToString = await response.Content.ReadAsStringAsync();

                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    logger.LogInformation("WSAA has responded successfully: StatusCode 200");
                    WSAAAuthToken token = ParseResponse(responseToString) ?? throw new WSAAServiceException("Error generating WSAAAuthToken: 'NULL' value inside LoginCmsResponse Field");
                    return token;
                }
                else
                {
                    WSAAResponseError responseError = ParseError(responseToString);
                    throw new WSAAServiceException($"Error generating WSAAAuthToken: WSAA response contained faults:\nHttp Response Error Code: { response.StatusCode }\n{ responseError.ToString() }");
                }
                
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get WSAA Authentication token: " + e);
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
                
                return xmlRequestLoginCms.OuterXml;
            }
            catch(Exception e)
            {
                logger.LogError("Failed to generate LoginCmsRequest: " + e);
                return null;
            }
        }
    
        private WSAAAuthToken? ParseResponse(string response)
        {
            var xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(response);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlResponse.NameTable);
            nsManager.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("wsaa", "http://wsaa.view.sua.dvadac.desein.afip.gov");

            XmlNode? loginCmsReturnNode = xmlResponse.SelectSingleNode("//wsaa:loginCmsReturn", nsManager);
            if (loginCmsReturnNode == null) return null;
            
            var innerXml = new XmlDocument();
            innerXml.LoadXml(loginCmsReturnNode.InnerText.Trim());

            XmlNode? headerNode = innerXml.SelectSingleNode("//header");
            XmlNode? credentialsNode = innerXml.SelectSingleNode("//credentials");

            if (headerNode == null || credentialsNode == null) return null;

            XmlNode? source = headerNode.SelectSingleNode("source");
            XmlNode? destination = headerNode.SelectSingleNode("destination");
            XmlNode? uniqueId = headerNode.SelectSingleNode("uniqueId");
            XmlNode? generationTime = headerNode.SelectSingleNode("generationTime");
            XmlNode? expirationTime = headerNode.SelectSingleNode("expirationTime");
            XmlNode? token = credentialsNode.SelectSingleNode("token");
            XmlNode? sign = credentialsNode.SelectSingleNode("sign");

            if (source == null ||
                destination == null ||
                uniqueId == null ||
                generationTime  == null ||
                expirationTime == null ||
                token == null ||
                sign == null ||
                string.IsNullOrEmpty(source.InnerText) ||
                string.IsNullOrEmpty(destination.InnerText) ||
                string.IsNullOrEmpty(uniqueId.InnerText) ||
                string.IsNullOrEmpty(generationTime.InnerText) ||
                string.IsNullOrEmpty(expirationTime.InnerText) ||
                string.IsNullOrEmpty(token.InnerText) ||
                string.IsNullOrEmpty(sign.InnerText))
            {
                return null;
            }

            if (!DateTime.TryParse(generationTime.InnerText, out DateTime generationTimeDate) ||
                !DateTime.TryParse(expirationTime.InnerText, out DateTime expirationTimeDate))
            {
                return null;
            }

            return new WSAAAuthToken
            {
                Source = source.InnerText,
                Destination = destination.InnerText,
                UniqueId = uniqueId.InnerText,
                GenerationTime = generationTimeDate,
                ExpirationTime = expirationTimeDate,
                Token = token.InnerText,
                Sign = sign.InnerText
            };


        }

        private WSAAResponseError ParseError(string failedResponse)
        {
            logger.LogInformation("Parsing WSAA failed response errors...");

            var xmlFailedResponse = new XmlDocument();
            xmlFailedResponse.LoadXml(failedResponse);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlFailedResponse.NameTable);
            nsManager.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");

            var xmlSoapFault = xmlFailedResponse.SelectSingleNode("//soapenv:Fault", nsManager);

            var xmlNodeFaultCode = xmlSoapFault!.SelectSingleNode("faultcode");
            var xmlNodeFaultString = xmlSoapFault!.SelectSingleNode("faultstring");

            return new WSAAResponseError {
                Code = xmlNodeFaultCode!.InnerText,
                Description = xmlNodeFaultString!.InnerText
            };
            
        }
    }
}