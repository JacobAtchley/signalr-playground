namespace web.Data;

public record UserSessionRecord(Guid Id, string UserName, string Group, string ConnectionId, DateTimeOffset LastConnectedDate);