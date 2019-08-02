# Creating WebSocket MRN Story Viewer using .NET Core and WPF

## Overview

[Elektron WebSocket API](https://developers.refinitiv.com/elektron/WebSocket-api/learning) is a server-side API which provides an interface to create direct WebSocket access to any OMM Content via ADS. The API leverages standard JSON and WebSocket protocols to be easy to implement and understand. No client API is required and it does mean that developer can use any programming language which provides a JSON parser and a client WebSocket library to connecting to the ADS server and then retrieving or posting the data using the data and messages specification provided on [WebSocket API Developer Guide](https://docs-developers.refinitiv.com/1563871102906/14977/).

This article provides a sample application which demonstrates how to use .NET Core SDK with WPF to create a desktop application to retrieving and displaying MRN News Story from the WebSocket server on ADS 3.2.1 or higher version. The application use ClientWebSocket interface from .NET Core framework with JSON.NET library to create a Client WebSocket adapter library for communicating with the WebSocket server on the ADS. The main reasons for using .NET Core SDK is that we can re-use the WebSocket adapter on multiple platforms and architectures. We can build an application that will run on Windows, but also on Linux, macOS and on different architectures. This is beneficial a lot to many use case, including desktop applications.

Though .NET WPF application currently works only on the Windows platform, we also provide a sample console application for testing the WebSocket functionality and the user can build and run the console app on multiple platforms as well. 

Please note that the sample application can just be connecting and retrieving MRN Story content from TREP ADS only and it does not support the [Elektron Real-Time in the cloud](https://developers.refinitiv.com/elektron/WebSocket-api/learning?content=63493&type=learning_material_item) which requires additional steps to manage the Access Token.

## Prerequisites

* User must have access to existing TREP 3.2.1 or higher which provide a WebSocket connection from Elektron service. The service must support NewsTextAnalytics domain and the user must have permission to request MRN_STORY RIC from the server.
* [.NET Core 3 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0) Preview or later version. It requires version 3 or higher because it's the first version which supports desktop application using WPF and Winform. Please find additional details from [MSDN dev blog](https://devblogs.microsoft.com/dotnet/net-core-3-and-support-for-windows-desktop-applications/) and the following [MSDN article](https://docs.microsoft.com/en-us/dotnet/core/porting/wpf?WT.mc_id=ondotnet-c9-cxa). 
* Visual Studio 2019 or Visual Code to compile and build projects. 
* Understand MRN Story content. Please find more details from [MRN DATA MODELS AND ELEKTRON IMPLEMENTATION GUIDE](https://developers.refinitiv.com/elektron/elektron-sdk-cc/docs?content=8681&type=documentation_item) and there are many articles explain about the MRN Story on Developer Portal. If you have experience with N2_UBMS previously, you can see a comparison from the following [article](https://developers.refinitiv.com/article/machine-readable-news-mrn-n2_ubms-comparison-and-migration-guide).

## Sample Application

![application](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/MainScreenWithStoryPage.JPG)

The main solution for the sample application consisting of four projects. There is two projects for WPF Desktop application and the sample console application. Another two projects are additional libraries for WebSocket Client adapter and MarketData WebSocket library. 

* The WebSocket client adapter is a library built on top of .NET Core ClientWebSocket class. It is responsible for handling a connection to a WebSocket server and sending or receiving a message from the ADS. The adapter provides a set of callback functions or delegates for handling and raising events to notify the application when it received a new message from the server. The application also provides events for connection status and errors from .NET ClientWebSocket. 

* The MarketData WebSocket library is responsible for handling business logic and creating a data model from the MRN JSON data. It also takes responsibility for managing the MRN update message and assembling the MRN fragment and then verifying and decompressing the data to a JSON plain-text. The library also provides an additional interface to convert MRN JSON plain-text to MRN data model class so that the application can access the MRN data model rather than parsing the data from JSON string directly. The library also provides a set of events for delivering an MRN Story data to the application layer and also provide events to notify the application when it has an error inside the library. 

## How the application works

The following sequence diagram depicts an overview of application workflow from the user point of view to the WebSocket server layer.

![MRNWebSocketViewerApp](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/sequence_overall.jpg)

The following is the steps to get the MRN Story data from the ADS server.

### How to open a WebSocket Connection

* User has to input a WebSocket endpoint and the DACS User, AppID and DACS Position and then click connect button. Note that if the ADS setup with DACS enabled, it requires a valid DACS user to connecting to the server. After the user click connects the UI layer has to register to receive events from an event source which is WebSocketClient and MRNStoryManager class. 

* The application then passes input parameters to WebSocketConnectionClient when it creates a new object. This class will be used by MrnStoryManager to make a connection to a WebSocket server and to send and receive a message from the server. Note that it has to add subprotocol "**tr_JSON2**" to connection option otherwise, the server will not accept the connection. 

### How the application receives a message from a WebSocket Connection

* In order to receive the message, the MrnStoryManager will call method Run() to start a new long-running task which implements a loop for receiving a message from .NET ClientWebSocket. It has to run until the user stops the operation and exit the application. The implementation of this class is asynchronous operation and below is a snippet of codes for the implementation from the MRN Viewer app.

```C#
 await Task.Run(async () =>
      {

          _WebSocketClient =new WebSocketConnectionClient("client1", 
                            new Uri(endpointServer), "tr_JSON2")
                            {Cts = new CancellationTokenSource()};

          _mrnManager = new MrnStoryManager(_WebSocketClient);
          _mrnManager.ErrorEvent += ProcessMrnErrorEvent;
          _mrnManager.StatusEvent += ProcessMrnStatusEvent;
          _mrnManager.MessageEvent += ProcessMrnMessageEvent;
          _mrnManager.LoginMessageEvent += ProcessLoginMessageEvent;
          _WebSocketClient.ConnectionEvent += ProcessConnectionEvent;
          _WebSocketClient.ErrorEvent += ProcessWebSocketErrorEvent;
                   
          await _WebSocketClient.Run().ConfigureAwait(false);

      }).ConfigureAwait(false);
```
And below is the implementation of the Run() method. It's an asynchronous method which implements while loop to receive a message from WebSocket connection.

```C#
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

```
### How the application process message from WebSocket connection

* After the connection has been established and the MRNStoryManager will send OMM Login request message to the server and wait until it gets a Login Accept and then send a new item request to the server to request data for MRN_STORY RIC from NewsTextAnalytics domain model.

* To process response message from ClientWebSocket, the MRNStoryManager create an internal event for receiving a message event from WebSocketConnectionClient and it will use JSON.NET to converting a raw buffer back to JSON plain-text and then parse the type of message from JSON message before generating specific events and send it back to UI layer or application layer. As stated earlier, the message could be a single JSON message or pack of a message which is a list that contains multiple sets of JSON data therefore that we need to iterate through the list to process the message one by one. 

In case of the message is a Ping message from a server, it will send the Pong message back immediately to avoid the disconnection due to ping loss issue. Below is a snippet of codes from the implementation.

```C#
 internal void ProcessWebSocketMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var data = Encoding.UTF8.GetString(e.Buffer);
                var messages = JArray.Parse(data);
                foreach (var JSONData in messages.Children())
                    if (JSONData["Type"] != null)
                    {
                        var msgType = (MessageTypeEnum) Enum.Parse(typeof(MessageTypeEnum), (string) JSONData["Type"],
                            true);

                        if (JSONData["Domain"] == null) continue;
                        var rdmDomain = (DomainEnum) Enum.Parse(typeof(DomainEnum), (string) JSONData["Domain"],
                            true);

                        switch (msgType)
                        {
                            case MessageTypeEnum.Error:
                                ProcessError(JSONData);
                                break;
                            case MessageTypeEnum.Ping:
                                _WebSocketMarketDataMgr.SendPingPong(false).GetAwaiter().GetResult();
                                //"Pong Sent"
                                break;
                            default:
                                ProcessMessage(JSONData, msgType, rdmDomain);
                                break;
                        }

                    }
            }
            catch (Exception ex)
            {
                var msg=$"Error ProcessWebSocketMessage() {ex.Message}\n{ex.StackTrace}";
                RaiseErrorEvent(DateTime.Now,msg);
            }
        }

```

* To generate MRN Story events, the MRNStoryManager class has to implement an algorithm to verify whether or not the update message is a complete message and then generate event containing MRN Story data and send it back to the application layer.

* When the message type is update message, it converting the JSON data to MarketPriceUpdateMessage object using method ToObject. It is an extension method which created for converting data using a .NET Reflection. Therefore the name of property inside the class will be the same as field name defined in MRN DATA MODELS AND ELEKTRON IMPLEMENTATION GUIDE document.

Below is a snippet of codes from the MRNStoryManager class when it processes the MRN update messages.

```C#
 var message = JSONData.ToObject<MarketPriceUpdateMessage>();
     message.MsgType = MessageTypeEnum.Update;

    if(message.Fields != null)
      ProcessFieldData(message.Fields);
```
Following codes are the implemention of function ProcessFieldData.

```C#
private bool ProcessFieldData(Dictionary<string, object> Fields)
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
                _mrnDataList[UpdateCount].JSONData = MarketDataUtils
                 .UnpackByteToJSONString(_mrnDataList[UpdateCount].FRAGMENT).GetAwaiter().GetResult();
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
```

* When a data item requires only a single message, then TOT_SIZE will equal the number of bytes in the FRAGMENT and FRAG_NUM will be 1. When multiple messages are required, then the data item can be deemed as fully received once the sum of the number of bytes of each FRAGMENT equals TOT_SUM. The consumer will also observe that all FRAG_NUM range from 1 to the number of a fragment, with no intermediate integers, skipped. In other words, a data item transmitted over three messages will contain FRAG_NUM values of 1, 2 and 3. Above algorithm will check if a message is a complete one, then it will decompressing the data and generate an event to the application layer by calling RaiseMrnMessageEvent.

### How application closes a connection

To close a connection the user can click a Disconnect button and the internal codes will set Stop flag to be true. In this situation they will call Abort() and Dispose() to close the connection to the server like the following snippet of codes.

```C#
 public bool Stop
            {
                get => _stop;
                set
                {
                    _stop = value;
                    if (_stop && this.WebSocket != null)
                    {
                        WebSocket.Abort();
                        WebSocket.Dispose();
                    }
                }
            }
```

## Build and Run Sample applications

There are two sample applications in the Visual Studio solution, MRNViewerApp which is a WPF desktop application, and WebSocketMRNConsumerApp which is a simple console application. 

### Build and Run WebSocketMRNConsumerApp

The WebSocketMRNConsumerApp is a console application demonstrate how to use the WebSokcetAdapter with the MRNManager to open a WebSocket connection to a WebSocket server, send an item request and handling the response message from the server. The example just prints MRN JSON data that the application receives to console output.

You may follow the following steps to build the application.

1) Run the Windows command line or using the terminal on macOS or Linux. Based on mrnWebSocketviewer repository you download from GitHub, change the folder to WebSocketMRNConsumerApp.

2) Make sure that you are running with .NET Core 3.0. Just check by running **dotnet --version**. We are testing with .NET Core 3 Preview 7 which is the latest version at the time we write this article.

```
c:\githubrepos\mrnWebSocketviewer\WebSocketMRNConsumerApp>dotnet --version
3.0.100-preview7-012821
```
3) Open folder WebSocketMRNConsumerApp with visual studio codes and then open file Program.cs. Then modify the server info and DACS user from the following codes.
```csharp
 private static readonly Uri WebSocketUri = new Uri("ws:<ADS Server Name/IP>:15000/WebSocket");
 private static readonly string Clientname = "client1";
 private static readonly string DACS_User = "<DACS User>";
 private static readonly string AppId = "<App ID, defualt 256>";
 private static readonly string Login_Position = "<DACS Login Position>";
```
4) Run **dotnet build** under WebSocketMRNConsumerApp folder. You should see the following console output and you can find the executable file with requires DLLs under folder "bin\Debug\netcoreapp3.0".
```
Welcome to .NET Core 3.0!
---------------------
SDK Version: 3.0.100-preview7-012821

...
--------------------------------------------------------------------------------------
Microsoft (R) Build Engine version 16.3.0-preview-19329-01+d31fdbf01 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  Restore completed in 184.93 ms for C:\githubrepos\mrnWebSocketviewer\WebSocketMRNConsumerApp\WebSocketMRNConsumeConsoleApp.csproj.
  Restore completed in 184.93 ms for C:\githubrepos\mrnWebSocketviewer\WebSocketAdapter\WebSocketAdapter.csproj.
  Restore completed in 184.93 ms for C:\githubrepos\mrnWebSocketviewer\MRNManager\MarketDataWebSocket.csproj.
  You are using a preview version of .NET Core. See: https://aka.ms/dotnet-core-preview
  You are using a preview version of .NET Core. See: https://aka.ms/dotnet-core-preview
  WebSocketAdapter -> C:\githubrepos\mrnWebSocketviewer\WebSocketAdapter\bin\Debug\netstandard2.0\WebSocketAdapter.dll
  MarketDataWebSocket -> C:\githubrepos\mrnWebSocketviewer\MRNManager\bin\Debug\netstandard2.0\MarketDataWebSocket.dll
  WebSocketMRNConsumeConsoleApp -> C:\githubrepos\mrnWebSocketviewer\WebSocketMRNConsumerApp\bin\Debug\netcoreapp3.0\WebSocketMRNConsumeConsoleApp.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:08.54
```
Note that if you want [Self Contained deployment](https://docs.microsoft.com/en-us/dotnet/core/deploying/), you can run **dotnet publish** command as below command where "-c release" is for release build and "release_build" is the name of output folder under WebSocketMRNConsumerApp folder.

```
dotnet publish -c release -r win-x64 -o release_build/
```
You can change **win-x64** to another OS and you can find the list from [rid-catalog page](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).

4) Change folder to "**bin\Debug\netcoreapp3.0**", you should see **WebSocketMRNConsumerConsoleApp.exe** or **WebSocketMRNConsumerConsoleApp** executable file on Linux or macOS.
![Console App](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/consoleappfolder1.JPG)

