using Microsoft.AspNetCore.SignalR;

namespace web;

public class PlaygroundUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.GetHttpContext()?.Request.Query.TryGetValue("userName", out var userName) ?? false ? userName.ToString() : null;
    }
}