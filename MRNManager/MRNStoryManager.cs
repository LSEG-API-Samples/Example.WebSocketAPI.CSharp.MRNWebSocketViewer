using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MarketDataWebSocket.Events;
using MarketDataWebSocket.Extensions;
using MarketDataWebSocket.Models.Data;
using MarketDataWebSocket.Models.Enum;
using MarketDataWebSocket.Models.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebsocketAdapter;
using WebsocketAdapter.Events;

namespace MarketDataWebSocket
{
    public class MrnStoryManager
    {
        private readonly WebsocketMarketDataManager _websocketMarketDataMgr;
        private readonly Dictionary<int, MrnStoryData> _mrnDataList = new Dictionary<int, MrnStoryData>();
        public int UpdateCount { get; private set; }
        public Dictionary<int, MrnStoryData> MrnData => new Dictionary<int, MrnStoryData>(_mrnDataList);
        public bool IsLoggedIn { get; set; }
        public string MRN_STORY_RIC { get; set; } = "MRN_STORY";
        public int MRN_STREAM_ID { get; set; } = 3;

        public MrnStoryManager(WebsocketConnectionClient websocketAdapter)
        {
            var websocketAdapter1 = websocketAdapter ?? throw new ArgumentNullException(nameof(websocketAdapter));
            _websocketMarketDataMgr = new WebsocketMarketDataManager(websocketAdapter);
            websocketAdapter1.MessageEvent += this.ProcessWebsocketMessage;
        }

        public async Task SendLogin(string username, string position, string appID = "256", int streamID = 1)
        {
            await _websocketMarketDataMgr.SendLogin(username, position, appID, streamID);
        }

        internal void ProcessWebsocketMessage(object sender, MessageEventArgs e)
        {
            /* Console.WriteLine($"Message Received:{MarketDataUtils.TimeStampToString(e.TimeStamp)}");
             Console.WriteLine("================ Original JSON Data =================");
             JArray messageJson = JArray.Parse(Encoding.ASCII.GetString(e.Buffer));
             string prettyJson = JsonConvert.SerializeObject(messageJson, Formatting.Indented);
             Console.WriteLine("Data:\n{0}\n", prettyJson);
             Console.WriteLine("=====================================================");
             */

            try
            {
                var data = Encoding.UTF8.GetString(e.Buffer);
                var messages = JArray.Parse(data);
                foreach (var jsonData in messages.Children())
                    if (jsonData["Type"] != null)
                    {
                        var msgType = (MessageTypeEnum) Enum.Parse(typeof(MessageTypeEnum), (string) jsonData["Type"],
                            true);

                        if (jsonData["Domain"] == null) continue;
                        var rdmDomain = (DomainEnum) Enum.Parse(typeof(DomainEnum), (string) jsonData["Domain"],
                            true);

                        switch (msgType)
                        {
                            case MessageTypeEnum.Error:
                                // Process Error
                                Console.WriteLine("Some Error Found Here");
                                ProcessError(jsonData);
                                break;
                            case MessageTypeEnum.Ping:
                                // Send Pong back
                                Console.WriteLine("Ping Received");
                                _websocketMarketDataMgr.SendPingPong(false).GetAwaiter().GetResult();
                                Console.WriteLine("Pong Sent");
                                break;
                            default:
                                ProcessMessage(jsonData, msgType, rdmDomain);
                                break;
                        }

                        Console.WriteLine();
                    }
            }
            catch (Exception ex)
            {
                var msg=$"Error ProcessWebsocketMessage() {ex.Message}\n{ex.StackTrace}";
                RaiseErrorEvent(DateTime.Now,msg);
            }
        }

        private void ProcessMessage(JToken jsonData, MessageTypeEnum msgType, DomainEnum domain)
        {
            switch (domain)
            {
                case DomainEnum.Login:
                    ProcessLogin(jsonData, msgType);
                    break;
                case DomainEnum.NewsTextAnalytics:
                    ProcessMrnStory(jsonData, msgType);
                    break;
                default:
                    Console.WriteLine("Unsupported Domain");
                    RaiseErrorEvent(DateTime.Now,$"Received response message for unhandled domain model");
                    break;
            }
        }

