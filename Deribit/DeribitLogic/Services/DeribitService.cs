using DeribitLogic.Models;
using DeribitLogic.Wrappers;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;

namespace DeribitLogic.Services;

public class DeribitService : IDeribitService
{
    private readonly IClientWebSocketWrapper _clientWebSocketWrapper;
    private ILogger<DeribitService> _logger;

    private AppSettings _appSettings;

    public DeribitService(IClientWebSocketWrapper clientWebSocketWrapper,
        ILogger<DeribitService> logger,
        AppSettings appSettings)
    {
        _clientWebSocketWrapper = clientWebSocketWrapper;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task ConnectAsync()
    {
        string url = getUrlBasedOnConfig();

        await _clientWebSocketWrapper.ConnectAsync(url, CancellationToken.None);
    }

    public async Task DisconnectAsync()
    {
        await _clientWebSocketWrapper.DisonnectAndDisposeAsync(WebSocketCloseStatus.NormalClosure, CancellationToken.None);
    }

    //TODO: make it async, use less magic number
    private string getUrlBasedOnConfig()
    {
        var config = _appSettings.DeribitApiClientConfig;

        StringBuilder url = new StringBuilder("wss://");

        url.Append(config.BaseUrl);
        url.Append("/den/ws");

        return url.ToString();
    }
}
