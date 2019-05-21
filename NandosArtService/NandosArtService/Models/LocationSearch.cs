using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NandosArtService.Models
{
    public class LocationSearch
    {
        public string locationName { get; set; }

        public Nullable<double> latitude { get; set; }

        public Nullable<double> longitude { get; set; }
    }
}
