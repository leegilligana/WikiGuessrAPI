using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class GameSessionCacher(
    IConnectionMultiplexer redis,
    IDistributedCache cache) : ICacheAndRetrieveGameSessions
{
    private readonly int ttl = 10;

    public async Task AddSessionToCacheAsync(Session session)
    {
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();

        var sessionDataKey = $"session:{session.Id}";
        var playerScoresKey = $"session:{session.Id}:scores";
        var playerNamesKey = $"session:{session.Id}:names";
        var lastAnsweredKey = $"session:{session.Id}:lastAnswered";

        HashEntry[] sessionData =
        [
            new("Seed", session.Seed.ToString()),
            new("Round", session.Round),
            new("RoundLimit", session.RoundLimit)
        ];
        _ = db.HashSetAsync(sessionDataKey, sessionData);
        _ = db.SortedSetAddAsync(playerScoresKey, session.PlayerScores.Keys.First().ToString(), 0);
        _ = db.HashSetAsync(playerNamesKey, session.PlayerNames.Keys.First().ToString(), session.PlayerNames.Values.First());
        _ = db.HashSetAsync(lastAnsweredKey, session.PlayerNames.Keys.First().ToString(), 0);

        _ = db.KeyExpireAsync(sessionDataKey, TimeSpan.FromMinutes(ttl));
        _ = db.KeyExpireAsync(playerScoresKey, TimeSpan.FromMinutes(ttl));
        _ = db.KeyExpireAsync(playerNamesKey, TimeSpan.FromMinutes(ttl));
        _ = db.KeyExpireAsync(lastAnsweredKey, TimeSpan.FromMinutes(ttl));

        batch.Execute();
    }

    public Task<bool> CheckIfSessionExistsAsync(Guid sessionId) => throw new NotImplementedException();

    public Task DeleteSessionAsync(Guid sessionId) => throw new NotImplementedException();

    public Task<Session> GetSessionAsync(Guid sessionId) => throw new NotImplementedException();

    public Task<bool> IncrementPlayerScoreAndCheckIfAllPlayersAnswered(Guid sessionId, Guid playerId, int points) => throw new NotImplementedException();

    public Task RemovePlayerFromSession(Guid sessionId, Guid playerId) => throw new NotImplementedException();

    private async Task ResetSessionTTL(Guid sessionId)
    {
        var timespan = TimeSpan.FromMinutes(ttl);
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();

        await batch.KeyExpireAsync($"session:{sessionId}", timespan);
        await batch.KeyExpireAsync($"session:{sessionId}:players", timespan);
        await batch.KeyExpireAsync($"session:{sessionId}:scores", timespan);
        await batch.KeyExpireAsync($"session:{sessionId}:lastAnswered", timespan);
    }
}
