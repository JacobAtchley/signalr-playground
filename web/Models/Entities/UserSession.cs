using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace web.Models.Entities;

public class UserSession
{
    [JsonProperty(Order = 1)]
    [Key]
    public string? ConnectionId { get; set; }

    [JsonProperty(Order = 2)]
    public string? UserName { get; set; }

    [JsonProperty(Order = 3)]
    public string? Group { get; set; }

    [JsonProperty(Order = 4)]
    public DateTimeOffset LastConnectedDate { get; set; }
}