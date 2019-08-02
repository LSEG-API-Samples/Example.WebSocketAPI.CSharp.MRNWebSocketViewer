using System;
using System.Collections.Generic;
using System.Text;


namespace MarketDataWebSocket.Events
{
    public class MrnErrorEventArgs
    {
        public DateTime TimeStamp { get; set; }
        public string ErrorMessage { get; set; }

    }
}
