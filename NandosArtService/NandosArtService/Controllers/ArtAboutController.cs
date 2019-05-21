using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NandosArtService.Models;

namespace NandosArtService.Controllers
{
    [Route("v1/artAbout")]
    [ApiController]
    public class ArtAboutController : ControllerBase
    {
        private readonly ArtContext _context;
        private readonly ILogger<ArtAboutController> _log;

        public ArtAboutController(ArtContext context, ILogger<ArtAboutController> log)
        {
            _context = context;
            _log = log;
        }

        // GET: api/ArtAbout/5
        [HttpGet]
        public async Task<ActionResult<ArtAbout>> GetArtAbout()
        {
            var artAbouts = await _context.ArtApplicationConfig.ToListAsync();
            ArtAbout rtnArtAbout = null;

            try
            {              
                if (artAbouts == null)
                {
                    _log.LogWarning("Art about records not found");
                    return NotFound();
                }
                //there should only be one entry in the table
                foreach (ArtAbout artAbout in artAbouts)
                {
                    rtnArtAbout = artAbout;
                    break;
                }

                _log.LogInformation("Retrieved application's about information");
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return rtnArtAbout;
        }
    }
}
