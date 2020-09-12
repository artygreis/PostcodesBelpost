using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostcodesBelpost
{
    class Zipcode
    {
        [JsonProperty("zipcode_id")]
        public string ZipcodeNumber { get; set; }
        [JsonProperty("building_id")]
        public string Building { get; set; }
    }
}
