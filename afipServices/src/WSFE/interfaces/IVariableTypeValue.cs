using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace afipServices.src.WSFE.interfaces
{
    public interface IVariableTypeValue
    {
        public string GetIdentifier();
        public string GetValue();
    }
}