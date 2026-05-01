using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface ICacheAndRetrieveGameSessions
{
    public Task AddSessionToCacheAsync(Session session);

    public Task<bool> CheckIfSessionExistsAsync(Guid sessionId);

    public Task DeleteSessionAsync(Guid sessionId);

    public Task<Session> GetSessionAsync(Guid sessionId);

    public Task<bool> IncrementPlayerScoreAndCheckIfAllPlayersAnswered(Guid sessionId, Guid playerId, int points);

    public Task RemovePlayerFromSession(Guid sessionId, Guid playerId);
}
