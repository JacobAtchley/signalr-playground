using System.ComponentModel.DataAnnotations;

namespace web.Models.Entities;

public class UserSession
{
    [Key]
    public string? ConnectionId { get; set; }
    public string? UserName { get; set; }
    public string? Group { get; set; }
    public DateTimeOffset LastConnectedDate { get; set; }
}