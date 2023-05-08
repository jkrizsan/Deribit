using DeribitLogic.Models;
using DeribitLogic.Wrappers;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DeribitLogic.Services;

public class DeribitService : IDeribitService
{

    private const string _urlPrefix = "wss://";
    //private const string _urlSuffix = "/den/ws"; // not weorking in this way
    private const string _urlSuffix = "/ws/api/v2";
    private const string _authMethod = "public/auth";
    private const string _clientCredentials = "client_credentials";

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

    public async Task AuthenticateAsync()
    {
        var config = _appSettings.DeribitApiClientConfig; //todo: put global variable

        Secret secret = new Secret()
        {
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret,
            GrantType = _clientCredentials
        };

        Message<Secret> message = new Message<Secret>() {Method = _authMethod, Params = secret};

        string jsonString = JsonSerializer.Serialize(message);

        await _clientWebSocketWrapper.SendMessageAsync(jsonString, WebSocketMessageType.Text, CancellationToken.None);

        string result = await _clientWebSocketWrapper.ReceiveMessageAsync(CancellationToken.None);

        //todo: less magic number, add own wxception type
        if (result.Contains("result") == false
            || result.Contains("error") == true) 
        {
            throw new Exception("Authentication error!");
        }
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

    public async Task InitializeAsync()
    {
        await ConnectAsync();
        await AuthenticateAsync();
    }

   

    //TODO: make it async
    private string getUrlBasedOnConfig()
    {
        var config = _appSettings.DeribitApiClientConfig;

        StringBuilder url = new StringBuilder(_urlPrefix);

        url.Append(config.BaseUrl);
        url.Append(_urlSuffix);

        return url.ToString();
    }
}
