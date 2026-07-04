namespace WikiGuessrAPI;

public static partial class LoggingEvents
{
    [LoggerMessage(
        Level = LogLevel.Information,
        EventId = 1,
        Message = "Adding player {playerName} with id {playerId} to session with id {sessionId}.")]
    public static partial void LogAddingPlayer(ILogger logger, Guid playerId, Guid sessionId, string playerName);

    [LoggerMessage(
        Level = LogLevel.Information,
        EventId = 2,
        Message = "Created new session with id {sessionId}, adding player {playerName} with id {playerId}.")]
    public static partial void LogNewSessionSuccess(ILogger logger, Guid playerId, Guid sessionId, string playerName);

    [LoggerMessage(
        Level = LogLevel.Information,
        EventId = 3,
        Message = "Creating new session with {roundLimit} rounds")]
    public static partial void LogNewSessionAttempt(ILogger logger, int roundLimit);

    [LoggerMessage(
        Level = LogLevel.Information,
        EventId = 4,
        Message = "Kicking player with id {playerId} from session with id {sessionId}.")]
    public static partial void LogKickPlayer(ILogger logger, Guid playerId, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        EventId = 5,
        Message = "Attempting to delete session with id {sessionId}. Triggered by player with id: {hostId}")]
    public static partial void LogDeleteSessionAttempt(ILogger logger, Guid sessionId, Guid hostId);

    [LoggerMessage(
        Level = LogLevel.Information,
        EventId = 6,
        Message = "Session with id {sessionId} deleted successfully.")]
    public static partial void LogDeleteSessionSuccess(ILogger logger, Guid sessionId);

    [LoggerMessage(
        Level = LogLevel.Information,
        EventId = 7,
        Message = "Attempting to remove player with id {playerId} from session with id {sessionId}. Triggered by player with id: {hostId}")]
    public static partial void LogRemovePlayerAttempt(ILogger logger, Guid playerId, Guid sessionId, Guid hostId);
}
