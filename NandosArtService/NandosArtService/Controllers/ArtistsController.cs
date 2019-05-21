using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NandosArtService.Models;
using NandosArtService.Services;

namespace NandosArtService.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ArtistsController : ControllerBase
    {
        private readonly ArtContext _context;
        private readonly ILogger<ArtistsController> _log;

        public ArtistsController(ArtContext context, ILogger<ArtistsController> log)
        {
            _context = context;
            _log = log;
        }

        // GET: api/Artists
        [HttpGet]        
        public async Task<ActionResult<IEnumerable<Artist>>> GetArtist()
        {
            var artists = await _context.Artists.ToListAsync();
           

            try
            {        
                if (artists != null)
                {
                    
                    //for each entry in artists, get artist links
                    foreach (Artist artist in artists)
                    {
                        if (artist.artistBioConsent == 1) //check if artist has given consent
                        {
                            var artistLinks = await _context.ArtistLinks.Where(a => a.artistId == artist.id).ToListAsync();
                            artist.artistLinks = artistLinks;
                        }
                        else  
                        {
                            //send null if consent is not there

                            artist.artistInfo = null;
                            artist.artistImageURL = null;
                        }
                    }
                    _log.LogInformation("All artist retrieved..");
                }                
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return artists;
        }

        // GET: api/Artists/5
        [HttpGet("{artistId}")]
        public async Task<ActionResult<Artist>> GetArtist(int artistId)
        {
            var artist = await _context.Artists.FindAsync(artistId);

            try
            {                             
                if (artist == null)
                {
                    _log.LogWarning("Artist record not found..");
                    var message = string.Format("Artist with id = {0} not found", artistId);
                    
                    return NotFound();
                }
                else
                {
                    if (artist.artistBioConsent == 1) //check if artist consent is there or not
                    {
                        var artistLinks = await _context.ArtistLinks.Where(a => a.artistId == artist.id).ToListAsync();
                        artist.artistLinks = artistLinks;
                    }
                    else 
                    {
                        //send null if consent is not there
                        artist.artistImageURL = null;
                        artist.artistInfo = null;

                    }
                    var artworks = await _context.Artwork.Where(a => a.artistId == artist.id).ToListAsync();

                    if (artworks != null)
                        {
                            //for each entry in artworks, get location name from restaurant location API
                            foreach (Artwork artwork in artworks)
                            {
                                artwork.artistName = artist.artistFirstName + " " + artist.artistSurName;

                                var locations = await _context.RestaurantLocations.Where(b => b.id == artwork.locationId).ToListAsync();
                                foreach (RestaurantLocation location in locations)
                                {
                                    artwork.locationName = location.locationName;
                                    break;
                                }
                            }
                        }
                        artist.artwork = artworks;  

                    _log.LogInformation("Retrieved artist for id :" + artistId);
                }               
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return artist;
        }
    }
}
