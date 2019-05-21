using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NandosArtService.Models;
using NandosArtService.Services;
using System;
using System.Threading.Tasks;

namespace NandosArtService.Middleware
{
    public class ApiKeyValidatorsMiddleware
    {
        private readonly RequestDelegate _next;
        private IApiKeyServices apiKeyRepo { get; set; }
        private ILogger _log { get; set; }

        private string apiKeyConst = "APIKey";

        public ApiKeyValidatorsMiddleware(RequestDelegate next,ILoggerFactory loggerFactory)
        {
            _next = next;
            _log = loggerFactory.CreateLogger<ApiKeyValidatorsMiddleware>();
            apiKeyRepo = new ApiKeyServices(loggerFactory);
        }

        public async Task Invoke(HttpContext httpContext, ArtContext dbContext)
        {           
            try
            {
                // Load API Keys from DB on first hit
                if (!apiKeyRepo.LoadKeys(dbContext))
                {
                    httpContext.Response.StatusCode = 503; //API key not loaded     

                    string msg = "API Key Load error";
                    await httpContext.Response.WriteAsync(msg);
                    _log.LogInformation(msg);

                    return;
                }

                if (!httpContext.Request.Headers.Keys.Contains(apiKeyConst))
                {
                    httpContext.Response.StatusCode = 400; //Bad Request                

                    string msg = "API Key is missing";
                    await httpContext.Response.WriteAsync(msg);
                    _log.LogInformation(msg);

                    return;
                }
                else
                {
                    if (!apiKeyRepo.CheckValidApiKey(httpContext.Request.Headers[apiKeyConst].ToString().Trim()))
                    {
                        httpContext.Response.StatusCode = 401; //UnAuthorized
                        string msg = "Invalid API Key";
                        await httpContext.Response.WriteAsync(msg);
                        _log.LogInformation(msg);
                        return;
                    }
                }
                await _next.Invoke(httpContext);
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
        }
    }

    #region ExtensionMethod
    public static class UserKeyValidatorsExtension
    {
        public static IApplicationBuilder ApplyApiKeyValidation(this IApplicationBuilder app)
        {
            app.UseMiddleware<ApiKeyValidatorsMiddleware>();
            return app;
        }
    }
    #endregion
}