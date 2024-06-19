using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using afipServices.src.Common.enums;
using afipServices.src.WSAA.models;

namespace afipServices.src.WSAA.interfaces
{
    public interface IWSAAService
    {
        Task<WSAAAuthToken?> GetAuthenticationToken(AfipService service);
    }
}