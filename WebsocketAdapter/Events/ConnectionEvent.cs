using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace WebsocketAdapter.Events
{
    public class ConnectionEventArgs : EventArgs
    {
        public DateTime TimeStamp { get; set; }
        public WebSocketState State { get; set; }
        public string StatusText { get; set; }
    }
}
