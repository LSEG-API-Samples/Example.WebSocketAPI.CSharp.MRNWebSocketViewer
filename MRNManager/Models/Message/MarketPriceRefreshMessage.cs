using System.Collections.Generic;
using Newtonsoft.Json;

namespace MarketDataWebSocket.Models.Message
{
    internal class MarketPriceRefreshMessage: ReqMessage
    {
        [Newtonsoft.Json.JsonProperty("Fields", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Dictionary<string,dynamic> Fields { get; set; }
        public new static MarketPriceRefreshMessage FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MarketPriceRefreshMessage>(data);
        }
    }
}