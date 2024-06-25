using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using afipServices.src.WSAA.models;
using afipServices.src.WSFE.enums;
using afipServices.src.WSFE.model;

namespace afipServices.src.WSFE.interfaces
{
    public interface IWSFEService
    {
        public Invoice LegalizeInvoice(WSAAAuthToken token, Invoice invoiceToLegalize);

        public Invoice? FetchInvoice();

        public Invoice? GetLastGeneratedInvoice();

        public List<IVariableTypeValue> GetVariableValues(WSFEVariableTypes type);

        
    }
}