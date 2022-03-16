namespace web.Models;

public class PersonalWebSocketEvent
{
    public Guid Id { get; set; }
    public Person? Before { get; set; }
    public Person? After { get; set; }
    public string? Trigger { get; set; }
    public DateTimeOffset Date { get; set; }
}