using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using afipServices.src.Common.enums;
using afipServices.src.TokenManager.exceptions;
using afipServices.src.WSAA.interfaces;
using afipServices.src.WSAA.models;

namespace afipServices.src.TokenManager.interfaces
{
    public class TokenManager(
        IWSAAService WSAAService,
        ILogger<TokenManager> logger
        ) : ITokenManager
    {
        public async Task<WSAAAuthToken?> GetAuthToken(AfipService afipService)
        {
            logger.LogInformation($"Fetching local token for { afipService } service...");
            WSAAAuthToken? token = GetLocalToken(afipService);
            if(token == null || token.IsExpired())
            {
                logger.LogInformation("Fetching a new token from WSAA Service...");
                try
                {
                    token = await GetRefreshedToken(afipService) ?? throw new TokenManagerException($"Failed to fetch a refreshed token from WSAAService for service { afipService }");
                    logger.LogInformation("Successfully fetched token from WSAA Service");
                    logger.LogInformation($"Storing local token for { afipService } service...");
                    StoreLocalToken(token, afipService);
                    logger.LogInformation($"Successfully stored token for { afipService } service");
                }
                catch (Exception e)
                {
                    logger.LogError($"Error fetching or storing a refreshed auth token for { afipService } service: { e }");
                    token = null;
                }
            }
            logger.LogInformation($"Successfully fetched token for { afipService } service");
            return token;
        }


        private WSAAAuthToken? GetLocalToken(AfipService afipService)
        {
            string tokenFolder = Environment.GetEnvironmentVariable("TokensDir")!;
            string tokenDir = tokenFolder + $"/{ afipService }Token.xml";

            if(!File.Exists(tokenDir))
            {
                logger.LogInformation($"Locally stored token for { afipService } service was not found");
                return null;
            }
            else
            {
                WSAAAuthToken localToken = ReadLocalToken(tokenDir);
                if(localToken.IsExpired())
                {
                    logger.LogInformation($"Locally stored token for { afipService } service was expired on { localToken.ExpirationTime }");
                }
                return localToken;
            }
        }

        private WSAAAuthToken ReadLocalToken(string tokenDir)
        {
            using FileStream tokenFile = new FileStream(tokenDir, FileMode.Open, FileAccess.Read);
            XmlSerializer formatter = new XmlSerializer(typeof(WSAAAuthToken));
            return (WSAAAuthToken)formatter.Deserialize(tokenFile)!;
        }

        private void StoreLocalToken(WSAAAuthToken tokenObj, AfipService afipService)
        {
            string tokenFolder = Environment.GetEnvironmentVariable("TokensDir")!;
            string tokenDir = tokenFolder + $"/{ afipService }Token.xml";

            using (FileStream tokenFile = new FileStream(tokenDir, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                XmlSerializer formatter = new XmlSerializer(tokenObj.GetType());
                formatter.Serialize(tokenFile, tokenObj);
            }
        }

        private async Task<WSAAAuthToken?> GetRefreshedToken(AfipService afipService)
        {
            WSAAAuthToken? newToken = await WSAAService.GetAuthenticationToken(afipService);
            return newToken;
        }
    }
}