using System.Text.Json.Serialization;

namespace DeribitLogic.Models;

public class Channel
{
    [JsonPropertyName("channels")]
    public List<string> Channels { get; init; }

    public Channel()
    {
        Channels = new List<string>();
    }
}
