namespace WikiGuessrAPI.Models.Exceptions;

#pragma warning disable SA1649, CA1032, SA1402
public class SessionNotFoundException(Guid sessionId)
    : Exception($"Session not found for id: {sessionId}");

public class SessionListNotFoundException()
    : Exception("Failed to retrieve sessions");

public class SessionIsFullException(Guid sessionId, Guid playerId)
    : Exception($"Session full. Player with id: {playerId} failed to join session with id: {sessionId}");

public class PlayerAlreadyInSessionException(Guid sessionId, Guid playerId)
    : Exception($"Player already in session. Player with id: {playerId} already in session with id: {sessionId}");

public class PlayerNotInSessionException(Guid sessionId, string playerName)
    : Exception($"Player not in session. Player with name: {playerName} not found in session with id: {sessionId}");

public class PlayerNotHostException(Guid sessionId, Guid playerId)
    : Exception($"Player is not host. Player with id: {playerId} is not the host of session with id: {sessionId}");

#pragma warning restore SA1649, CA1032, SA1402
