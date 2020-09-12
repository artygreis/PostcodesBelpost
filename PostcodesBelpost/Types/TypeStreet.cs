using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostcodesBelpost
{
    class TypeStreet
    {
        [JsonProperty("typestreet_id")]
        public string TypeStreetName { get; set; }
    }
}
