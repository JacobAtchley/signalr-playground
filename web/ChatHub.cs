using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public Task BroadcastMessage(string name, string message) =>
        Clients.All.SendAsync("broadcastMessage", name, message);

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation(
            "User connected to chat hub. {UserId} {ConnectionId}",
            Context.UserIdentifier,
            Context.ConnectionId);

        return base.OnConnectedAsync();
    }
}