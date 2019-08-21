using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MarketDataWebSocket.Models.Data;
using MarketDataWebSocket.Models.Enum;
using MarketDataWebSocket.Models.Message;
using WebsocketAdapter;

namespace MarketDataWebSocket
{
    internal class WebsocketMarketDataManager
    {
        private readonly WebsocketConnectionClient _websocket;

        public WebsocketMarketDataManager(WebsocketConnectionClient websocket)
        {
            _websocket = websocket ?? throw new ArgumentNullException(nameof(websocket));
        }

        public async Task SendLogin(string username, string position, string appID = "256", int streamID = 1)
        {
            
            var loginReq = new ReqMessage
            {
                 ID = streamID, Domain = DomainEnum.Login,
                 MsgType = MessageTypeEnum.Request,
                 Key = new MessageKey()
            };
            loginReq.Key.Elements = new Dictionary<string, object>{{"ApplicationId", appID}, {"Position", position}};
            loginReq.Key.Name = new List<string> {username};

            await ClientWebSocketUtils.SendTextMessage(_websocket.WebSocket, loginReq.ToJson());
           
        }

        public async Task SendMrnStoryRequest(int streamId, string storyItem = "MRN_STORY")
        {
             var marketPriceReq = new ReqMessage
             {
                 ID = streamId,
                 Domain = DomainEnum.NewsTextAnalytics,
                 Key = new MessageKey {Name = new List<string> {storyItem}, NameType = NameTypeEnum.Ric}
             };
             await ClientWebSocketUtils.SendTextMessage(_websocket.WebSocket, marketPriceReq.ToJson());
          
        }


        public async Task SendMarketPriceRequest(string itemList, int streamId, List<string> fieldList = null)
        {
            var marketPriceReq = new ReqMessage
            {
                ID = streamId,
                Domain = DomainEnum.MarketPrice,
                Key = new MessageKey
                {
                    Name = itemList.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                    NameType = NameTypeEnum.Ric
                }
            };
            if (fieldList != null)
                marketPriceReq.View = fieldList;
            await ClientWebSocketUtils.SendTextMessage(_websocket.WebSocket, marketPriceReq.ToJson());
        }

        public async Task SendCloseMessage(int streamId, DomainEnum domain = DomainEnum.MarketPrice)
        {
            var closeMsg = new CloseMessage {ID = streamId, Domain = domain, MsgType = MessageTypeEnum.Close};
            Console.WriteLine(closeMsg.ToJson());
            await ClientWebSocketUtils.SendTextMessage(_websocket.WebSocket, closeMsg.ToJson());
        }

        // True = Ping
        // False = Pong
        public async Task SendPingPong(bool sendPing)
        {
            var pingMsg = new PingPongMessage {Type = sendPing ? MessageTypeEnum.Ping : MessageTypeEnum.Pong};
            await ClientWebSocketUtils.SendTextMessage(_websocket.WebSocket, pingMsg.ToJson());
            Console.WriteLine(pingMsg.ToJson());
        }
    }
}