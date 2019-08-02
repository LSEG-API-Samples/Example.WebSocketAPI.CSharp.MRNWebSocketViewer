using MarketDataWebSocket.Models.Data;
using MarketDataWebSocket.Models.Data.Model;
using MarketDataWebSocket.Models.Enum;
using Newtonsoft.Json;

namespace MarketDataWebSocket.Models.Message
{
    internal class ErrorMessage:IMessage
    {
        [Newtonsoft.Json.JsonProperty("Domain", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public override DomainEnum? Domain { get; set; }

        [Newtonsoft.Json.JsonProperty("ID", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public override int? ID { get; set; }

        public override MessageTypeEnum? MsgType { get; set; }

        [Newtonsoft.Json.JsonProperty("Text", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Text { get; set; }

        [Newtonsoft.Json.JsonProperty("Type", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public MessageTypeEnum? Type { get; set; }

        [Newtonsoft.Json.JsonProperty("Debug", DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public DebugInfo DebugInfo { get; set; }
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
        public static ErrorMessage FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorMessage>(data);
        }
    }
}