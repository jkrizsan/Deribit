using System.Text.Json.Serialization;

namespace DeribitLogic.Models;

public class Secret
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; init; }

    [JsonPropertyName("client_id")]
    public string ClientId { get; init; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; init; }
}