        private void ProcessError(JToken jsonData)
        {
            var message = jsonData.ToObject<ErrorMessage>();
            var errorMsg = new StringBuilder();

            errorMsg.Append($"Websocket Connection Error ID:{message.ID}\n");
            errorMsg.Append($"Message:{message.Text}\n");
            errorMsg.Append($"DebugInfo:\n{message.DebugInfo.ToJson()}\n");
            RaiseErrorEvent(DateTime.Now,errorMsg.ToString());
        }

      

        private void ProcessLogin(JToken jsonData, MessageTypeEnum msgType)
        {   
            switch (msgType)
            {
                case MessageTypeEnum.Refresh:
                {
                    var message = jsonData.ToObject<RefreshMessage>();
                    if (message.State.Stream == StreamStateEnum.Open && message.State.Data != DataStateEnum.Suspect)
                    {
                            IsLoggedIn = true;
                            _websocketMarketDataMgr.SendMrnStoryRequest(MRN_STREAM_ID, MRN_STORY_RIC).GetAwaiter().GetResult();
                    }
                    RaiseLoginMessageEvent(DateTime.Now,message);
                   
                }
                break;
                case MessageTypeEnum.Status:
                    {
                        var message = jsonData.ToObject<StatusMessage>();
                        if (message.State.Stream == StreamStateEnum.Closed || message.State.Stream == StreamStateEnum.ClosedRecover)
                            IsLoggedIn = false;
                        RaiseLoginMessageEvent(DateTime.Now, message);
                    }
                    break;
                case MessageTypeEnum.Update:
                    RaiseLoginMessageEvent(DateTime.Now, jsonData.ToObject<UpdateMessage>());
                    break;
               
            }
            
        }

        private void ProcessMrnStory(JToken jsonData, MessageTypeEnum msgType)
        {
            switch (msgType)
            {
                case MessageTypeEnum.Refresh:
                    {
                        var message = jsonData.ToObject<MarketPriceRefreshMessage>();
                        message.MsgType = MessageTypeEnum.Refresh;
                        //Console.WriteLine($"Ric Name:{MarketDataUtils.StringListToString(message.Key.Name)}");
                        if (message.Fields != null)
                        {
                            var mrnRefreshData = message.Fields.ToObject<MrnStoryData>();
                            mrnRefreshData.MsgType = MessageTypeEnum.Refresh;
                            RaiseMrnMessageEvent(DateTime.Now, mrnRefreshData);
                        }
                    }
                    break;
                case MessageTypeEnum.Update:
                    {
                        var message = jsonData.ToObject<MarketPriceUpdateMessage>();
                        message.MsgType = MessageTypeEnum.Update;

                        if(message.Fields != null)
                            ProcessFieldData(message.Fields);
                            
                    }
                    break;
                case MessageTypeEnum.Status:
                    { 
                        var message = jsonData.ToObject<StatusMessage>();
                        //Check if item stream is closed or closed recover and resend item request again if Login still open.
                        if (message.State.Stream == StreamStateEnum.Closed ||
                            message.State.Stream == StreamStateEnum.ClosedRecover)
                        {
                            if (IsLoggedIn)
                            {
                                _websocketMarketDataMgr.SendMrnStoryRequest(MRN_STREAM_ID, MRN_STORY_RIC).GetAwaiter().GetResult();
                            }
                        }
                        RaiseMrnStatusEvent(DateTime.Now,DomainEnum.NewsTextAnalytics, message);
                    }
                    break;
            }
        }


