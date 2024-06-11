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

namespace afipServices.src.Encryption
{
    public static class EncryptionManager
    {

        private static string GenerateLoginTicketRequestXml(AfipService afipService)
        {

            var xmlLoginTicketRequest = new XmlDocument();
            //xmlLoginTicketRequest.Load(Environment.GetEnvironmentVariable("EncryptionXmlModelsDir")! + "LoginTicketRequest.xml");
            xmlLoginTicketRequest.Load("./src/Encryption/xmlModels/LoginTicketRequest.xml");
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

            return xmlLoginTicketRequest.OuterXml;
        }

        private static byte[] GenerateCMSRequest(string LoginTicketRequestXml)
        {
            Encoding EncodedMsg = Encoding.UTF8;
            byte[] loginTicketRequestBytes = EncodedMsg.GetBytes(LoginTicketRequestXml);
            return loginTicketRequestBytes;
        }

        private static X509Certificate2 GetX509CertificateFromFile()
        {
            
            string certDir = Environment.GetEnvironmentVariable("X509CertDir")!;
            string? insecureCertPassword = Environment.GetEnvironmentVariable("X509CertPassword");

            X509Certificate2 certificate;

            if(insecureCertPassword != null)
            {
                SecureString certPassword = new SecureString();
                foreach (char c in insecureCertPassword)
                {
                    certPassword.AppendChar(c);
                }
                certPassword.MakeReadOnly();
                certificate = new X509Certificate2(File.ReadAllBytes(certDir), certPassword);
            }
            else
            {
                certificate = new X509Certificate2(File.ReadAllBytes(certDir));
            }
            return certificate;
            
        }

        private static byte[] SignCMSRequest(byte[] CMSRequest)
        {
            //ContentInfo obj is required to generate a signedcms obj
            ContentInfo contentInfoCMSRequest = new ContentInfo(CMSRequest);
            SignedCms signedCMSRequest = new SignedCms(contentInfoCMSRequest);

            CmsSigner signeeCertificate = new CmsSigner(GetX509CertificateFromFile());
            signeeCertificate.IncludeOption = X509IncludeOption.EndCertOnly;
            
            // Firmo el mensaje PKCS #7
            signedCMSRequest.ComputeSignature(signeeCertificate);
            // Encodeo el mensaje PKCS #7.
            return signedCMSRequest.Encode();

        }

        private static string EncodeCMSRequest(byte[] signedCMSRequest)
        {
            string encodedSignedCMSRequest = Convert.ToBase64String(signedCMSRequest);
            return encodedSignedCMSRequest;
        }

        public static string GetEncryptedLoginTicketRequest(AfipService afipService)
        {
            string xmlLoginTicketRequest = GenerateLoginTicketRequestXml(afipService);
            byte[] CMSLoginTicketRequest = GenerateCMSRequest(xmlLoginTicketRequest);
            byte[] signedCMSLoginTicketRequest = SignCMSRequest(CMSLoginTicketRequest);
            string base64EncodedSignedCMSLoginTicketRequest = EncodeCMSRequest(signedCMSLoginTicketRequest);

            return base64EncodedSignedCMSLoginTicketRequest;
        }

    }
}