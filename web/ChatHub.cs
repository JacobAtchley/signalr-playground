using Microsoft.AspNetCore.SignalR;
using web.Data;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public Task BroadcastMessage(Message message)
    {
        return Clients.All.SendAsync("broadcastMessage", message);
    }

    public override async Task OnConnectedAsync()
    {
        var group = Context.GetHttpContext()?.Request.Query["group"].ToString();

        if (!string.IsNullOrWhiteSpace(group))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            _logger.LogInformation("User added to group. {User} {Group}", Context.UserIdentifier, group);
        }
        else
        {
            _logger.LogInformation("User is connected with no group specifiedS");
        }


        _logger.LogInformation(
            "User connected to chat hub. {UserId} {ConnectionId}",
            Context.UserIdentifier,
            Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "User disconnected from chat hub. {UserId} {ConnectionId}",
            Context.UserIdentifier,
            Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }
}