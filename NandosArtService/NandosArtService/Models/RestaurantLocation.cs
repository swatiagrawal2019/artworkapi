using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

namespace NandosArtService.Models
{
    public class RestaurantLocation
    {
        public string id { get; set; }

        public string locationName { get; set; }

        public Nullable<double> latitude { get; set; }

        public Nullable<double> longitude { get; set; }

        [NotMapped]
        public Nullable<double> distance { get; set; }

    }
}
