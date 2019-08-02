using System.Collections.Generic;
using MarketDataWebSocket.Models.Data;
using MarketDataWebSocket.Models.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarketDataWebSocket.Models.Message
{
    internal class ReqMessage : IMessage
    {
        [Newtonsoft.Json.JsonProperty("Domain", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public override DomainEnum? Domain { get; set; }
        [Newtonsoft.Json.JsonProperty("ID", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public override int? ID { get; set; }
        [Newtonsoft.Json.JsonProperty("MsgType", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public override MessageTypeEnum? MsgType { get; set; }
        [Newtonsoft.Json.JsonProperty("ConfInfoInUpdates", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? ConfInfoInUpdates { get; set; }
        [Newtonsoft.Json.JsonProperty("Key", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public MessageKey Key { get; set; }
        [Newtonsoft.Json.JsonProperty("KeyInUpdates", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? KeyInUpdates { get; set; }
        [Newtonsoft.Json.JsonProperty("Pause", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Pause { get; set; }
        [Newtonsoft.Json.JsonProperty("Priority", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public PriorityInfo Priority { get; set; }
        [Newtonsoft.Json.JsonProperty("Private", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Private { get; set; }
        [Newtonsoft.Json.JsonProperty("QoS", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public QosInfo QoS { get; set; }
        [Newtonsoft.Json.JsonProperty("Qualified", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Qualified { get; set; }
        [Newtonsoft.Json.JsonProperty("Refresh", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? Refresh { get; set; }
        [Newtonsoft.Json.JsonProperty("Streaming", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]   
        public bool? Streaming { get; set; }
        [Newtonsoft.Json.JsonProperty("View", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<string> View { get; set; }
        [Newtonsoft.Json.JsonProperty("WorstQos", DefaultValueHandling = DefaultValueHandling.Ignore, Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public QosInfo WorstQos { get; set; }
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        public static ReqMessage FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ReqMessage>(data);
        }
    };
}