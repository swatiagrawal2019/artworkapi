using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NandosArtService.Models
{
   public class ArtFinder
    {

        public Nullable<double> latitude { get; set; }

        public Nullable<double> longitude { get; set; }

        public string artImage { get; set; }

        public string locationId { get; set; }


    }
}
