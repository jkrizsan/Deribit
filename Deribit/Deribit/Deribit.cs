using DeribitLogic.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deribit;

public class Deribit
{
    private readonly static ILogger<Deribit> _logger;
    private static IHost? _host;

    public static async Task Main(string[] args)
    {
        Console.CancelKeyPress += Console_CancelKeyPress;


        _host = CreateHostBuilder(args).Build();
    }

    async static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        _logger.LogInformation("CTRL+C pressed, initiating graceful shutdown...");
        // TODO Stop the API client here
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
            });
}