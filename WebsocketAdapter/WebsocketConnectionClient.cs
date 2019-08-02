using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebsocketAdapter.Events;

namespace WebsocketAdapter
{

        public class WebsocketConnectionClient
        {
             /// <summary> ClientWebsocket instance name
            public string Name { get; set; }

             private bool _stop = false;
            /// <summary> Current WebSocket associated with this client connection. </summary>
            public ClientWebSocket WebSocket { get; set; }

            /// <summary> Indicates whether connection thread is stopped by users</summary>
            public bool Stop
            {
                get => _stop;
                set
                {
                    _stop = value;
                    if (!_stop || this.WebSocket == null) return;
                    WebSocket.Abort();
                    WebSocket.Dispose();
                }
            }

            /// <summary> This is used to cancel operations when closing the application. </summary>
            public CancellationTokenSource Cts { get; set; }

            /// <summary> URI to connect the WebSocket to. </summary>
            public Uri Host { get; set; }          

            public int SendBufferSize { get; set; } = 8192;
            public int RecvBufferSize { get; set; } = 8192;
        /// <summary>
        /// Connection Retry Interval period(in millisecond) after the connection is Aborted or Closed. Adapter will try to reconnecting to the server.
        /// Default value is 3000 which is 3 second.
        /// </summary>
        public int ConnectionRetryInterval { get; set; } = 3000;
            private List<string> _subProtocol=new List<string>();
            public WebsocketConnectionClient(string name, Uri hostUri,string subProtocol)
            {
                Name = name;
                Host = hostUri;
                Cts = new CancellationTokenSource();
                if(!string.IsNullOrEmpty(subProtocol))
                    _subProtocol=subProtocol.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            /// <summary>
            /// Creates a WebSocket connection and connecting to the server. Raise connection event.
            /// </summary>
            private async Task Connect()
            {

                WebSocket = new ClientWebSocket();        
                WebSocket.Options.SetBuffer(RecvBufferSize, SendBufferSize);
                foreach(var protocolName in _subProtocol)
                     WebSocket.Options.AddSubProtocol(protocolName.Trim());
                
                try
                {
                    var task=WebSocket.ConnectAsync(Host, CancellationToken.None);

                    if (WebSocket.State != WebSocketState.Open)
                        RaiseConnectionEvent(DateTime.Now, WebSocket.State, $"Connecting to {Host}");

                    await task;
                }
                catch (Exception ex)
                {
                RaiseConnectionEvent(DateTime.Now, WebSocket.State, $"Connect():: {ex.Message}");
                RaiseErrorEvent(DateTime.Now,WebSocket.State, $"Connect():: {ex.Message}");
                }
                if (WebSocket.State == WebSocketState.Open)
                {
                    RaiseConnectionEvent(DateTime.Now, WebSocket.State, $"Connected to {Host}");

                }
            }

            /// <summary>
            /// Closes the existing connection and creates a new one.
            /// </summary>
            private async Task Reconnect()
            {
                Console.WriteLine("The WebSocket connection is closed for " + Name);
                while(!Stop && WebSocket.State!=WebSocketState.Open)
                {
                    
                    Console.WriteLine("Reconnect to the server for " + Name + " after 3 seconds...");
                    RaiseConnectionEvent(DateTime.Now, WebSocketState.Connecting, $"Attempting to connect to {Host}");
                    Thread.Sleep(ConnectionRetryInterval);
                    if (WebSocket == null) continue;
                    WebSocket.Dispose();
                    WebSocket = null;
                    await Connect();
                }
            }

            /// <summary>
            /// The main loop of the WebsocketConnectionClient
            /// </summary>
            public async Task Run()
            {
                var cancellationToken = new CancellationToken();
                if (Cts != null)
                    Cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                await Task.Factory.StartNew(async () =>
                {
                    await Connect();
                    
                    while (!Stop && !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            ReceiveMessage();

                            if (WebSocket.State == WebSocketState.Aborted)
                            {
                                await Reconnect().ConfigureAwait(false);
                            }
                        }
                        catch (System.AggregateException ex)
                        {
                            RaiseErrorEvent(DateTime.Now, WebSocket.State, ex.Message);
                            RaiseConnectionEvent(DateTime.Now, WebSocket.State, ex.Message);
                            await Reconnect().ConfigureAwait(false);

                        }
                    }
                    Console.WriteLine("Exit Task");
                }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
            }

           
            /// <summary>Reads data from the WebSocket and parses to JSON message</summary>
            private void ReceiveMessage()
            {
                var readBuffer = new ArraySegment<byte>(new byte[RecvBufferSize]);
                MemoryStream memoryStream = null;
                byte[] dataBuffer = null;

                while (!Stop && !Cts.IsCancellationRequested)
                {
                    var result = WebSocket.ReceiveAsync(readBuffer, Cts.Token);
                    if (!result.IsFaulted)
                    {
                        if (result.Result.EndOfMessage)
                        {
                            if (memoryStream != null)
                            {
                                memoryStream.Write(readBuffer.Array ?? throw new InvalidOperationException(),
                                    readBuffer.Offset, readBuffer.Count);
                                dataBuffer = memoryStream.GetBuffer();
                                memoryStream.Dispose();
                            }
                            else
                            {
                                dataBuffer = readBuffer.Array;
                            }

                            break;
                        }
                        else
                        {
                            if (memoryStream == null)
                                memoryStream = new MemoryStream((int) RecvBufferSize * 5);

                            memoryStream.Write(readBuffer.Array ?? throw new InvalidOperationException(),
                                readBuffer.Offset, readBuffer.Count);
                            readBuffer = new ArraySegment<byte>(new byte[RecvBufferSize]);
                        }
                    }
                    else
                    {
                        RaiseErrorEvent(DateTime.Now, WebSocket.State,
                            "Unhandled Exception occured inside ReceiveMessage()");
                        break;
                    }
                }

                // Pass the data buffer back to app layer via callback.
                RaiseMessageEvent(DateTime.Now, dataBuffer);

            }


            protected void RaiseConnectionEvent(DateTime timestamp, WebSocketState state, string message)
            {
                var connectionCallback = new WebsocketAdapter.Events.ConnectionEventArgs
                    { TimeStamp = timestamp, State = state, StatusText = message };
                OnConnection(connectionCallback);
            }

            protected void RaiseMessageEvent(DateTime timestamp, byte[] message)
            {
                var messageCallback = new WebsocketAdapter.Events.MessageEventArgs { Buffer = message, TimeStamp = timestamp };
                OnMessage(messageCallback);
            }

            protected void RaiseErrorEvent(DateTime timestamp,  WebSocketState state, string errorMsg)
            {
                var errorCallback = new WebsocketAdapter.Events.ErrorEventArgs
                    { ErrorDetails = errorMsg, TimeStamp = timestamp, ClientWebSocketState = state };
                OnError(errorCallback);
            }

            protected virtual void OnConnection(ConnectionEventArgs e)
            {
                var handler = ConnectionEvent;
                handler?.Invoke(this, e);
            }

            protected virtual void OnMessage(WebsocketAdapter.Events.MessageEventArgs e)
            {
                var handler = MessageEvent;
                handler?.Invoke(this, e);
            }

            protected virtual void OnError(WebsocketAdapter.Events.ErrorEventArgs e)
            {
                var handler = ErrorEvent;
                handler?.Invoke(this, e);
            }

            public event EventHandler<WebsocketAdapter.Events.ConnectionEventArgs> ConnectionEvent;
            public event EventHandler<WebsocketAdapter.Events.MessageEventArgs> MessageEvent;
            public event EventHandler<WebsocketAdapter.Events.ErrorEventArgs> ErrorEvent;
        }
    
}