using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using afipServices.src.Common.enums;

namespace afipServices.src.Encryption.interfaces
{
    public interface IEncryptionManager
    {
        string? GetEncryptedLoginTicketRequest(AfipService afipService);
    }
}