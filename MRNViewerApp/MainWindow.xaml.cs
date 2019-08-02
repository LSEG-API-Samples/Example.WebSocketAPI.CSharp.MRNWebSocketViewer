using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MarketDataWebSocket;
using MarketDataWebSocket.Events;
using MarketDataWebSocket.Models.Enum;
using MarketDataWebSocket.Models.Message;
using WebsocketAdapter;
using WebsocketAdapter.Events;

namespace MRNWebsocketViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MrnStoryDataGrid1.ItemsSource = _fragmentCollection;
            var ipV4 = (from ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList
                        where ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip)
                        select ip).FirstOrDefault();
            DACSPosition.Text = (ipV4 == null) ? $"127.0.0.1/net" : $"{ipV4}/net";
        }

        private async void ConnectBtn_OnClick(object sender, RoutedEventArgs e)
        {
            //Get values of the login and server from WPF UI
            _dacsUser = DACSUserTxt.Text.Trim();
            _dacsPosition = DACSPosition.Text.Trim();
            _appId = AppIdTxt.Text.Trim();
            var endpointServer = WebsocketEndpointTxt.Text.Trim();
         
            ///

            if (!_isConnected && !_isWait)
            {
                IsConnected(false, true, "Cancel");
                await Task.Run(async () =>
                {

                    _websocketClient =new WebsocketConnectionClient("client1", new Uri(endpointServer), "tr_json2"){Cts = new CancellationTokenSource()};

                    _mrnManager = new MrnStoryManager(_websocketClient);
                    _mrnManager.ErrorEvent += ProcessMrnErrorEvent;
                    _mrnManager.StatusEvent += ProcessMrnStatusEvent;
                    _mrnManager.MessageEvent += ProcessMrnMessageEvent;
                    _mrnManager.LoginMessageEvent += ProcessLoginMessageEvent;
                    _websocketClient.ConnectionEvent += ProcessConnectionEvent;
                    _websocketClient.ErrorEvent += ProcessWebSocketErrorEvent;
                   
                    await _websocketClient.Run().ConfigureAwait(false);

                }).ConfigureAwait(false);

            }
            else
            if (_isConnected && !_isWait)
            {
                _websocketClient.Stop = true;
                _websocketClient.Cts.Cancel();
                IsConnected(false, false, "Connect");
            }
            else if (!_isConnected && _isWait)
            {
                _websocketClient.Stop = true;
                _websocketClient.Cts.Cancel();
                IsConnected(false, false, "Connect");
            }


        }

        private void IsConnected(bool isConnected, bool isWait, string status)
        {
            ConnectBtn.Content = status;

            WebsocketEndpointTxt.IsReadOnly = isConnected;
            LoginExpandBtn.IsEnabled = !isConnected;
            this._isWait = isWait;
            this._isConnected = isConnected;
        }

        private void ProcessMrnMessageEvent(object sender, MrnMessageEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (e.Data.MsgType)
                {
                    case MessageTypeEnum.Refresh:
                    {
                        var msg = $"Streaming";
                        var originalTitle = $"{_windowsName} Status::";
                        MrnViewerDesktop.Title = $"{originalTitle} {msg}";
                        break;
                    }
                    case MessageTypeEnum.Update:
                    {
                        var story = new Model.MrnStory
                        {
                            TimeStamp = e.TimeStamp,
                            Index = _count++,
                            Story = e.Data.Story,
                            GUID = e.Data.GUID,
                            MrnSource = e.Data.Story.Provider,
                            Frag_Num = (int)e.Data.FRAG_NUM,
                            Tot_Size = e.Data.TOT_SIZE
                        };

                        _fragmentCollection.Insert(0, story);
                        break;
                    }
                }
            }));
        }

        private void ProcessLoginMessageEvent(object sender, LoginMessageEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var msg = new StringBuilder();
                switch (e.Message.MsgType)
                {
                    case MessageTypeEnum.Refresh:
                    {
                       
                        var respMsg = (RefreshMessage) e.Message;
                        if (respMsg.State.Stream == StreamStateEnum.Open && respMsg.State.Data != DataStateEnum.Suspect)
                        {
                            msg.Append($"{e.TimeStamp} Data State:{respMsg.State.Data} Stream State:{respMsg.State.Stream} State Code:{respMsg.State.Code} Status Text:{respMsg.State.Text}");
                            LoginExpandBtn.IsExpanded = false;
                        }
                    }
                        break;
                    case MessageTypeEnum.Status:
                        var status = (StatusMessage) e.Message;
                        if (status.State.Stream == StreamStateEnum.Closed ||
                            status.State.Stream == StreamStateEnum.ClosedRecover)
                        {
                            msg.Append($"Disconnect {status.State.Text}");
                            IsConnected(false, false, "Connect");
                        }
                        break;
                    case MessageTypeEnum.Update:
                        throw new NotImplementedException();
                }
                var originalTitle = $"{_windowsName} Status::";
                MrnViewerDesktop.Title = $"{originalTitle} {msg.ToString()}";
            }));
           
        }
        private void ProcessMrnStatusEvent(object sender, MrnStatusMsgEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Status.State.Stream == StreamStateEnum.Closed ||
                    e.Status.State.Stream == StreamStateEnum.ClosedRecover)
                {
                    _websocketClient.Stop = true;
                    _websocketClient.Cts.Cancel();
                    IsConnected(false, false, "Connect");
                }
                var msg = new StringBuilder();
                msg.Append($" MRN_STORY Status:: data state:{e.Status.State.Data} stream state:{e.Status.State.Stream} code:{e.Status.State.Code} status text:{e.Status.State.Text}");
                MrnViewerDesktop.Title = $"{_windowsName}:: {msg.ToString()}";
                MessageBox.Show(msg.ToString());
            }));
        }
        private void ProcessMrnErrorEvent(object sender, MrnErrorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var msg = $"{e.TimeStamp} {e.ErrorMessage}";
                MrnViewerDesktop.Title = $"{_windowsName} Error:: {msg}";
                MessageBox.Show(msg);
            }));
        }
        private void ProcessWebSocketErrorEvent(object sender, ErrorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var msg = new StringBuilder();
                msg.Append($"Websocket Connection state is {e.ClientWebSocketState} ");
                if (e.ClientWebSocketState == WebSocketState.Closed || e.ClientWebSocketState == WebSocketState.Aborted)
                    msg.Append($"{e.ErrorDetails}");
                MrnViewerDesktop.Title = $"{_windowsName}:: {msg}";
            }));
        }
        private void ProcessConnectionEvent(object sender, ConnectionEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var msg = new StringBuilder();
                switch (e.State)
                {
                    case WebSocketState.Open:
                        {
                            if (!_isConnected && _isWait)
                            {
                                IsConnected(true, false, "Disconnect");
                                Task.Run(async () => { await _mrnManager.SendLogin(_dacsUser, _dacsPosition, _appId); });
                                msg.Append($"Connection is {e.State} {e.StatusText}");
                            }
                           
                        }
                        break;
                    case WebSocketState.Aborted:
                    case WebSocketState.Closed:
                        IsConnected(false, false, "Connect");
                        msg.Append($"Connection Closed");
                        break;
                    default:
                        msg.Append($"{e.StatusText}");
                        IsConnected(false, true, "Cancel");
                        break;
                }

                MrnViewerDesktop.Title = $"{_windowsName}:: {msg}";
            }));
        }


        private void MrnStoryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mrnGridData = (Model.MrnStory)MrnStoryDataGrid1.SelectedItem;
            if (mrnGridData?.Story == null) return;
            mrnGridData.Story.Body = mrnGridData.Story.Body.Replace("\\n", "\r\n");
            mrnGridData.Story.Body = mrnGridData.Story.Body.Replace("\\t", "\t");
            var mrnStoryWindows = new ShowStory
            {
                TopicCodeTxt = { Text = string.Join(",", mrnGridData.Story.Subjects) },
                MrnStoryData = mrnGridData
            };
            mrnStoryWindows.PnacTxt.ToolTip = mrnGridData.Story.ToJson();
            mrnStoryWindows.DataContext = mrnStoryWindows.MrnStoryData;
            mrnStoryWindows.Title = $"Story:: {mrnGridData.Story.Headline}";
            mrnStoryWindows.ShowDialog();

        }

        private int _count = 1;
        private WebsocketConnectionClient _websocketClient = null;
        private MrnStoryManager _mrnManager = null;
        private string _dacsUser = string.Empty;
        private string _dacsPosition = string.Empty;
        private string _appId = "256";
        private bool _isConnected = false;
        private bool _isWait = false;

        private readonly ObservableCollection<Model.MrnStory> _fragmentCollection =new ObservableCollection<Model.MrnStory>();
        private readonly string _windowsName = "MRNStory Viewer";


    }
}
