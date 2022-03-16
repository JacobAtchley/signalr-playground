namespace web.Models;

public record UserSessionRecord(Guid Id, string? UserName, string? Group, string ConnectionId, DateTimeOffset LastConnectedDate);