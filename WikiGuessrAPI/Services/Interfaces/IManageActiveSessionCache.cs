using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageActiveSessionCache
{
    public Task<IEnumerable<Session>> GetActiveSessionsAsync();

    public Task<Session> FetchSessionAsync(Guid sessionId);

    public Task IncrementHintAsync(Guid sessionId, long updateDue);

    public Task<Dictionary<string, int>> IncrementRoundAndFetchLeaderboardAsync(Guid sessionId, long updateDue);

    public Task IncrementPlayerScoreAsync(Guid sessionId, Guid playerId, int scoreIncrease, int round, int hint);
}
