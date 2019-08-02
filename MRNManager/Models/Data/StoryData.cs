using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MarketDataWebSocket.Models.Data
{
    public class StoryData
    {
        [Newtonsoft.Json.JsonProperty("altId", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string AltId { get; set; }
        [Newtonsoft.Json.JsonProperty("audiences", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<string> Audiences { get; set; }
        [Newtonsoft.Json.JsonProperty("firstCreated", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public DateTime FirstCreated { get; set; }
        [Newtonsoft.Json.JsonProperty("headline", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Headline { get; set; }
        [Newtonsoft.Json.JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Id { get; set; }
        [Newtonsoft.Json.JsonProperty("instanceOf", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<string> InstanceOf { get; set; }
        [Newtonsoft.Json.JsonProperty("language", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Language { get; set; }
        [Newtonsoft.Json.JsonProperty("provider", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Provider { get; set; }
        [Newtonsoft.Json.JsonProperty("pubStatus", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string PubStatus { get; set; }
        [Newtonsoft.Json.JsonProperty("subjects", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<string> Subjects { get; set; }
        [Newtonsoft.Json.JsonProperty("takeSequence", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int  TakeSequence { get; set; }
        [Newtonsoft.Json.JsonProperty("urgency", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int Urgency { get; set; }
        [Newtonsoft.Json.JsonProperty("versionCreated", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public DateTime VersionCreated { get; set; }
        [Newtonsoft.Json.JsonProperty("body", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Body { get; set; }
        [Newtonsoft.Json.JsonProperty("mineType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string MineType { get; set; }
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        public static StoryData FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<StoryData>(data);
        }

    }
}
