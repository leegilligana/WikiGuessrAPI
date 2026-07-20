using WikiGuessrAPI.Models;
using WikiGuessrAPI.Models.Exceptions;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services.Classes;

public class ActiveSessionManager(IManageActiveSessionCache redisCache) : IManageActiveSessions
{
    public async Task<Session> FetchSessionAsync(Guid sessionId) =>
        (await redisCache.FetchSessionAsync(sessionId))
        ?? throw new SessionNotFoundException(sessionId);

    public async Task<IEnumerable<Session>> GetActiveSessionsAsync() =>
        await redisCache.GetActiveSessionsAsync()
        ?? throw new SessionListNotFoundException();

    public async Task<Dictionary<string, int>> GetPlayerLeaderboardAsync(Guid sessionId)
    {
        var session = await redisCache.FetchSessionAsync(sessionId);
        return session.PlayerNames.ToDictionary(
                kvp => kvp.Value,
                kvp => session.PlayerScores.GetValueOrDefault(kvp.Key, 0));
    }

    public async Task IncrementHintAsync(Guid sessionId, long updateDue) =>
        await redisCache.IncrementHintAsync(sessionId, updateDue);

    public async Task IncrementPlayerScoreAsync(Guid sessionId, Guid playerId, int scoreIncrease, int round, int hint) =>
        await redisCache.IncrementPlayerScoreAsync(sessionId, playerId, scoreIncrease, round, hint);

    public async Task<Dictionary<string, int>> ProcessRoundEndAndGetLeaderboardAsync(Guid sessionId, long updateDue)
    {
        var session = await redisCache.FetchSessionAsync(sessionId)
            ?? throw new SessionNotFoundException(sessionId);

        return session.PlayerNames.ToDictionary(
                kvp => kvp.Value,
                kvp => session.PlayerScores.GetValueOrDefault(kvp.Key, 0));
    }
}
