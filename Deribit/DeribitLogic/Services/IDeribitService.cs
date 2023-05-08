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

    /// <summary>
    /// Here call all the necessary method before the proper running
    /// </summary>
    /// <returns></returns>
    public Task InitializeAsync();

    /// <summary>
    /// Send message for authentication, check the result
    /// </summary>
    /// <returns></returns>
    public Task AuthenticateAsync();

    /// <summary>
    /// Subscribe for topics based on the config
    /// </summary>
    /// <param name="subscribeMethod"></param>
    /// <returns></returns>
    public Task SubscribeAsync(string subscribeMethod);

    /// <summary>
    /// Run the listener until the user stop it.
    /// </summary>
    /// <returns></returns>
    public Task RunListener();
}
