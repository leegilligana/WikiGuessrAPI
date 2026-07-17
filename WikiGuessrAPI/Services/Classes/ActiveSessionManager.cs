using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services.Classes;

public class ActiveSessionManager : IManageActiveSessions
{
    public Task<IEnumerable<Session>> GetActiveSessionsAsync() => throw new NotImplementedException();

    public Task<Dictionary<string, int>> GetPlayerLeaderboardAsync(Guid sessionId) => throw new NotImplementedException();

    public Task IncrementHintAsync(Guid sessionId, long updateDue) => throw new NotImplementedException();

    public Task IncrementPlayerScoreAsync(Guid sessionId, Guid playerId, int scoreIncrease, int round) => throw new NotImplementedException();

    public Task<Dictionary<string, int>> ProcessRoundEndAndGetLeaderboardAsync(Guid sessionId, long updateDue) => throw new NotImplementedException();

    public Task SetSessionUpdateDueAsync(Guid sessionId, long updateDue) => throw new NotImplementedException();
}
