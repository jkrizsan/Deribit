using System.Net.WebSockets;

namespace DeribitLogic.Wrappers;

public class ClientWebSocketWrapper : IClientWebSocketWrapper
{
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
}
