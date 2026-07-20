using System.Globalization;
using StackExchange.Redis;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Classes;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class InactiveSessionCacher(IConnectionMultiplexer redis) : IManageInactiveSessionCache
{
    public async Task AddSessionToCacheAsync(Session session)
    {
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();
        var ttlTimespan = TimeSpan.FromMinutes(SessionCacheHelper.ttl);

        _ = batch.HashSetAsync(SessionCacheHelper.GetSessionDataKey(session.Id), [
            new("Seed", session.Seed.ToString()),
            new("Round", session.Round),
            new("Hint", session.Hint),
            new("UpdateDue", session.UpdateDue),
            new("RoundLimit", session.RoundLimit),
            new("HostId", session.HostId.ToString())]);

        var scoreEntries = session.PlayerScores
            .Select(kvp => new SortedSetEntry(kvp.Key.ToString(), kvp.Value))
            .ToArray();

        if (scoreEntries.Length > 0)
        {
            _ = batch.SortedSetAddAsync(SessionCacheHelper.GetPlayerScoresKey(session.Id), scoreEntries);
        }

        var nameEntries = session.PlayerNames
            .Select(kvp => new HashEntry(kvp.Key.ToString(), kvp.Value))
            .ToArray();

        if (nameEntries.Length > 0)
        {
            _ = batch.HashSetAsync(SessionCacheHelper.GetPlayerNamesKey(session.Id), nameEntries);
        }

        var lastAnsweredEntries = session.PlayerScores.Keys
            .Select(playerId => new HashEntry(playerId.ToString(), 0))
            .ToArray();

        if (lastAnsweredEntries.Length > 0)
        {
            _ = batch.HashSetAsync(SessionCacheHelper.GetLastAnsweredKey(session.Id), lastAnsweredEntries);
        }

        _ = batch.KeyExpireAsync(SessionCacheHelper.GetSessionDataKey(session.Id), ttlTimespan);
        _ = batch.KeyExpireAsync(SessionCacheHelper.GetPlayerScoresKey(session.Id), ttlTimespan);
        _ = batch.KeyExpireAsync(SessionCacheHelper.GetPlayerNamesKey(session.Id), ttlTimespan);
        _ = batch.KeyExpireAsync(SessionCacheHelper.GetLastAnsweredKey(session.Id), ttlTimespan);
        _ = batch.SetAddAsync("sessions:index", session.Id.ToString());

        batch.Execute();
    }

    public async Task DeleteSessionAsync(Guid sessionId)
    {
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();

        _ = batch.KeyDeleteAsync(SessionCacheHelper.GetSessionDataKey(sessionId));
        _ = batch.KeyDeleteAsync(SessionCacheHelper.GetPlayerScoresKey(sessionId));
        _ = batch.KeyDeleteAsync(SessionCacheHelper.GetPlayerNamesKey(sessionId));
        _ = batch.KeyDeleteAsync(SessionCacheHelper.GetLastAnsweredKey(sessionId));
        _ = batch.SetRemoveAsync("sessions:index", sessionId.ToString());

        batch.Execute();
    }

    public async Task RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId)
    {
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();

        var playerIdStr = playerId.ToString();
        var ttlTimespan = TimeSpan.FromMinutes(SessionCacheHelper.ttl);

        var playerScoresKey = SessionCacheHelper.GetPlayerScoresKey(sessionId);
        var playerNamesKey = SessionCacheHelper.GetPlayerNamesKey(sessionId);
        var lastAnsweredKey = SessionCacheHelper.GetLastAnsweredKey(sessionId);

        _ = batch.SortedSetRemoveAsync(playerScoresKey, playerIdStr);
        _ = batch.HashDeleteAsync(playerNamesKey, playerIdStr);
        _ = batch.HashDeleteAsync(lastAnsweredKey, playerIdStr);

        _ = batch.KeyExpireAsync(playerScoresKey, ttlTimespan);
        _ = batch.KeyExpireAsync(playerNamesKey, ttlTimespan);
        _ = batch.KeyExpireAsync(lastAnsweredKey, ttlTimespan);

        batch.Execute();
        await Task.CompletedTask;
    }

    public async Task AddPlayerToSessionAsync(Guid sessionId, Guid playerId, string playerName)
    {
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();

        var playerIdStr = playerId.ToString();
        var ttlTimespan = TimeSpan.FromMinutes(SessionCacheHelper.ttl);

        var playerScoresKey = SessionCacheHelper.GetPlayerScoresKey(sessionId);
        var playerNamesKey = SessionCacheHelper.GetPlayerNamesKey(sessionId);
        var lastAnsweredKey = SessionCacheHelper.GetLastAnsweredKey(sessionId);

        _ = batch.SortedSetAddAsync(playerScoresKey, playerIdStr, 0);
        _ = batch.HashSetAsync(playerNamesKey, playerIdStr, playerName);
        _ = batch.HashSetAsync(lastAnsweredKey, playerIdStr, 0);
        _ = batch.KeyExpireAsync(playerScoresKey, ttlTimespan);
        _ = batch.KeyExpireAsync(playerNamesKey, ttlTimespan);
        _ = batch.KeyExpireAsync(lastAnsweredKey, ttlTimespan);

        batch.Execute();
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Session>> GetAllInactiveSessionsAsync() => await SessionCacheHelper.GetAllSessions(redis, false);

    public async Task<Session?> FetchSessionAsync(Guid sessionId) => await SessionCacheHelper.FetchSessionAsync(redis, sessionId);

    public async Task SetSessionTTLInSecondsAsync(Guid sessionId, int ttl)
    {
        var timespan = TimeSpan.FromSeconds(ttl);
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();

        batch.KeyExpireAsync(SessionCacheHelper.GetSessionDataKey(sessionId), timespan);
        batch.KeyExpireAsync(SessionCacheHelper.GetPlayerNamesKey(sessionId), timespan);
        batch.KeyExpireAsync(SessionCacheHelper.GetPlayerScoresKey(sessionId), timespan);
        batch.KeyExpireAsync(SessionCacheHelper.GetLastAnsweredKey(sessionId), timespan);

        batch.Execute();
        await Task.CompletedTask;
    }
}
