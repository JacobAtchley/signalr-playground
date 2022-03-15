using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace web.Data;
public class Message
{
    [JsonProperty("text")]
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonProperty("date")]
    [JsonPropertyName("date")]
    public DateTimeOffset? Date { get; set; }
}