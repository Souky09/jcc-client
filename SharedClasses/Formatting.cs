using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    public class Formatting
    {   
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("fields")]
        public List<Field>    Fields { get; set; }
    }
}
