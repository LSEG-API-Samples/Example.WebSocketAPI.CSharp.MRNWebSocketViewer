using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebsocketAdapter
{
    public class ClientWebSocketUtils
    {
        public static async Task SendTextMessage(ClientWebSocket clientWebsocket, string message, bool endOfMessage = true, CancellationToken token = default)
        {
            await Task.Run(async () =>
            {
                var sendBytes = Encoding.UTF8.GetBytes(message);
                var sendBuffer = new ArraySegment<byte>(sendBytes);
                await clientWebsocket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: endOfMessage,
                    cancellationToken: token).ConfigureAwait(false);
            }, token);
            return;
        }
    }
}
