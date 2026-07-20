using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageActiveSessions
{
    public Task<Dictionary<string, int>> GetPlayerLeaderboardAsync(Guid sessionId);

    public Task IncrementPlayerScoreAsync(Guid sessionId, Guid playerId, int scoreIncrease, int round, int hint);

    public Task<IEnumerable<Session>> GetActiveSessionsAsync();

    public Task<Dictionary<string, int>> ProcessRoundEndAndGetLeaderboardAsync(Guid sessionId, long updateDue);

    public Task IncrementHintAsync(Guid sessionId, long updateDue);

    public Task<Session> FetchSessionAsync(Guid sessionId);
}
