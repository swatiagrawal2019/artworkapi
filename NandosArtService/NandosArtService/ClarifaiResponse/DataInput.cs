using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NandosArtService.ClarifaiResponse
{
    public class DataInput
    {
        public string id { get; set; }

        public string created_at { get; set; }

        public ImageData data { get; set; }

        public Status status { get; set; }

    }
}
