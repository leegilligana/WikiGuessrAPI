using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageCachedSessionInfo
{
    public Task AddSessionToCacheAsync(Session session);

    public Task<bool> CheckIfSessionExistsAsync(Guid sessionId);

    public Task DeleteSessionAsync(Guid sessionId);

    public Task<Session?> FetchSessionAsync(Guid sessionId);

    public Task<bool> IncrementPlayerScoreAndCheckIfAllPlayersAnswered(Guid sessionId, Guid playerId, int points, int round);

    public Task RemovePlayerFromSession(Guid sessionId, Guid playerId);
}