5) Run WebSocketMRNConsumerConsoleApp executable file and you should see the console output like this.

```
Starting MRN WebSocket Consumer app. Press Ctrl+C to exit.
******************* Process WebSocket Connection Events **********************
OnConnection Event Received:30-07-2019 10:20:04.095
Connection State:Connecting
Message:Connecting to ws://172.20.33.21:15000/WebSocket
*********************************************************************

******************* Process WebSocket Connection Events **********************
OnConnection Event Received:30-07-2019 10:20:04.108
Connection State:Open
Message:Connected to ws://172.20.33.21:15000/WebSocket
*********************************************************************

******************* Process Login Message Events **********************
7/30/2019 10:20:04 AM  received Refresh
Login name:apitest
Login Refresh stream:Open state:Ok code: status text:Login accepted by host APIS21.
*********************************************************************

******************* Process MRN Message Events **********************
TimeStamp:7/30/2019 10:20:05 AM
Received Refresh 7/30/2019 10:20:05 AM

Active Date:2019-07-20
MRN_TYPE:STORY
Context ID:3752
Prod Perm:10001
Fragment Count=1 Total Size=0 bytes
*********************************************************************

******************* Process MRN Message Events **********************
TimeStamp:7/30/2019 10:20:08 AM
Received Update 7/30/2019 10:20:08 AM

Active Date:2019-07-30
MRN_TYPE:STORY
Context ID:0
Prod Perm:0
Fragment Count=1 Total Size=1422 bytes
{
  "altId": "nATR5M3xRC",
  "audiences": [
    "NP:ATRA"
  ],
  "firstCreated": "2019-07-30T03:20:08.165Z",
  "headline": "JAKARTA KEMARIN, UDARA TIDAK SEHAT HINGGA PENDATAAN HEWAN KURBAN",
  "id": "ATR5M3xRC_1907302yDpNcDgBV/yU207NoaycbIvbKcYCpNcno8+bD",
  "language": "id",
  "provider": "NS:ATR",
  "pubStatus": "stat:usable",
  "subjects": [
    "M:1QD",
    "N2:LID"
  ],
  "takeSequence": 1,
  "urgency": 3,
  "versionCreated": "2019-07-30T03:20:08.165Z",
  "body": "\nJakarta (ANTARA) - Sejumlah peristiwa di wilayah Jakarta terjadi pada Senin (29/7). Mulai dari kualitas udara ibu kota yang tidak sehat, kritikan DPRD ke Pemprov DKI hingga lokasi pemotongan hewan kurban.\n\nBerikut rangkuman berita metropolitan yang disajikan oleh LKBN ANTARA.\n\nJakarta masih menyandang predikat sebagai kota dengan kualitas udara terkotor di dunia dengan indeks kualitas udara di laman resmi AirVisual pada Senin pagi pukul 06.00 WIB mencapai angka 188 mikrogram per meter kubik.\n\nPeringkat kedua disusul oleh Tashkent, Uzbekistan dengan angka AQI sebesar 173 mikrogram per meter kubik.\n\nSelengkapnya bisa dibaca di sini\n\nTerkait kualitas udara yang buruk, Dewan Perwakilan Rakyat Daerah (DPRD) DKI Jakarta mendesak Gubernur Anies Baswedan agar segera menyikapi persoalan polusi udara di ibu kota yang terus memprihatinkan karena berdampak buruk bagi kesehatan masyarakat.\n\n\"Jika kualitas udara sudah melebihi batas ambang kesehatan, maka kita harus memaksa gubernur untuk bagaimana mengatasi ini,\" kata Wakil Ketua DPRD DKI Jakarta Ramly HI Muhammad.\n\nMenurut dia, kondisi kualitas udara yang semakin memprihatinkan itu harus disikapi secara cepat oleh pemangku kepentingan terkait. Jika tidak ada respon terhadap masalah bisa menimbulkan ancaman besar di sisi kesehatan.\n\nKarena itu, dalam waktu dekat unsur DPRD segera melakukan pertemuan dengan gubernur untuk mencari solusi dan jalan keluar terkait buruknya kualitas udara di ibu kota.\n\nSelengkapnya bisa dibaca di sini\n\n\n\nMenjelang Hari Raya Idul Adha, Pemerintah Kota Administrasi Jakarta Utara akan melakukan pendataan hewan kurban.\n\nHal itu dilakukan sesuaj Instruksi Gubernur DKI JakartaNomor 46 Yahun 2019 tentang Pengendalian Penampungan dan Pemotongan Hewan dalam rangka Idul Adha 2019/1440 Hijriyah.\n\nAsisten Kesejahteraan Rakyat Kota Administrasi Jakarta Utara, Wawan Budi Rohman mengatakan, pihaknya telah meminta para camat dan lurah melakukan pendataan lokasi penjualan hewan kurban dan lokasi pemotongan.\n\n\"Kita ingin semua lokasi penjualan atau pemotongan tersebut didata semua dan segera dilaporkan ke tingkat kota,\" kata Wawan.\n\nSedangkan untuk lokasi pemotongan, Wawan menuturkan sudah ada lokasi umum seperti masjid, sekolah, kantor atau lainnya.\n\nSelengkapnya bisa dibaca di sini\n"
}
*********************************************************************

******************* Process MRN Message Events **********************
TimeStamp:7/30/2019 10:20:08 AM
Received Update 7/30/2019 10:20:08 AM

Active Date:2019-07-30
MRN_TYPE:STORY
Context ID:0
Prod Perm:0
Fragment Count=1 Total Size=1114 bytes
{
  "altId": "nATR2TTPFQ",
  "audiences": [
    "NP:ATRA"
  ],
  "firstCreated": "2019-07-30T03:20:08.412Z",
  "headline": "BNNP DKI MINTA KAMPUS PROAKTIF IKUT BERANTAS NARKOBA",
  "id": "ATR2TTPFQ_1907302T/PUMB4huO0QPZtFnuZxoB5nLyzFg05NUbVl3",
  "language": "id",
  "provider": "NS:ATR",
  "pubStatus": "stat:usable",
  "subjects": [
    "M:1QD",
    "N2:LID"
  ],
  "takeSequence": 1,
  "urgency": 3,
  "versionCreated": "2019-07-30T03:20:08.412Z",
  "body": "\nJakarta (ANTARA) - Badan Narkotika Nasional Provinsi DKI Jakarta meminta perguruan tinggi lebih proaktif untuk ikut memberantas peredaran gelap narkoba yang sudah mengarah kalangan mahasiswa.\n\n\"Dari kampus juga punya kesadaran diri tinggi mengajukan untuk sosialisasi atau tes urine,\" kata Kepala Bidang Rehabilitasi BNNP DKI Jakarta dr Wahyu Wulandari di Jakarta, Selasa.\n\nWahyu mengimbau perguruan tinggi untuk sadar dan mandiri sehingga nantinya BNN akan membantu kampus dalam memberikan supervisi dalam pemberantasan narkoba.\n\n\n\nBNNP DKI Jakarta, lanjut dia, akan meningkatkan pembentukan relawan dari pihak kampus yang diharapkan mampu mewakili BNN dalam upaya memberantas narkoba.\n\nSaat ini, lanjut dia, sejumlah perguruan tinggi di Jakarta sudah dibentuk relawan antinarkoba.\n\n\"Kalau ada kasus, kami harap orang (relawan) ini mampu tampil dan bisa lapor ke BNN dengan komunikasi intensif,\" ucapnya.\n\nSebelumnya, Polres Metro Jakarta Barat menangkap lima orang pengedar ganja jaringan kampus.\n\nDua di antaranya berinisial TW dan PHS yang merupakan mahasiswa aktif di salah satu perguruan tinggi swasta di Jakarta Timur.\n\nMereka bahkan menggunakan kampusnya untuk bertransaksi narkoba.\n\nSedangkan tiga orang lainnya berinisial HK, AT, dan FF merupakan mahasiswa \"drop-out\".\n\nDari penangkapan tersebut, polisi menyita barang bukti ganja seberat 12 kilogram.\n\nPara tersangka dijerat Pasal 111 Undang-Undang No 35 Tahun 2009 tentang Penyalahgunaan Narkoba dengan ancaman hukuman pidana penjara minimal 20 tahun sampai maksimal seumur hidup.\n"
}
*********************************************************************
...

******************* Process MRN Message Events **********************
TimeStamp:7/30/2019 10:21:02 AM
Received Update 7/30/2019 10:21:02 AM

Active Date:2019-07-30
MRN_TYPE:STORY
Context ID:0
Prod Perm:0
Fragment Count=1 Total Size=410 bytes
{
  "altId": "nPt5lKpln",
  "audiences": [
    "NP:XX"
  ],
  "firstCreated": "2019-07-30T03:21:00.996Z",
  "headline": "PLATTS: 242--Platts/CQI Test Page: 07/30/19 3:21 GMT",
  "id": "Pt5lKpln__1907302gjaNJxsG6xvQZ6VsNDuXuR3HPZgaz7Rd07Qp+",
  "language": "en",
  "provider": "NS:PLTS",
  "pubStatus": "stat:usable",
  "subjects": [
    "M:1QD",
    "R:PGA0242",
    "R:PGA242",
    "N2:LEN"
  ],
  "takeSequence": 1,
  "urgency": 3,
  "versionCreated": "2019-07-30T03:21:00.996Z",
  "body": "New York (Platts)--29Jul19/1121 pm EDT/ 0321 GMT\nPlatts Test 1\nPlatts Test 2\n--Platts Global Alert--\n"
}
*********************************************************************

Exit the app
```

