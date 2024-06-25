using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using afipServices.src.Common.enums;
using afipServices.src.WSAA.models;

namespace afipServices.src.TokenManager.interfaces
{
    public interface ITokenManager
    {
        public Task<WSAAAuthToken?> GetAuthToken(AfipService afipService);
        
    }
}