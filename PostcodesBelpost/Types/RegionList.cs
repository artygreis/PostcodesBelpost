using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostcodesBelpost
{
    class RegionList
    {
        [JsonProperty("regions")]
        public List<Region> Regions { get; set; }
    }
}
