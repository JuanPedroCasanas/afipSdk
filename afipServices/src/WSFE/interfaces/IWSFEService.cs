using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using afipServices.src.WSAA.models;
using afipServices.src.WSFE.enums;
using afipServices.src.WSFE.models;

namespace afipServices.src.WSFE.interfaces
{
    public interface IWSFEService
    {
        public Invoice AuthorizeInvoice(WSAAAuthToken token, Invoice invoiceToAuthorize);

        public Invoice? GetInvoice(WSAAAuthToken token, int invoiceNumber);

        public Invoice? GetLastAuthorizedInvoice(WSAAAuthToken token, string cuit, int ptoVta, int invoiceType);

        public List<VariableTypeValue> GetVariableValues(WSAAAuthToken token, WSFEVariableTypes type);

        
    }
}