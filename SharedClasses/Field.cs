using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    public class Field
    {
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("format")]
        public string Format { get; set; }
        [JsonProperty("length")]
        public string Length { get; set; }
    }
}
