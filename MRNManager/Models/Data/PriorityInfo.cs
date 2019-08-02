using Newtonsoft.Json;

namespace MarketDataWebSocket.Models.Data
{
    internal class PriorityInfo
    {
        [Newtonsoft.Json.JsonProperty("Class", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]

        public int Class { get; set; }
        [Newtonsoft.Json.JsonProperty("Count", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]

        public int Count { get; set; }
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        public static PriorityInfo FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PriorityInfo>(data);
        }
    };
}