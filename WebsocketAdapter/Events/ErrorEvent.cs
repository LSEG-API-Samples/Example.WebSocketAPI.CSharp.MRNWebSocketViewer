using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace WebsocketAdapter.Events
{
    public class ErrorEventArgs:EventArgs
    {
        public DateTime TimeStamp { get; set; }
        public string ErrorDetails { get; set; }
        public WebSocketState ClientWebSocketState { get; set; }
    }
}
