using DeribitLogic.Models;
using DeribitLogic.Services;
using DeribitLogic.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deribit;

public class Deribit
{
    private static ILogger<Deribit> _logger;
    private static IHost? _host;
    private  static IDeribitService _deribitService;

    public static async Task Main(string[] args)
    {
        Console.CancelKeyPress += Console_CancelKeyPress;


        _host = CreateHostBuilder(args).Build();

        await runClientAsync();

    }

    private static async Task runClientAsync()
    {
        _deribitService = _host.Services.GetRequiredService<IDeribitService>();
        _logger = _host.Services.GetRequiredService<ILogger<Deribit>>();

        try
        {
            await _deribitService.InitializeAsync();
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex.Message);
        }
    }

    async static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        _logger.LogInformation("CTRL+C pressed, initiating graceful shutdown...");

        await _deribitService.DisconnectAsync();

        e.Cancel = true;
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
            });
}