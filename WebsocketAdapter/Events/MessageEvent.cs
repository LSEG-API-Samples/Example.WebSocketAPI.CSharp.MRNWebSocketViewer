using System;
using System.Collections.Generic;
using System.Text;

namespace WebsocketAdapter.Events
{
    public class MessageEventArgs : EventArgs
    {
        public DateTime TimeStamp { get; set; }
        public byte[] Buffer { get; set; }
    }
}
