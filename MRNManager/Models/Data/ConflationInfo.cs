﻿using Newtonsoft.Json;

namespace MarketDataWebSocket.Models.Data
{
    public class ConflationInfo
    {
        [Newtonsoft.Json.JsonProperty("Count", DefaultValueHandling = DefaultValueHandling.Ignore,
            Required = Newtonsoft.Json.Required.AllowNull,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? Count { get; set; }
        [Newtonsoft.Json.JsonProperty("Time", DefaultValueHandling = DefaultValueHandling.Ignore,
            Required = Newtonsoft.Json.Required.AllowNull,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? Time { get; set; }
    }
}