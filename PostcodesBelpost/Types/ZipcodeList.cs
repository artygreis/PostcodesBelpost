using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostcodesBelpost
{
    class ZipcodeList
    {
        [JsonProperty("zipcodes")]
        public List<Zipcode> Zipcodes { get; set; }
    }
}
