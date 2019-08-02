using System;
using System.Collections.Generic;
using System.Text;
using MarketDataWebSocket.Models.Data;

namespace MarketDataWebSocket.Events
{
    public class MrnMessageEventArgs
    {
        public DateTime TimeStamp { get; set; }
        public MrnStoryData Data { get; set; }
    }
}
