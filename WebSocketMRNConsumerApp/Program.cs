using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using MarketDataWebSocket;
using MarketDataWebSocket.Models.Enum;
using MarketDataWebSocket.Models.Message;
using WebsocketAdapter;

namespace WebSocketMRNConsumerConsole
{
    class Program
    {
        private static readonly Uri WebsocketUri = new Uri("ws://localhost:15000/WebSocket");
        private static readonly string Clientname = "apitest1";
        private static readonly string DACS_User = "<DACS USER>";
        private static readonly string AppId = "256";
        private static readonly string Login_Position = "127.0.0.1/net";
        private static bool b_cancel = false;
        static void Main(string[] args)
        {
                Console.WriteLine("Starting MRN Websocket Consumer app. Press Ctrl+C to exit.\n");
                var websocket = new WebsocketConnectionClient(Clientname, WebsocketUri,"tr_json2");
                websocket.Cts = new CancellationTokenSource();
                var mrnmanager = new MrnStoryManager(websocket);
                mrnmanager.MRN_STREAM_ID = 3;
                mrnmanager.MessageEvent += (sender, e) =>
                {
                    Console.WriteLine("******************* Process MRN Message Events **********************");
                    Console.WriteLine($"TimeStamp:{e.TimeStamp}");
                    Console.WriteLine($"Received {e.Data.MsgType} {e.TimeStamp}\n");
                    Console.WriteLine($"Active Date:{e.Data.ACTIV_DATE}");
                    Console.WriteLine($"MRN_TYPE:{e.Data.MRN_TYPE}");
                    Console.WriteLine($"Context ID:{e.Data.CONTEXT_ID}");
                    Console.WriteLine($"Prod Perm:{e.Data.PROD_PERM}");
                    Console.WriteLine($"Fragment Count={e.Data.FRAG_NUM} Total Size={e.Data.TOT_SIZE} bytes");
                    if(e.Data.Story!=null)
                        Console.WriteLine(e.Data.Story.ToJson());
                    Console.WriteLine("*********************************************************************");
                    Console.WriteLine();
                };
                mrnmanager.ErrorEvent += (sender, e) =>
                {
                    Console.WriteLine("******************* Process MRN Error Events **********************");
                    Console.WriteLine($"TimeStamp:{e.TimeStamp}");
                    Console.WriteLine($"{e.ErrorMessage}");
                    Console.WriteLine("*********************************************************************");
                    Console.WriteLine();
                };
                mrnmanager.StatusEvent += (sender, e) =>
                {
                    Console.WriteLine("******************* Process MRN Status Events **********************");
                    Console.WriteLine($"Received {e.Status.MsgType} {e.TimeStamp}");
                    Console.WriteLine($"Stream State:{e.Status.State.Stream}");
                    Console.WriteLine($"Data State:{e.Status.State.Data}");
                    Console.WriteLine($"State Code:{e.Status.State.Code}");
                    Console.WriteLine($"Status Text:{e.Status.State.Text}");
                    Console.WriteLine("*********************************************************************");
                    Console.WriteLine();
                };
                mrnmanager.LoginMessageEvent += (sender, e) =>
                {
                    Console.WriteLine("******************* Process Login Message Events **********************");
                    Console.WriteLine($"{e.TimeStamp}  received {e.Message.MsgType}");
                    switch (e.Message.MsgType)
                    {
                        case MessageTypeEnum.Refresh:
                        {
                            var message = (RefreshMessage) e.Message;
                            Console.WriteLine($"Login name:{message.Key.Name.FirstOrDefault()}");
                            Console.WriteLine(
                                $"Login Refresh stream:{message.State.Stream} state:{message.State.Data} code:{message.State.Code} status text:{message.State.Text}");
                        }
                            break;

                        case MessageTypeEnum.Status:
                        {
                            var message = (StatusMessage) e.Message;
                            Console.WriteLine($"Login name:{message.Key.Name.FirstOrDefault()}");
                            Console.WriteLine(
                                $"Login Status stream:{message.State.Stream} state:{message.State.Data} code:{message.State.Code} status text:{message.State.Text}");
                            if (message.State.Stream == StreamStateEnum.Closed ||
                                message.State.Stream == StreamStateEnum.ClosedRecover)
                                b_cancel = true;
                        }
                            break;
                    }
                    Console.WriteLine("*********************************************************************");
                    Console.WriteLine();
                };
                websocket.ErrorEvent += (sender, e) =>
                {
                    Console.WriteLine("******************* Process Websocket Error Events **********************");
                    Console.WriteLine($"OnConnectionError {e.TimeStamp} {e.ClientWebSocketState} {e.ErrorDetails}");
                    Console.WriteLine("*********************************************************************");
                    Console.WriteLine();
                };

                websocket.ConnectionEvent += (sender, e) =>
                {
                    Console.WriteLine("******************* Process Websocket Connection Events **********************");
                    Console.WriteLine($"OnConnection Event Received:{MarketDataUtils.TimeStampToString(e.TimeStamp)}");

                    Console.WriteLine($"Connection State:{e.State}");
                    Console.WriteLine($"Message:{e.StatusText}");
                    if (e.State == WebSocketState.Open)
                    {
                          mrnmanager.SendLogin(DACS_User, Login_Position,AppId,1).GetAwaiter().GetResult();
                    }
                    Console.WriteLine("*********************************************************************");
                    Console.WriteLine();
                };
                Console.CancelKeyPress += (sender,e) =>
                {

                    websocket.Stop = true;
                    b_cancel = true;
                };
                websocket.Run().GetAwaiter().GetResult();
                while (!b_cancel);

                Console.WriteLine("Quit the app");
           
        }

       
    }
}
