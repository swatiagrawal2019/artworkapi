using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace NandosArtService.Models
{
    public class LocationResult
    {
        public string locationId { get; set; }

        public string locationName { get; set; }

        public string distanceFromLocation { get; set; }

        public bool isCustAtRestaurant { get; set; }
                
        public string distanceUnit { get; set; }

    }
}
