using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using afipServices.src.WSAA.models;
using afipServices.src.WSFE.enums;
using afipServices.src.WSFE.exceptions;
using afipServices.src.WSFE.interfaces;
using afipServices.src.WSFE.models;

namespace afipServices.src.WSFE
{
    public class WSFEService(
        ILogger<WSFEService> logger,
        IHttpClientFactory httpClientFactory
        )
    {   

        public async Task<List<VariableTypeValue>?> GetVariableTypeValues(WSAAAuthToken token, WSFEVariableTypes type)
        {
            try
            {
                string request = GenerateGetVariableValuesRequest(token,  type) ?? throw new WSFEServiceException("Failed to generate a valid Variable Value request");
                
                using HttpClient client = httpClientFactory.CreateClient();
                     
                client.DefaultRequestHeaders.Add("User-Agent", "Apache-HttpClient/4.1.1 (java 1.5)");

                var byteRequest = new StringContent(request, 
                                                    Encoding.UTF8,
                                                    "text/xml"
                                                    );

                byteRequest.Headers.ContentType!.CharSet = "utf-8";

                string requestUri = Environment.GetEnvironmentVariable($"WSFEParamGetUri_{ type }")!;

                HttpResponseMessage response = await client.PostAsync(requestUri, byteRequest);
                string? responseToString = await response.Content.ReadAsStringAsync();

                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    logger.LogInformation("WSFE has responded successfully: StatusCode 200");
                    List<VariableTypeValue> values = ParseGetVariableTypeValuesResponse(responseToString, type) ?? throw new WSFEServiceException("Error parsing WSFEVariableTypeValues response");
                    return values;
                }
                else
                {
                    List<WSFEResponseError> responseErrors = ParseGetVariableTypeValuesError(responseToString);
                    if(responseErrors.Any())
                    {
                        var strResponseErrors = new StringBuilder();
                        foreach(WSFEResponseError error in responseErrors)
                        {
                            strResponseErrors.Append($"\t{ error.ToString() }\n");
                        }
                        throw new WSFEServiceException($"Error requesting WSFEVariableTypeValues: WSFE response contained errors:\nHttp Response Error Code: { response.StatusCode }\nResponse Errors: \n{ strResponseErrors }");
                    }
                    else
                    {
                        throw new WSFEServiceException($"Error requesting WSFEVariableTypeValues: WSFE response failed, no formal errors found: \nHttp Response Error Code: { response.StatusCode }\nFailed Response: \n{ responseToString }");
                    }
                }
                
            }
            catch(Exception e)
            {
                logger.LogError("Failed to get WSFEVariableTypeValues: " + e);
                return null;
            }
            
        }
        public string? GenerateAuthorizeInvoiceRequest(WSAAAuthToken token, Invoice invoice)
        {
            try
            {
                logger.LogInformation("Reading AuthorizeInvoiceRequest.xml template...");
                var authInvoiceRequestXml = new XmlDocument();
                authInvoiceRequestXml.Load("./src/WSFE/xmlModels/requests/AuthorizeInvoiceRequest.xml");
                logger.LogInformation("Successfully loaded AuthorizeInvoiceRequest.xml");
                
                var nsManager = new XmlNamespaceManager(authInvoiceRequestXml.NameTable);
                nsManager.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsManager.AddNamespace("ar", "http://ar.gov.afip.dif.FEV1/");

                authInvoiceRequestXml.SelectSingleNode("//ar:Token", nsManager)!.InnerText = token.Token;
                authInvoiceRequestXml.SelectSingleNode("//ar:Sign", nsManager)!.InnerText = token.Sign;
                authInvoiceRequestXml.SelectSingleNode("//ar:Cuit", nsManager)!.InnerText = token.GetCuit();
                authInvoiceRequestXml.SelectSingleNode("//ar:CantReg", nsManager)!.InnerText = invoice.AmountOfInvoices.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:PtoVta", nsManager)!.InnerText = invoice.PtoVta.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:CbteTipo", nsManager)!.InnerText = invoice.Type.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:Concepto", nsManager)!.InnerText = invoice.Concept.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:DocTipo", nsManager)!.InnerText = invoice.DocumentType.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:DocNro", nsManager)!.InnerText = invoice.DocumentNumber.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:CbteDesde", nsManager)!.InnerText = invoice.FromInvoiceNumber.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:CbteHasta", nsManager)!.InnerText = invoice.ToInvoiceNumber.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:CbteFch", nsManager)!.InnerText = invoice.GenerationDate?.ToString("yyyyMMdd") ?? string.Empty;
                authInvoiceRequestXml.SelectSingleNode("//ar:ImpTotal", nsManager)!.InnerText = invoice.FinalTotalValue.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:ImpTotConc", nsManager)!.InnerText = invoice.NonTaxedNetValue.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:ImpNeto", nsManager)!.InnerText = invoice.TaxedNetValue.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:ImpOpEx", nsManager)!.InnerText = invoice.ExemptValue.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:ImpTrib", nsManager)!.InnerText = invoice.TributeValue.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:ImpIVA", nsManager)!.InnerText = invoice.IVAValue.ToString();
                authInvoiceRequestXml.SelectSingleNode("//ar:FchServDesde", nsManager)!.InnerText = invoice.PaymentStartDate?.ToString("yyyyMMdd") ?? string.Empty;
                authInvoiceRequestXml.SelectSingleNode("//ar:FchServHasta", nsManager)!.InnerText = invoice.PaymentEndDate?.ToString("yyyyMMdd") ?? string.Empty;
                authInvoiceRequestXml.SelectSingleNode("//ar:FchVtoPago", nsManager)!.InnerText = invoice.PaymentExpireDate?.ToString("yyyyMMdd") ?? string.Empty;
                authInvoiceRequestXml.SelectSingleNode("//ar:MonId", nsManager)!.InnerText = invoice.CurrencyId;
                authInvoiceRequestXml.SelectSingleNode("//ar:MonCotiz", nsManager)!.InnerText = invoice.CurrencyValue.ToString();

                //All array item nodes are detailed in the correspondant xmlModel/file.xml for documenting purposes
                if(invoice.AsociatedInvoices != null)
                {
                var cbtesAsocNode = authInvoiceRequestXml.SelectSingleNode("//ar:CbtesAsoc", nsManager);
                    foreach (var asociatedInvoice in invoice.AsociatedInvoices)
                    {
                        var cbteAsocNode = authInvoiceRequestXml.CreateElement("ar", "CbteAsoc", "http://ar.gov.afip.dif.FEV1/");
                        cbteAsocNode.AppendChild(CreateElement(authInvoiceRequestXml, "Tipo", asociatedInvoice.Type.ToString()));
                        cbteAsocNode.AppendChild(CreateElement(authInvoiceRequestXml, "PtoVta", asociatedInvoice.PtoVta.ToString()));
                        cbteAsocNode.AppendChild(CreateElement(authInvoiceRequestXml, "Nro", asociatedInvoice.Number.ToString()));
                        cbteAsocNode.AppendChild(CreateElement(authInvoiceRequestXml, "Cuit", asociatedInvoice.Cuit ?? string.Empty));
                        cbteAsocNode.AppendChild(CreateElement(authInvoiceRequestXml, "CbteFch", asociatedInvoice.GenerationDate?.ToString("yyyyMMdd") ?? string.Empty));
                        cbtesAsocNode!.AppendChild(cbteAsocNode);
                    }
                }

                if(invoice.Tributes != null)
                {
                    var tributosNode = authInvoiceRequestXml.SelectSingleNode("//ar:Tributos", nsManager);
                    foreach (var tribute in invoice.Tributes)
                    {
                        var tributoNode = authInvoiceRequestXml.CreateElement("ar", "Tributo", "http://ar.gov.afip.dif.FEV1/");
                        tributoNode.AppendChild(CreateElement(authInvoiceRequestXml, "Id", tribute.Id.ToString()));
                        tributoNode.AppendChild(CreateElement(authInvoiceRequestXml, "Desc", tribute.Description ?? string.Empty));
                        tributoNode.AppendChild(CreateElement(authInvoiceRequestXml, "BaseImp", tribute.TaxBase.ToString()));
                        tributoNode.AppendChild(CreateElement(authInvoiceRequestXml, "Alic", tribute.Aliquot.ToString()));
                        tributoNode.AppendChild(CreateElement(authInvoiceRequestXml, "Importe", tribute.Value.ToString()));
                        tributosNode!.AppendChild(tributoNode);
                    }
                }

                if(invoice.IVAs != null)
                {
                    var ivaNode = authInvoiceRequestXml.SelectSingleNode("//ar:Iva", nsManager);
                    foreach (var iva in invoice.IVAs)
                    {
                        var ivaItemNode = authInvoiceRequestXml.CreateElement("ar", "AlicIva", "http://ar.gov.afip.dif.FEV1/");
                        ivaItemNode.AppendChild(CreateElement(authInvoiceRequestXml, "Id", iva.Id.ToString()));
                        ivaItemNode.AppendChild(CreateElement(authInvoiceRequestXml, "BaseImp", iva.TaxBase.ToString()));
                        ivaItemNode.AppendChild(CreateElement(authInvoiceRequestXml, "Importe", iva.Value.ToString()));
                        ivaNode!.AppendChild(ivaItemNode);
                    }
                }

                if(invoice.Optionals != null)
                {
                    var opcionalesNode = authInvoiceRequestXml.SelectSingleNode("//ar:Opcionales", nsManager);
                    foreach (var optional in invoice.Optionals)
                    {
                        var opcionalNode = authInvoiceRequestXml.CreateElement("ar", "Opcional", "http://ar.gov.afip.dif.FEV1/");
                        opcionalNode.AppendChild(CreateElement(authInvoiceRequestXml, "Id", optional.Id));
                        opcionalNode.AppendChild(CreateElement(authInvoiceRequestXml, "Valor", optional.Value));
                        opcionalesNode!.AppendChild(opcionalNode);
                    }
                }

                if(invoice.Buyers != null)
                {
                    var compradoresNode = authInvoiceRequestXml.SelectSingleNode("//ar:Compradores", nsManager);
                    foreach (var buyer in invoice.Buyers)
                    {
                        var compradorNode = authInvoiceRequestXml.CreateElement("ar", "Comprador", "http://ar.gov.afip.dif.FEV1/");
                        compradorNode.AppendChild(CreateElement(authInvoiceRequestXml, "DocTipo", buyer.DocumentType.ToString()));
                        compradorNode.AppendChild(CreateElement(authInvoiceRequestXml, "DocNro", buyer.DocumentNumber.ToString()));
                        compradorNode.AppendChild(CreateElement(authInvoiceRequestXml, "Porcentaje", buyer.OwnershipPercentage.ToString()));
                        compradoresNode!.AppendChild(compradorNode);
                    }
                }

                if (invoice.AsociatedPeriod != null && invoice.AsociatedPeriod.Length == 2)
                {
                    authInvoiceRequestXml.SelectSingleNode("//ar:PeriodoAsoc//ar:FchDesde", nsManager)!.InnerText = invoice.AsociatedPeriod[0].ToString("yyyyMMdd");
                    authInvoiceRequestXml.SelectSingleNode("//ar:PeriodoAsoc//ar:FchHasta", nsManager)!.InnerText = invoice.AsociatedPeriod[1].ToString("yyyyMMdd");
                }

                if(invoice.Activities != null)
                {
                    var actividadesNode = authInvoiceRequestXml.SelectSingleNode("//ar:Actividades", nsManager);
                    foreach (var activity in invoice.Activities)
                    {
                        var actividadNode = authInvoiceRequestXml.CreateElement("ar", "Actividad", "http://ar.gov.afip.dif.FEV1/");
                        actividadNode.AppendChild(CreateElement(authInvoiceRequestXml, "Id", activity.Id.ToString()));
                        actividadesNode!.AppendChild(actividadNode);
                    }
                }

                logger.LogInformation("Successfully generated AuthorizeInvoiceRequest.xml");
                return authInvoiceRequestXml.OuterXml;
            }
            catch (Exception e)
            {
                logger.LogError($"Error generating AuthorizeInvoiceRequest.xml: { e }");
                return null;
            }
        }
        public string? GenerateGetVariableValuesRequest(WSAAAuthToken token, WSFEVariableTypes type)
        {
            try
            {
                logger.LogInformation($"Generating Variable Value request for variable: { type } ...");
                var typeRequestXml = new XmlDocument();
                typeRequestXml.Load($"./src/WSFE/xmlModels/requests/variableTypes/{ type }_REQUEST.xml");
                var nsManager = new XmlNamespaceManager(typeRequestXml.NameTable);
                nsManager.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsManager.AddNamespace("ar", "http://ar.gov.afip.dif.FEV1/");
                
                var tokenNode = typeRequestXml.SelectSingleNode("//ar:Token", nsManager);
                var signNode = typeRequestXml.SelectSingleNode("//ar:Sign", nsManager);
                var cuitNode = typeRequestXml.SelectSingleNode("//ar:Cuit", nsManager);

                tokenNode!.InnerText = token.Token;
                signNode!.InnerText = token.Sign;
                cuitNode!.InnerText = token.GetCuit();

                logger.LogInformation($"Successfully generated Variable Value request for variable: { type }");
                return typeRequestXml.OuterXml;
            }
            catch(Exception e)
            {
                logger.LogError("Error generating Variable Value request: " + e);
                return null;
            }

        }
        
        public List<VariableTypeValue> ParseGetVariableTypeValuesResponse(string response, WSFEVariableTypes type)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(response.Trim());
            
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("ar", "http://ar.gov.afip.dif.FEV1/");

            XmlNode? resultGetNode = doc.SelectSingleNode("//ar:ResultGet", nsManager);
            var values = new List<VariableTypeValue>();
            if (resultGetNode != null)
            {
                string variableNode = GetVariableTypeXmlSectionName(type);
                XmlNodeList variableNodes = resultGetNode.SelectNodes($"ar:{ variableNode }", nsManager) ?? throw new WSFEServiceException($"No nodes were found for variable { type }, xmlNode: { variableNode }");
                
                if (variableNodes != null)
                {
                    foreach (XmlNode node in variableNodes)
                    {
                            string? id = node.SelectSingleNode("ar:Id", nsManager)?.InnerText;
                            string? desc = node.SelectSingleNode("ar:Desc", nsManager)?.InnerText;
                            string? fchDesde = node.SelectSingleNode("ar:FchDesde", nsManager)?.InnerText;
                            string? fchHasta = node.SelectSingleNode("ar:FchHasta", nsManager)?.InnerText;

                            //fchHasta (ExpirationDate) is an optional field, hence why i don't check if it's null
                            if(id != null && desc != null && fchDesde != null)
                            {
                                var value = new VariableTypeValue 
                                    {
                                        Type = type.ToString(),
                                        Id = id,
                                        Description = desc,
                                        GenerationDate = fchDesde,
                                        ExpirationDate = fchHasta
                                    };
                                values.Add(value);
                            }
                    }
                }
            }
            return values;
        }
        
        public List<WSFEResponseError> ParseGetVariableTypeValuesError(string failedResponse)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(failedResponse);

            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("ar", "http://ar.gov.afip.dif.fev1/");

            var errors = new List<WSFEResponseError>();

            // Parse Errors section
            XmlNode errorsNode = doc.SelectSingleNode("//ar:Errors", nsManager)!;
            if (errorsNode != null)
            {
                XmlNodeList errNodes = errorsNode.SelectNodes("ar:Err", nsManager)!;

                foreach (XmlNode errNode in errNodes)
                {
                    string code = errNode.SelectSingleNode("ar:Code", nsManager)?.InnerText!;
                    string msg = errNode.SelectSingleNode("ar:Msg", nsManager)?.InnerText!;

                    if(code != null && msg != null)
                    {
                        var newError = new WSFEResponseError
                            {
                                Code = code,
                                Description = msg
                            };
                        errors.Add(newError);
                    }
                }
            }
            return errors;
        }

        //Maybe better to do this through environment variables?
        //The reason i chose this approach was to not pollute code with the terribly named afip native variables
        private string GetVariableTypeXmlSectionName(WSFEVariableTypes type)
        {
            string xmlSectionName = string.Empty;
            switch(type)
            {
                case WSFEVariableTypes.INVOICE_TYPES:
                    xmlSectionName = "CbteTipo";
                    break;
                case WSFEVariableTypes.CONCEPT_TYPES:
                    xmlSectionName = "ConceptoTipo";
                    break;
                case WSFEVariableTypes.DOCUMENT_TYPES:
                    xmlSectionName = "DocTipo";
                    break;
                case WSFEVariableTypes.IVA_TYPES:
                    xmlSectionName = "IvaTipo";
                    break;
                case WSFEVariableTypes.CURRENCY_TYPES:
                    xmlSectionName = "Moneda";
                    break;
                case WSFEVariableTypes.OPTIONAL_TYPES:
                    xmlSectionName = "OpcionalTipo";
                    break;
                case WSFEVariableTypes.TRIBUTE_TYPES:
                    xmlSectionName = "TributoTipo";
                    break;    
            }
            return xmlSectionName;
        }
        
        private XmlElement CreateElement(XmlDocument doc, string name, string value)
        {
            var element = doc.CreateElement(name);
            element.InnerText = value;
            return element;
        } 
    }
}