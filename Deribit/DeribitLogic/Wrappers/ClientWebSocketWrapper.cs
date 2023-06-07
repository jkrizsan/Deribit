using System.Net.WebSockets;
using System.Text;

namespace DeribitLogic.Wrappers;

public class ClientWebSocketWrapper : IClientWebSocketWrapper
{
    private const int _bufferSize = 1024;
    private readonly ClientWebSocket _clientWebSocket;

    public ClientWebSocketWrapper()
    {
        _clientWebSocket = new ClientWebSocket();
    }

    public async Task ConnectAsync(string url, CancellationToken token)
    {
        Uri uri = new Uri(url);
        await _clientWebSocket.ConnectAsync(uri, token);
    }

    public async Task DisonnectAndDisposeAsync(WebSocketCloseStatus webSocketCloseStatus, CancellationToken token)
    {
        await _clientWebSocket.CloseAsync(webSocketCloseStatus, string.Empty, token);
        _clientWebSocket.Dispose();
    }

    public async Task<string> ReceiveMessageAsync(CancellationToken token)
    {
        var buffer = new byte[_bufferSize];
        var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

        return  await Task.Run(() => Encoding.UTF8.GetString(buffer, 0, result.Count));
    }

    public async Task SendMessageAsync(string message, WebSocketMessageType type, CancellationToken token)
    {
        byte[]? encoded = Encoding.UTF8.GetBytes(message);
        ArraySegment<Byte> buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);

        await _clientWebSocket.SendAsync(buffer, type, true, token);
    }
}
