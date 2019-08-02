using Newtonsoft.Json;

namespace MarketDataWebSocket.Models.Data
{
    namespace Model
    {
        internal class DebugInfo
        {
            [Newtonsoft.Json.JsonProperty("File", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
            public string File { get; set; }
            [Newtonsoft.Json.JsonProperty("Line", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
            public int? Line { get; set; }
            [Newtonsoft.Json.JsonProperty("Message", DefaultValueHandling = DefaultValueHandling.Ignore,  NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
            public string Message { get; set; }
            [Newtonsoft.Json.JsonProperty("Offset", DefaultValueHandling = DefaultValueHandling.Ignore,  NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
            public int? Offset { get; set; }
            public string ToJson()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            }

            public static DebugInfo FromJson(string data)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<DebugInfo>(data);
            }
        };
    }

  
   
}
