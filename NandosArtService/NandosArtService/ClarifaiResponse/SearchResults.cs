using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NandosArtService.ClarifaiResponse
{
    public class SearchResults
    {
        public Status status { get; set; }

        public string id { get; set; }

        public List<Hit> hits { get; set; }

    }
}
