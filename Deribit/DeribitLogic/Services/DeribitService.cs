using DeribitLogic.Models;
using DeribitLogic.Wrappers;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DeribitLogic.Services;

public class DeribitService : IDeribitService
{
    //TODO: move to config
    private const string _urlPrefix = "wss://";
    private const string _urlPosfix = "/ws/api/v2";
    private const string _authMethod = "public/auth";
    private const string _clientCredentials = "client_credentials";

    private const string _subscribe = "public/subscribe";
    private const string _unsubscribe = "public/unsubscribe";
    private const string _result = "result";
    private const string _error = "error";

    private const int _maxWaitingCount = 10;


    private readonly IClientWebSocketWrapper _clientWebSocketWrapper;
    private ILogger<DeribitService> _logger;

    private volatile bool _isRun = true; // the listener still need to run

    private readonly DeribitApiClientConfig _deribitApiClientConfig;

    public DeribitService(IClientWebSocketWrapper clientWebSocketWrapper,
        ILogger<DeribitService> logger,
        AppSettings appSettings)
    {
        _clientWebSocketWrapper = clientWebSocketWrapper;
        _logger = logger;

        validateSettings(appSettings);

        _deribitApiClientConfig = appSettings?.DeribitApiClientConfig ?? new DeribitApiClientConfig();
    }

    public async Task AuthenticateAsync()
    {
        Secret secret = new Secret()
        {
            ClientId = _deribitApiClientConfig?.ClientId ?? string.Empty,
            ClientSecret = _deribitApiClientConfig?.ClientSecret ?? string.Empty,
            GrantType = _clientCredentials
        };

        Message<Secret> message = buildMessage(_authMethod, secret);

        string jsonString = createJson(message);

        await sendMessageAsync(jsonString);

        string result = await receiveMessageAsync();

        //todo: less magic number, use own exception type
        if (checkResultIsFine(result))
        {
            throw new Exception("Authentication error!");
        }

        _logger.LogInformation("Authentication was successful!");
    }

    public async Task ConnectAsync()
    {
        string url = getUrlBasedOnConfig();

        await _clientWebSocketWrapper.ConnectAsync(url, CancellationToken.None);
    }

    public async Task DisconnectAsync()
    {
        await SubscribeAsync(_unsubscribe);

        await _clientWebSocketWrapper.DisonnectAndDisposeAsync(WebSocketCloseStatus.Empty, CancellationToken.None);

        _logger.LogInformation("Disconnected from the service!");
    }

    public async Task InitializeAsync()
    {
        await ConnectAsync();
        await AuthenticateAsync();
        await SubscribeAsync(_subscribe);
    }

    public async Task RunListener()
    {
        while (_isRun)
        {
            string result = await receiveMessageAsync();

            _logger.LogInformation($"Received: {result}");
        }

        await DisconnectAsync();

        _logger.LogInformation($"The {nameof(RunListener)} is stopped!");
    }

    public async Task SubscribeAsync(string subscribeMethod)
    {
        Channel channel = buildChannel();

        Message<Channel> message = buildMessage<Channel>(subscribeMethod, channel);

        string jsonString = createJson(message);

        await sendMessageAsync(jsonString);

        for (int i = 0; i < _maxWaitingCount; i++)
        {
            string result = await receiveMessageAsync();

            if (result.Contains(_result))
            {
                int numberOfTopice = 0;

                foreach (var topic in _deribitApiClientConfig?.SubscribeTo ?? new List<string>())
                {
                    numberOfTopice = result.Contains(topic) ? ++numberOfTopice : numberOfTopice;
                }

                if (numberOfTopice == _deribitApiClientConfig?.SubscribeTo?.Count)
                {
                    _logger.LogInformation("Subscription/Unsubscription was successful!");
                    return;
                }
                else
                {
                    throw new Exception("Subscription error!");
                }
            }
        }       
    }

    private async Task sendMessageAsync(string jsonString)
    {
        try
        {
            await _clientWebSocketWrapper.SendMessageAsync(jsonString, WebSocketMessageType.Text, CancellationToken.None);
        }
        catch
        {
            throw new Exception("Unexpected error happened while send a message!");
        }
    }

    private async Task<string> receiveMessageAsync()
    {
        try
        {
            return await _clientWebSocketWrapper.ReceiveMessageAsync(CancellationToken.None);
        }
        catch 
        {
            throw new Exception("Unexpected error happened while received message!");
        }
    }

    private Channel buildChannel()
        => new Channel()
        {
            Channels = _deribitApiClientConfig?.SubscribeTo ?? new List<string>()
        };

    private void validateSettings(AppSettings appSettings)
    {
        _ = appSettings ?? throw new ArgumentNullException(nameof(appSettings));

        _ = appSettings?.DeribitApiClientConfig ?? throw new ArgumentNullException(nameof(appSettings.DeribitApiClientConfig));

        if (string.IsNullOrWhiteSpace(appSettings?.DeribitApiClientConfig?.BaseUrl))
        {
            throw new ArgumentNullException($"The {nameof(appSettings.DeribitApiClientConfig.BaseUrl)} must be set!");
        }

        if (string.IsNullOrWhiteSpace(appSettings?.DeribitApiClientConfig?.ClientId))
        {
            throw new ArgumentNullException($"The {nameof(appSettings.DeribitApiClientConfig.ClientId)} must be set!");
        }

        if (string.IsNullOrWhiteSpace(appSettings?.DeribitApiClientConfig?.ClientSecret))
        {
            throw new ArgumentNullException($"The {nameof(appSettings.DeribitApiClientConfig.ClientSecret)} must be set!");
        }

        if (appSettings?.DeribitApiClientConfig?.SubscribeTo == null
            || appSettings?.DeribitApiClientConfig?.SubscribeTo?.Count == 0)
        {
            throw new ArgumentNullException($"The {nameof(appSettings.DeribitApiClientConfig.SubscribeTo)} must be set!");
        }
    }

    private string createJson<T>(Message<T> message) where T : class
        => JsonSerializer.Serialize(message);

    public void SwitchOffListener()
        => _isRun = false;
    
    private bool checkResultIsFine(string result)
        => result.Contains(_result) == false || result.Contains(_error) == true;
    
    private Message<T> buildMessage<T>(string subscribeMethod, T param) where T : class
        => new Message<T>() { Method = subscribeMethod, Params = param};

    private string getUrlBasedOnConfig()
    {
        StringBuilder url = new StringBuilder(_urlPrefix);

        url.Append(_deribitApiClientConfig.BaseUrl);
        url.Append(_urlPosfix);

        return url.ToString();
    }
}