When the connection between the console application and WebSocket server has gone down, the MRN Manager will close the connection and attempt to make a new connection to the server on behalf of the application. So user should be able to receive the update message when it can establish to the WebSocket server again.

## Build and Run MRNViewerApp

The MRNWebSocketViewerApp is a WPF desktop application which supports running on Windows OS only. This project using WPF with .NET Core 3.0 Preview to create the application so you can deploy the app using Self-Contained Deployment(SCD) like the other .NET Core application. The benefit of using this approach, you have control of the version of .NET Core that is deployed with your app and the other apps or updates cannot break the behavior. While some of the disadvantages are that the size of your deployment package is relatively large since you have to include .NET Core as well as your app and its third-party dependencies. Anyway, it still easy to copy only the target build directory to share the application. 

To build the desktop app, you don't need to open the project in Visual Studio. Instead, you can just run **dotnet build** or **dotnet publish** command to build the app. However, if you want to see the UI design view, you need to open a solution file with Visual Studio 2019. At the time we write this article Visual Studio 2017 and the previous version does not support WPF project with .NET Core.

### Building the Desktop App

Please follow below steps to build the application.

1) Running command line on Windows and change folder to "MRNViewerApp".
2) Run command **dotnet build** under folder MRNViewerApp and you should see the following output.

