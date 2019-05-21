using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NandosArtService.Models;
using NandosArtService.Services;

namespace NandosArtService.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ArtworkController : ControllerBase
    {
        private readonly ArtContext _context;
        private readonly ILogger<ArtworkController> _log;

        public ArtworkController(ArtContext context, ILogger<ArtworkController> log)
        {
            _context = context;
            _log = log;
        }

        // GET: api/artwork/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Artwork>> GetArtwork(int id)
        {
            var artwork = await _context.Artwork.FindAsync(id);

            try
            {
                if (artwork == null)
                {
                    _log.LogWarning("Artwork by id not found..");
                    return NotFound();

                }
                else
                {
                    var locations = await _context.RestaurantLocations.Where(b => b.id == artwork.locationId).ToListAsync();
                    foreach (RestaurantLocation location in locations)
                    {
                        artwork.locationName = location.locationName;
                        break;
                    }

                    var artists = await _context.Artists.Where(a => a.id == artwork.artistId).ToListAsync();
                    if (artists != null)
                    {
                        //for each entry in artworks, get location name from restaurant location API
                        foreach (Artist artist in artists)
                        {
                            artwork.artistName = artist.artistFirstName + " " + artist.artistSurName;
                            artwork.artistInfo = artist.artistInfo;
                            var artistLinks = await _context.ArtistLinks.Where(b => b.artistId == artist.id).ToListAsync();
                            artwork.artistLinks = artistLinks;
                        }
                    }
                    _log.LogInformation("Retrieved artwork, artist and artistlinks for artwork :" + id);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return artwork;
        }
    }
}
