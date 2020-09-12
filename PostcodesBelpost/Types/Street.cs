using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostcodesBelpost
{
    class Street
    {
        [JsonProperty("street_id")]
        public string StreetName { get; set; }
    }
}
