using System.Text.Json.Serialization;

namespace DeribitLogic.Models;

class Message<T> where T : class
{
    [JsonPropertyName("jsonrpc")]
    public double Jsonrpc { get; init; } = 2.0;

    [JsonPropertyName("id")]
    public int Id { get; init; } = 1;

    [JsonPropertyName("method")]
    public string Method { get; set; } = "public/auth";

    [JsonPropertyName("params")]
    public T Params { get; init; }
}