        private bool ProcessFieldData(IDictionary<string, object> Fields)
        {
           
            var mrnData = Fields.ToObject<MrnStoryData>();
            mrnData.MsgType = MessageTypeEnum.Update;
            var newUpdateByteArray = mrnData.FRAGMENT ?? throw new ArgumentNullException("mrnData.FRAGMENT");
            var newUpdateFragmentSize = (int?) newUpdateByteArray?.Length ?? 0;

            if (mrnData.FRAG_NUM == 1 && mrnData.TOT_SIZE > 0)
            {
                    //Shrink FRAGMENT size to TOT_SIZE
                    mrnData.FRAGMENT=new byte[mrnData.TOT_SIZE];
                    Buffer.BlockCopy(newUpdateByteArray ?? throw new InvalidOperationException(), 0,
                        mrnData.FRAGMENT, 0, (int) newUpdateFragmentSize);
                    mrnData.FragmentSize = newUpdateFragmentSize;
                    _mrnDataList.Add(UpdateCount, mrnData);
            }
            else if (mrnData.FRAG_NUM > 1)
            {
                if (_mrnDataList[UpdateCount].GUID == mrnData.GUID)
                {
                    var tmpByteArray = _mrnDataList[UpdateCount].FRAGMENT;
                    var tmpTotalSize= _mrnDataList[UpdateCount].TOT_SIZE;
                    var tmpFragmentSize = _mrnDataList[UpdateCount].FragmentSize;

                    _mrnDataList[UpdateCount] = mrnData;
                    _mrnDataList[UpdateCount].FRAGMENT = tmpByteArray;
                    _mrnDataList[UpdateCount].TOT_SIZE = tmpTotalSize;
                    _mrnDataList[UpdateCount].FragmentSize = tmpFragmentSize;

                    Buffer.BlockCopy(newUpdateByteArray, 0,
                        _mrnDataList[UpdateCount].FRAGMENT,
                        (int) _mrnDataList[UpdateCount].FragmentSize, (int) newUpdateFragmentSize);
                    
                    // Calculate current Fragment Size
                    _mrnDataList[UpdateCount].FragmentSize += newUpdateFragmentSize;
                }
                else
                {
                    var msg =
                        $"Cannot find previous update with the same GUID {mrnData.GUID}. This update will be skipped.";
                    RaiseErrorEvent(DateTime.Now,msg);
                    UpdateCount++;
                }
            }

            // Check if the update contains complete MRN Story 
            if (_mrnDataList[UpdateCount].IsCompleted)
            {
                _mrnDataList[UpdateCount].JsonData = MarketDataUtils
                 .UnpackByteToJsonString(_mrnDataList[UpdateCount].FRAGMENT).GetAwaiter().GetResult();
                RaiseMrnMessageEvent(DateTime.Now, _mrnDataList[UpdateCount]);
                UpdateCount++;
                return true;
            }
            else
            {
               if (_mrnDataList[UpdateCount].FragmentSize > _mrnDataList[UpdateCount].TOT_SIZE)
               {
                   var msg = $"Received message with GUID={_mrnDataList[UpdateCount].GUID} has a size greater than total message size. This update will be skipped.";
                   Console.WriteLine(msg);
                   RaiseErrorEvent(DateTime.Now, msg);
                   UpdateCount++;
                }
            }

            return false;
        }
        protected void RaiseMrnMessageEvent(DateTime timestamp,MrnStoryData data)
        {
            var messageCallback = new MrnMessageEventArgs() {Data = data, TimeStamp = timestamp};
            OnMessage(messageCallback);
        }
        protected void RaiseMrnStatusEvent(DateTime timestamp,DomainEnum domain,StatusMessage status)
        {
            var statusCallback = new MrnStatusMsgEventArgs() {Domain = domain, Status=status, TimeStamp = timestamp };
            OnStatus(statusCallback);
        }
        protected void RaiseLoginMessageEvent(DateTime timestamp, IMessage message)
        {
            var messageCallback = new LoginMessageEventArgs() { Message= message, TimeStamp = timestamp };
            OnLoginMessage(messageCallback);
        }

        protected void RaiseErrorEvent(DateTime timestamp, string errorMsg)
        {
            var errorCallback = new MrnErrorEventArgs() {TimeStamp = timestamp, ErrorMessage = errorMsg};
            OnError(errorCallback);
        }
        protected virtual void OnMessage(MrnMessageEventArgs e)
        {
            var handler = MessageEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnError(MrnErrorEventArgs e)
        {
            var handler = ErrorEvent;
            handler?.Invoke(this, e);
        }
        protected virtual void OnStatus(MrnStatusMsgEventArgs e)
        {
            var handler = StatusEvent;
            handler?.Invoke(this, e);
        }
        protected virtual void OnLoginMessage(LoginMessageEventArgs e)
        {
            var handler = LoginMessageEvent;
            handler?.Invoke(this, e);
        }
        public event EventHandler<MrnStatusMsgEventArgs> StatusEvent;
        public event EventHandler<MrnMessageEventArgs> MessageEvent;
        public event EventHandler<LoginMessageEventArgs> LoginMessageEvent;
        public event EventHandler<MrnErrorEventArgs> ErrorEvent;
    }
}