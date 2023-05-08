namespace DeribitLogic.Services;

public interface IDeribitService
{
    /// <summary>
    /// Connect to the remote service via ClientWebSocketWrapper
    /// </summary>
    /// <returns></returns>
    public Task ConnectAsync();

    /// <summary>
    /// Disconnect from the remote service.
    /// </summary>
    /// <returns></returns>
    public Task DisconnectAsync();
}
