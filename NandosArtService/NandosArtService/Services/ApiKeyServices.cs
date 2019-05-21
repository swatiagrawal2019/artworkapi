using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NandosArtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NandosArtService.Services
{
    public class ApiKeyServices : IApiKeyServices
    {
        static List<ApiKey> ApiKeyList;
        private readonly ILogger _log;

        public ApiKeyServices(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger<ApiKeyServices>();
        }
        public bool LoadKeys(ArtContext dbContext)
        {
            try
            {
                //Load only once 
                if (ApiKeyList == null)
                {
                    ApiKeyList = dbContext.APIKeys.ToList();
                }
                if (ApiKeyList.Count == 0)
                {
                    return false; //keys not set up in DB 
                }
                _log.LogInformation("Api keys loaded from database..");
            }
            catch (Exception e)
            {
                //Failed to load API keys from DB. Log exception
                _log.LogError(e.Message);
            }
            if (ApiKeyList == null)
            {
                return false;
            }
            return true;
        }

        public bool CheckValidApiKey(string reqkey)
        {
            try
            {
                _log.LogInformation("API key validation...");

                ApiKey FindApiKey = ApiKeyList
                       .Where(e => e.apiKey.Equals(reqkey))
                       .SingleOrDefault();

                if (FindApiKey != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
               
            }
            catch (Exception e)
            {
                //Failed to load API keys from DB. Log exception
                _log.LogError(e.Message);
            }
            return false;
        }
    }
}