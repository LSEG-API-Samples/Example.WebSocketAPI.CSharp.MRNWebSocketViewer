using System;
using System.Collections.Generic;
using System.Text;

namespace MarketDataWebSocket.Models.Data
{
    public class LoginElementData
    {
        public int ClientToServerPingInterval{get;set;}
        public int MaxMsgSize { get; set; }
        public int PingCount { get; set; }
        public int ServerToClientPingInterval { get; set; }
        public override string ToString()
        {
            var msg = new StringBuilder();
            msg.Append($"ClientToServerPingInterval:{this.ClientToServerPingInterval}\n");
            msg.Append($"MaxMsgSize:{this.MaxMsgSize}\n");
            msg.Append($"PingCount:{this.PingCount}\n");
            msg.Append($"ServerToClientPingInterval:{this.ServerToClientPingInterval}\n");
           
            return msg.ToString();

        }
    }
}
