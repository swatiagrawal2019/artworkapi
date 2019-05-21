using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NandosArtService.Models
{
    public class Artist
    {
        public int id { get; set; }
               
        public string artistSurName { get; set; }
               
        public string artistFirstName { get; set; }

        public string artistImageURL { get; set; }        

        public string artistInfo { get; set; }        

        public List<ArtistLinks> artistLinks { get; set; }

        public int artistBioConsent { get; set; }

        public List<Artwork> artwork { get; set; }
    }

    public class ArtistLinks
    {
        public int id { get; set; }

        public int artistId { get; set; }

        public string linkType { get; set; }

        public string url { get; set; }


    }


}
