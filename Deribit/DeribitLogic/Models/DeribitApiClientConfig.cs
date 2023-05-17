namespace DeribitLogic.Models;

public class DeribitApiClientConfig
{
    public string? BaseUrl { get; init; }

    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public List<string>? SubscribeTo { get; init; }
}

