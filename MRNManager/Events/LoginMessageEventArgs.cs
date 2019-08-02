using System;
using System.Collections.Generic;
using System.Text;
using MarketDataWebSocket.Models.Data;

namespace MarketDataWebSocket.Events
{
    public class LoginMessageEventArgs
    {
        public DateTime TimeStamp { get; set; }
        public IMessage Message { get; set; }
    }
}