```
c:\githubrepos\mrnWebSocketviewer>dotnet build
Microsoft (R) Build Engine version 16.3.0-preview-19329-01+d31fdbf01 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  Restore completed in 258.65 ms for c:\githubrepos\mrnWebSocketviewer\MRNViewerApp\MRNWebSocketViewerApp.csproj.
  Restore completed in 251.92 ms for c:\githubrepos\mrnWebSocketviewer\WebSocketAdapter\WebSocketAdapter.csproj.
  Restore completed in 251.95 ms for c:\githubrepos\mrnWebSocketviewer\MRNManager\MarketDataWebSocket.csproj.
  Restore completed in 258.4 ms for c:\githubrepos\mrnWebSocketviewer\WebSocketMRNConsumerApp\WebSocketMRNConsumerConsoleApp.csproj.
  You are using a preview version of .NET Core. See: https://aka.ms/dotnet-core-preview
  You are using a preview version of .NET Core. See: https://aka.ms/dotnet-core-preview
  You are using a preview version of .NET Core. See: https://aka.ms/dotnet-core-preview
  You are using a preview version of .NET Core. See: https://aka.ms/dotnet-core-preview
  WebSocketAdapter -> c:\githubrepos\mrnWebSocketviewer\WebSocketAdapter\bin\Debug\netstandard2.0\WebSocketAdapter.dll
  MarketDataWebSocket -> c:\githubrepos\mrnWebSocketviewer\MRNManager\bin\Debug\netstandard2.0\MarketDataWebSocket.dll
  WebSocketMRNConsumerConsoleApp -> c:\githubrepos\mrnWebSocketviewer\WebSocketMRNConsumerApp\bin\Debug\netcoreapp3.0\WebSocketMRNConsumerConsoleApp.dll
  MRNWebSocketViewerApp -> c:\githubrepos\mrnWebSocketviewer\MRNViewerApp\bin\Debug\netcoreapp3.0\MRNWebSocketViewerApp.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:11.23
```

