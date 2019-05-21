using NandosArtService.Models;
using System.Collections.Generic;

namespace NandosArtService.Services
{
    public interface IApiKeyServices
    {
        bool LoadKeys(ArtContext dbContext);
        bool CheckValidApiKey(string reqkey);
    }
}