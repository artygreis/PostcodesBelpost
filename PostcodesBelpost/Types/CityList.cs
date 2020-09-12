using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostcodesBelpost
{
    class CityList
    {
        [JsonProperty("citys")]
        public List<City> Cities { get; set; }
    }
}
