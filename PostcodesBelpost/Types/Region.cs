using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostcodesBelpost
{
    class Region
    {
        [JsonProperty("region_id")]
        public string RegionName { get; set; }
    }
}
