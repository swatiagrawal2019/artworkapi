using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

namespace NandosArtService.Models
{
    public class Artwork
    {
        public int id { get; set; }

        public int artistId { get; set; }

        public string locationId { get; set; }

        public string artworkName { get; set; }

        public string artworkInfo { get; set; }

        public string displayImageUrl { get; set; }
                      
        [NotMapped]
        public string locationName { get; set; }

        [NotMapped]
        public string artistName { get; set; }

        [NotMapped]
        public string artistInfo { get; set; }

        [NotMapped]
        public double matchScore { get; set; }

        [NotMapped]
        public List<ArtistLinks> artistLinks { get; set; }


      
    }
}
