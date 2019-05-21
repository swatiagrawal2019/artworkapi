using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NandosArtService.Models;

//to be removed
namespace NandosArtService.Services
{
    public class ArtWorkLocationServices : IArtWorkLocationServices
    {
        public string GetArtworkLocationName(string LocationId)
        {
            //Call restaurant API - To do
            string _location = @"Call restaurant Location API for loc id:" + LocationId;
            return _location;
        }

    }
}
