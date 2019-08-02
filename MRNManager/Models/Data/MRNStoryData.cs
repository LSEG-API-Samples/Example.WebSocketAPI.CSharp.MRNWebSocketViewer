using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;
using MarketDataWebSocket.Models.Enum;
using Newtonsoft.Json;

namespace MarketDataWebSocket.Models.Data
{
    public class MrnStoryData
    {
        public MrnStoryData()
        {
        }
        public MessageTypeEnum MsgType { get; set; }

        public long PROD_PERM { get; set; }
        public long RECORDTYPE { get; set; }
        public string RDN_EXCHD2 { get; set; }
        public double CONTEXT_ID { get; set; }
        public long DDS_DSO_ID { get; set; }
        public string SPS_SP_RIC { get; set; }
        public string ACTIV_DATE { get; set; }
        public long TIMACT_MS { get; set; }
        public string GUID { get; set; }
        
        public string MRN_V_MAJ { get; set; }
        public MrnTypeEnum MRN_TYPE { get; set; }
        public string MRN_V_MIN { get; set; }
        public string MRN_SRC { get; set; }
        public long FRAG_NUM { get; set; }
        public long TOT_SIZE { get; set; }
        public byte[] FRAGMENT { get; set; }
        public long FragmentSize { get; set; }
        public string JsonData { get; set; }

        public StoryData Story => string.IsNullOrEmpty(this.JsonData) ?null:
                                  JsonConvert.DeserializeObject<StoryData>(this.JsonData);
       
        public bool IsCompleted => this.TOT_SIZE > 0 && (this.FragmentSize) == this.TOT_SIZE;
    }
}
