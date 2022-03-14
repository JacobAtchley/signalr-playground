using Microsoft.AspNetCore.SignalR;
using web.Data;
using web.Services;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IUserSessionStore _userSessionStore;

    public ChatHub(ILogger<ChatHub> logger, IUserSessionStore userSessionStore)
    {
        _logger = logger;
        _userSessionStore = userSessionStore;
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

        await _userSessionStore.AddUserSessionAsync(new UserSessionRecord(Guid.NewGuid(), Context.UserIdentifier, group, Context.ConnectionId, DateTimeOffset.UtcNow), default);

        _logger.LogInformation(
            "User connected to chat hub. {UserId} {ConnectionId}",
            Context.UserIdentifier,
            Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "User disconnected from chat hub. {UserId} {ConnectionId}",
            Context.UserIdentifier,
            Context.ConnectionId);

        await _userSessionStore.RemoveUserSessionAsync(Context.ConnectionId, default);

        await base.OnDisconnectedAsync(exception);
    }
}