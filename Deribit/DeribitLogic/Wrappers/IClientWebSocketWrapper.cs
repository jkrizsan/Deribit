using System.Net.WebSockets;

namespace DeribitLogic.Wrappers;

public interface IClientWebSocketWrapper
{
    /// <summary>
    /// Connect to the remote service
    /// </summary>
    /// <param name="url"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task ConnectAsync(string url, CancellationToken token);

    /// <summary>
    /// Disconnect from the remote service and dispose client instance
    /// </summary>
    /// <param name="webSocketCloseStatus"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task DisonnectAndDisposeAsync(WebSocketCloseStatus webSocketCloseStatus, CancellationToken token);
}
