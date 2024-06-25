using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using afipServices.src.Common.enums;
using afipServices.src.Encryption.interfaces;

namespace afipServices.src.Encryption
{
    /*
    Handles the whole encrypting process in order to get a LoginTicketRequest.xml request ready for the WSAA server to correctly receive it
    Returns a Base64 encoded string if everything goes well
    Returns null if anything fails in the process 
    */
    public class EncryptionManager (ILogger<EncryptionManager> logger) : IEncryptionManager 
    {

        private string? GenerateLoginTicketRequestXml(AfipService afipService)
        {
            try
            {
                logger.LogInformation("Reading LoginTicketRequest.xml template...");
                var xmlLoginTicketRequest = new XmlDocument();
                xmlLoginTicketRequest.Load("./src/Encryption/xmlModels/LoginTicketRequest.xml");
                logger.LogInformation("Successfully loaded LoginTicketRequest.xml");
                var xmlNodeUniqueId = xmlLoginTicketRequest.SelectSingleNode("//uniqueId");
                var xmlNodeGenerationTime = xmlLoginTicketRequest.SelectSingleNode("//generationTime");
                var xmlNodeExpirationTime = xmlLoginTicketRequest.SelectSingleNode("//expirationTime");
                var xmlNodeService = xmlLoginTicketRequest.SelectSingleNode("//service");

                var generationTime = DateTime.Now.AddMinutes(-10);
                var expirationTime = DateTime.Now.AddMinutes(+10);

                xmlNodeGenerationTime!.InnerText = generationTime.ToString("s");
                xmlNodeExpirationTime!.InnerText = expirationTime.ToString("s");

                long uniqueId = new DateTimeOffset(generationTime).ToUnixTimeSeconds();

                xmlNodeUniqueId!.InnerText = uniqueId.ToString();
                xmlNodeService!.InnerText = afipService.ToString();

                logger.LogInformation(
                    "Successfully generated LoginTicketRequest.xml, Ticket info:\n\tUniqueId: { uniqueId }\n\tService: { service }\n\tGenTime: { genTime }\n\tExpTime: { expTime }",
                    xmlNodeUniqueId.InnerText,
                    xmlNodeService.InnerText,
                    xmlNodeGenerationTime.InnerText,
                    xmlNodeExpirationTime.InnerText
                    );

                return xmlLoginTicketRequest.OuterXml;
            }
            catch(Exception e)
            {
                logger.LogError("Failed to generate LoginTicketRequest.xml: " + e);
                return null;
            }
        }

        private byte[]? GenerateCMSRequest(string LoginTicketRequestXml)
        {
            try
            {
                logger.LogInformation("Encoding LoginTicketRequest.xml to UTF-8 bytes...");
                Encoding EncodedMsg = Encoding.UTF8;
                byte[] loginTicketRequestBytes = EncodedMsg.GetBytes(LoginTicketRequestXml);
                logger.LogInformation("Successfully encoded LoginTicketRequest.xml to UTF-8 bytes");
                return loginTicketRequestBytes;
            }
            catch(Exception e)
            {
                logger.LogError("Failed to encode LoginTicketRequest.xml to UTF8 bytes: " + e);
                return null;
            }
        }

        private X509Certificate2? GetX509CertificateFromFile()
        {
            try
            {
                logger.LogInformation("Fetching X509Certificate from file...");
                string certDir = Environment.GetEnvironmentVariable("X509CertDir")!;
                string? insecureCertPassword = Environment.GetEnvironmentVariable("X509CertPassword");

                X509Certificate2 certificate;

                if(insecureCertPassword != null)
                {
                    logger.LogInformation("Securing X509Certificate password...");
                    SecureString certPassword = new SecureString();
                    foreach (char c in insecureCertPassword)
                    {
                        certPassword.AppendChar(c);
                    }
                    certPassword.MakeReadOnly();
                    logger.LogInformation("Generating password protected X509Certificate object...");
                    certificate = new X509Certificate2(File.ReadAllBytes(certDir), certPassword);
                }
                else
                {
                    logger.LogInformation("Generating X509Certificate object without password...");
                    certificate = new X509Certificate2(File.ReadAllBytes(certDir));
                }
                logger.LogInformation("Successfully fetched X509Certificate from file");
                return certificate;
            }
            catch(Exception e)
            {
                logger.LogError("Failed to fetch X509Certificate from file: " + e);
                return null;
            }
            
        }

        private byte[]? SignCMSRequest(byte[] CMSRequest, X509Certificate2 certificate)
        {
            try
            {
            logger.LogInformation("Generating ContentInfoCMSRequest...");
            //ContentInfo obj is required to generate a signedcms obj
            ContentInfo contentInfoCMSRequest = new ContentInfo(CMSRequest);
            SignedCms signedCMSRequest = new SignedCms(contentInfoCMSRequest);
            logger.LogInformation("Generating cms signee with X509Certificate...");
            CmsSigner signeeCertificate = new CmsSigner(certificate);
            signeeCertificate.IncludeOption = X509IncludeOption.EndCertOnly;
            
            logger.LogInformation("Computing cms signature...");
            // Firmo el mensaje PKCS #7
            signedCMSRequest.ComputeSignature(signeeCertificate);
            logger.LogInformation("Succesfully signed CMSRequest");
            // Encodeo el mensaje PKCS #7.
            return signedCMSRequest.Encode();
            }
            catch(Exception e)
            {
                logger.LogError("Failed to sign CMSRequest: " + e);
                return null;
            }


        }

        private string? EncodeCMSRequest(byte[] signedCMSRequest)
        {
            try
            {
            logger.LogInformation("Encoding signed CMS Request to Base64...");
            string encodedSignedCMSRequest = Convert.ToBase64String(signedCMSRequest);
            logger.LogInformation("Successfully encoded signed CMS Request to Base64");
            return encodedSignedCMSRequest;
            }
            catch(Exception e)
            {
                logger.LogError("Failed to encode signed CMS Request to Base64: " + e);
                return null;
            }
        }

        public string? GetEncryptedLoginTicketRequest(AfipService afipService)
        {
            logger.LogInformation("Starting LoginTicketRequest encrypting process...");

            string? xmlLoginTicketRequest = GenerateLoginTicketRequestXml(afipService);
            if (xmlLoginTicketRequest == null) return null;

            byte[]? CMSLoginTicketRequest = GenerateCMSRequest(xmlLoginTicketRequest);
            if (CMSLoginTicketRequest == null) return null;

            X509Certificate2? x509Certificate = GetX509CertificateFromFile();
            if (x509Certificate == null) return null;

            byte[]? signedCMSLoginTicketRequest = SignCMSRequest(CMSLoginTicketRequest, x509Certificate);
            if (signedCMSLoginTicketRequest == null) return null;

            string? base64EncodedSignedCMSLoginTicketRequest = EncodeCMSRequest(signedCMSLoginTicketRequest);
            if (base64EncodedSignedCMSLoginTicketRequest == null) return null;

            return base64EncodedSignedCMSLoginTicketRequest;
        }

    }
}