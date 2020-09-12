using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostcodesBelpost
{
    class StreetList
    {
        [JsonProperty("streets")]
        public List<Street> Streets { get; set; }
    }
}