Then you can find MRNWebSocketViewerApp.exe which is WPF application under the folder "bin\Debug\netcoreapp3.0".

3) If you wish to use Self Contained Deployment you can also run the same command line as the console application to build a .NET app.

```
dotnet publish -c release -r win-x64 -o mrnviewerapp_release/
```
Then you can copy mrnviewerapp_release to run on the other Windows 10 and you don't need to install .NET Core before launching the app.

### Running the Desktop App

* User has to launch MRNWebSocketViewerApp.exe and then it should show the following main desktop application.

    ![MRNWebSocketViewerApp](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/MRNViewerAppExe.JPG)

    ![MRNWebSocketViewerApp2](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/exe1.JPG)

* User can set DACS Login(User, AppId and change Position) by click Login button and change WebSocket endpoint to your ADS server.

    ![LoginAndEndpoint](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/exe2.JPG)

* The application shows the Connection status with an additional message via the Desktop Title Bar. The following sample screenshot shows sample messages from the WebSocket adapter in a different situation.

    Server Unavailable or unable to connect to the server.
    ![Error1](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/exe3.JPG)

    The application is making a connection to the server. While it's waiting, the user can cancel the request by click Cancel button.
    ![Error2](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/exe4.JPG)

    The connection to the WebSocket server is closed because the DACS user(user1) does not have permission to access the server.
    ![Error3](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/exe5.JPG)

