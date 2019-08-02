using System;
using System.Collections.Generic;
using System.Text;
using MarketDataWebSocket.Models.Enum;
using MarketDataWebSocket.Models.Message;


namespace MarketDataWebSocket.Events
{
    public class MrnStatusMsgEventArgs
    {
        public DateTime TimeStamp { get; set; }
        public DomainEnum Domain { get; set; }
        public StatusMessage Status { get; set; }

    }
}
