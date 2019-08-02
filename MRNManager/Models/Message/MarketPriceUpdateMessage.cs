using System.Collections.Generic;
using Newtonsoft.Json;

namespace MarketDataWebSocket.Models.Message
{
    internal class MarketPriceUpdateMessage : UpdateMessage
    {
        [Newtonsoft.Json.JsonProperty("Fields", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Dictionary<string, object> Fields { get; set; }

        public new static MarketPriceUpdateMessage FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MarketPriceUpdateMessage>(data);
        }
    };
}