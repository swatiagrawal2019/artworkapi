using Microsoft.EntityFrameworkCore;
using NandosArtService.Models;
using System;

namespace NandosArtService.Models
{ 
    public class ArtContext : DbContext
    {
        public ArtContext(DbContextOptions<ArtContext> options)
            : base(options)
        {
        }

        public DbSet<Artwork> Artwork { get; set; }

        public DbSet<Artist> Artists { get; set; }

        public DbSet<ArtistLinks> ArtistLinks { get; set; }

        public DbSet<ArtAbout> ArtApplicationConfig { get; set; }

        public DbSet<ApiKey> APIKeys { get; set; }

        public DbSet<RestaurantLocation> RestaurantLocations { get; set; }

       

    }
}
