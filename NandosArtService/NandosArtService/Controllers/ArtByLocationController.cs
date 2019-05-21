using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NandosArtService.Models;
using Microsoft.EntityFrameworkCore;
using NandosArtService.Services;
using Microsoft.Extensions.Logging;

namespace NandosArtService.Controllers
{
    [Route("v1/artByLocation/")]
    [ApiController]
    public class ArtByLocationController : ControllerBase
    {

        //private readonly Services.IArtByLocationServices _services;

        private readonly ArtContext _context;
        private readonly ILogger<ArtByLocationController> _log;

        public ArtByLocationController(ArtContext context ,ILogger<ArtByLocationController> log)
        {
            _context = context;
            _log = log;
        }

        // GET: api/artByLocation/5
        [HttpGet("{locationId}")]
        public async Task<ActionResult<IEnumerable<Artwork>>> GetArtworkForLocation(string locationId)
        {
            var artworks = await _context.Artwork.Where(a => a.locationId == locationId).ToListAsync();

            string locationName = _context.RestaurantLocations.Where(x => x.id == locationId).SingleOrDefault()?.locationName;

            try
            {
                if (artworks == null)
                {
                    _log.LogWarning("Artwork by location not found..");
                    return NotFound();
                }
                else
                {
                    //for each entry in artworks, get location name from restaurant location API
                    foreach (Artwork artwork in artworks)
                    {

                        //Get artist name
                        var artists = await _context.Artists.Where(a => a.id == artwork.artistId).ToListAsync();
                        if (artists != null)
                        {
                            //for each entry in artworks, get location name from restaurant location API
                            foreach (Artist artist in artists)
                            {
                                if (artist != null)
                                {
                                    artwork.artistName = artist.artistFirstName + " " + artist.artistSurName;
                                    artwork.artistInfo = artist.artistInfo;
                                }
                                var artistLinks = await _context.ArtistLinks.Where(b => b.artistId == artist.id).ToListAsync();
                                artwork.artistLinks = artistLinks;
                            }
                        }

                        artwork.locationName = locationName;
                    }
                    _log.LogInformation("Retrieved artist,artist links for location :" + locationId);
                }
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return artworks;
        }

    }
}