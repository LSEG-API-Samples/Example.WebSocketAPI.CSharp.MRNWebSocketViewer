using Newtonsoft.Json;

namespace MarketDataWebSocket.Models.Data
{
    public class PostUserInfo
    {
        [Newtonsoft.Json.JsonProperty("Address", DefaultValueHandling = DefaultValueHandling.Ignore,  NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Address { get; set; }
        [Newtonsoft.Json.JsonProperty("UserId", DefaultValueHandling = DefaultValueHandling.Ignore,  NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]

        public int UserId { get; set; }
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        public static PostUserInfo FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PostUserInfo>(data);
        }
    }
}