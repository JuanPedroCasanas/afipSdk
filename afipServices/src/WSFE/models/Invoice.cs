using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.models
{
    public class Invoice
    {
        public string Cuit { get; set; } = string.Empty;

        public bool IsAuthorized { get; set; } = false;

        //If IsAuthorized == true then these fields are filled:
        //*--------------------------------------------*//
        public string? DateOfAuthorization { get; set; }
        public string? CAE { get; set; }
        public string? CAEExpirationDate { get; set; }
        public List<AuthObservation>? AuthObservations { get; set; }
        public List<AuthEvent>? AuthEvents { get; set; }
        public List<AuthError>? AuthErrors { get; set; }
        //*--------------------------------------------*//

        //Invoice Header (FeCabReq in Afip doc)
        //*--------------------------------------------*//
        public int AmountOfInvoices { get; set; } 
        //Must be the same for all asociated invoices
        public int Type { get; set; }
        //Punto de venta
        public int PtoVta { get; set; }
        //*--------------------------------------------*//

        //Invoice Details (FeDetReq in Afip doc)
        //*--------------------------------------------*//
        public int Concept { get; set; }
        public int DocumentType { get; set; }
        public long DocumentNumber { get; set; }
        public int FromInvoiceNumber { get; set; }
        public int ToInvoiceNumber { get; set; }
        //Must be "YYYYMMDD" Format when sending the XML Request
        public DateTime? GenerationDate { get; set; }
        //ImpTotal
        public double FinalTotalValue { get; set; } //TaxedNetValue + NonTaxedNetValue + ExcemptValue + IVA + Tributes
        //ImpTotConc
        public double NonTaxedNetValue { get; set; }
        //ImpNeto
        public double TaxedNetValue { get; set; }
        //ImpOpEx
        public double ExemptValue { get; set; }
        //ImpIVA
        public double IVAValue { get; set; }
        //ImpTrib
        public double TributeValue { get; set; }
        //FchServDesde
        //Must be "YYYYMMDD" Format when sending the XML Request
        public DateTime? PaymentStartDate { get; set; }
        //FchServHasta
        //Must be "YYYYMMDD" Format when sending the XML Request
        public DateTime? PaymentEndDate { get; set; }
        public DateTime? PaymentExpireDate { get; set; }
        public string CurrencyId { get; set; } = "PES"; //Pesos argentinos as default value
        public double CurrencyValue { get; set; } = 1; //Pesos argentinos as default value
        public List<AsociatedInvoice>? AsociatedInvoices { get; set; }
        public List<Tribute>? Tributes { get; set; }
        public List<IVA>? IVAs { get; set; }
        public List<Optional>? Optionals { get; set; }
        public List<Buyer>? Buyers { get; set; }
        //Must be a 2 size array
        public DateTime[]? AsociatedPeriod { get; set; }
        public List<Activity>? Activities { get; set; }
        //*--------------------------------------------*//


        public void AddTribute(Tribute tribute)
        {
            if(Tributes == null) 
            {
                Tributes = new List<Tribute>();
            }
            Tributes.Add(tribute);
        }

        public void AddAsociatedInvoice(AsociatedInvoice asociatedInvoice)
        {
            if(AsociatedInvoices == null) 
            {
                AsociatedInvoices = new List<AsociatedInvoice>();
            }
            AsociatedInvoices.Add(asociatedInvoice);
        }

        public void AddIVA(IVA IVAToAdd)
        {
            if(IVAs == null)
            {
                IVAs = new List<IVA>();
            }
            IVAs.Add(IVAToAdd);
        }

        public void AddOptional(Optional optional)
        {
            if(Optionals == null)
            {
                Optionals = new List<Optional>();
            }
            Optionals.Add(optional);

        }

        public void AddActivity(Activity activity)
        {
            if(Activities == null)
            {
                Activities = new List<Activity>();
            }
            Activities.Add(activity);
        }

        public void AddBuyer(Buyer buyer)
        {
            if(Buyers == null)
            {
                Buyers = new List<Buyer>();
            }
            Buyers.Add(buyer);
        }

        public void AddObservation(AuthObservation authObservation)
        {
            if(AuthObservations == null)
            {
                AuthObservations = new List<AuthObservation>();
            }
            AuthObservations.Add(authObservation);
        }

        public void AddAuthEvent(AuthEvent authEvent)
        {
            if(AuthEvents == null)
            {
                AuthEvents = new List<AuthEvent>();
            }
            AuthEvents.Add(authEvent);
        }

        public void AddAuthError(AuthError authError)
        {
            if(AuthErrors == null)
            {
                AuthErrors = new List<AuthError>();
            }
            AuthErrors.Add(authError);
        }
    }
}