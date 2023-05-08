using DeribitLogic.Models;
using DeribitLogic.Services;
using DeribitLogic.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deribit;

//TODO: should use more logging and have to solve a couple of problems that mentioned at the todos.
//Todo: The error handling also should be improved 
public class Deribit
{
    private static ILogger<Deribit> _logger;
    private static IHost? _host;
    private static IDeribitService _deribitService;

    private static IHelper _helper;

    public static async Task Main(string[] args)
    {        
        _host = CreateHostBuilder(args).Build();

        //todo: have to finalize the async related bug
        _helper = _host.Services.GetRequiredService<IHelper>();
        Console.CancelKeyPress += async (sender, e) =>
        {
            await _helper.Console_CancelKeyPressAsync(sender, e);
            e.Cancel = true;
        };

        await runClientAsync();
    }

    private static async Task runClientAsync()
    {
        _deribitService = _host.Services.GetRequiredService<IDeribitService>();
        _logger = _host.Services.GetRequiredService<ILogger<Deribit>>();

        try
        {
            await _deribitService.InitializeAsync();
            await _deribitService.RunListener();
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex.Message);
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                IConfiguration config = new ConfigurationBuilder()
               .AddJsonFile("appSettings.json", true, true)
               .Build();

                services.AddSingleton(config.GetSection(nameof(AppSettings))
                    .Get<AppSettings>());

                services.AddTransient<IClientWebSocketWrapper, ClientWebSocketWrapper>();
                services.AddTransient<IDeribitService, DeribitService>();
                services.AddTransient<IHelper, Helper>();
            });

    //TODO: temp solution
    public interface IHelper
    {
        Task Console_CancelKeyPressAsync(object? sender, ConsoleCancelEventArgs e);
    }

    public class Helper : IHelper
    {
        public async Task Console_CancelKeyPressAsync(object? sender, ConsoleCancelEventArgs e)
        {
            _logger.LogInformation("CTRL+C pressed, initiating graceful shutdown...");

            await _deribitService.DisconnectAsync();

            e.Cancel = true;
        }
    }
}