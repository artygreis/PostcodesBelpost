using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostcodesBelpost
{
    class City
    {
        [JsonProperty("city_id")]
        public string CityName { get; set; }
    }
}
