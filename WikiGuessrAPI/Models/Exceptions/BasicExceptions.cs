namespace WikiGuessrAPI.Models.Exceptions;

public class SessionNotFoundException(Guid sessionId)
    : Exception($"Session not found for id: {sessionId}");

public class SessionIsFullException(Guid sessionId, Guid playerId)
    : Exception($"Session full. Player with id: {playerId} failed to join session with id: {sessionId}");

public class PlayerAlreadyInSessionException(Guid sessionId, Guid playerId)
    : Exception($"Player already in session. Player with id: {playerId} already in session with id: {sessionId}");