* Once the connection has been established and Login accepted by the ADS server, the application will send MRN_STORY item request to the ADS and waiting for a Refresh and Update messages back from the server. When the MRN Manager receives a complete MRN update, it will raise a message event back to the application layer and then the application can add the message to the internal list and bind it to DataGridView. 

Below is a screenshot when the application receiving MRN Story update. It shows the timestamp application received the message with the Story Headline in the DataGridView. It also shows a whole message size in bytes and the fragment count which is a number of update messages before it assembly to the same story.
![OnMessage1](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/onmessage1.jpg)

### Displaying a News Story

To display a full story, the user must double click at specific row containing headline they are interesting on the DataGridView and then the application will pop up a new Dialog containing a full story with additional details such as Topics Code related to the News. 

Below is screehttps://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/newsstory1.JPG)

* There is an option for the user to copy only the News Story to the clipboard(click Copy Body to Clipboard) or save original MRN_STORY JSON data to file(click JSON data to file).
![newsstory1](https://raw.githubusercontent.com/Refinitiv-API-Samples/Example.WebSocketAPI.CSharp.MRNWebSocketViewer/master/images/newsstory1.jpg)

Please refer to [MRN DATA MODELS AND ELEKTRON IMPLEMENTATION GUIDE](https://developers.refinitiv.com/elektron/elektron-sdk-cc/docs?content=8681&type=documentation_item) for the structure of the JSON data inside .json file.

* Please note that the application using .NET Dictionary class to catching the MRN_STORY object inside the application, hence the user may experience the case that memory keeps growing when running the application for a long period of time. 

# Summary

This article provides example applications to demonstrate Elektron WebSocket API usage. It also uses ClientWebSocket class from .NET Core 3.0 SDK to communicate with WebSocket server on the TREP ADS server. The solution project also provides a sample WebSocket adapter library which developer can re-use it to build a cross-platform console application and WPF Windows desktop application. The solution project also provides an MRNStoryViewer Desktop application which user can use to test the WebSocket server and displaying a News Story on the desktop application and it also has an option for a user to save the JSON data to file. This should help a user to explorer MRN_STORY data from Elektron data feed.

# Download
Please download solution projects from Github.

# References

* [MRN DATA MODELS AND ELEKTRON IMPLEMENTATION GUIDE](https://developers.refinitiv.com/elektron/elektron-sdk-cc/docs?content=8681&type=documentation_item)
* [Elektron WebSocket API](https://developers.refinitiv.com/elektron/WebSocket-api)
* [Elektron WebSocket API Developer Guide](https://docs-developers.refinitiv.com/1564715686609/14977/)
* [Supporting Windows Forms and WPF in .NET Core 3](https://channel9.msdn.com/Shows/On-NET/Supporting-Windows-Forms-and-WPF-in-NET-Core-3)
* [.NET Core 3 and Support for Windows Desktop Applications](https://devblogs.microsoft.com/dotnet/net-core-3-and-support-for-windows-desktop-applications/)
* [.NET Core dotnet command reference](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet?tabs=netcore21)
* [ClientWebSocket Class .NET Core reference](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket?view=netcore-3.0)

## Authors

* **Moragodkrit Chumsri** - Release 1.0.  *Initial work*

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details


