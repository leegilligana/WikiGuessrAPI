using StackExchange.Redis;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services.Classes;

public class ActiveSessionCacher(IConnectionMultiplexer redis) : IManageActiveSessionCache
{
    public async Task<IEnumerable<Session>> GetActiveSessionsAsync() => await SessionCacheHelper.GetAllSessions(redis, true);

    public async Task<Dictionary<Guid, int>> GetPlayerLeaderboardAsync(Guid sessionId) => (await FetchSessionAsync(sessionId)).PlayerScores;

    public async Task<Session> FetchSessionAsync(Guid sessionId) => await SessionCacheHelper.FetchSessionAsync(redis, sessionId);

    public async Task IncrementHintAsync(Guid sessionId, long updateDue)
    {
        var db = redis.GetDatabase();
        var sessionDataKey = SessionCacheHelper.GetSessionDataKey(sessionId);
        var tran = db.CreateTransaction();
        _ = tran.HashIncrementAsync(sessionDataKey, "Hint", 1);
        _ = tran.HashSetAsync(sessionDataKey, "UpdateDue", updateDue);
        await tran.ExecuteAsync();
    }

    public async Task IncrementPlayerScoreAsync(Guid sessionId, Guid playerId, int scoreIncrease, int round, int hint)
    {
        var db = redis.GetDatabase();

        var playerScoresKey = SessionCacheHelper.GetPlayerScoresKey(sessionId);
        var lastAnsweredKey = SessionCacheHelper.GetLastAnsweredKey(sessionId);

        var playerScoresTask = db.SortedSetRangeByRankAsync(playerScoresKey);
        var lastAnsweredTask = db.HashGetAllAsync(lastAnsweredKey);

        await Task.WhenAll(playerScoresTask, lastAnsweredTask);

        var totalPlayersCount = (await playerScoresTask).Length;
        var lastAnsweredHash = await lastAnsweredTask;

        var answeredDict = lastAnsweredHash.ToDictionary(
            x => x.Name.ToString(),
            x => int.Parse(x.Value!));

        answeredDict[playerId.ToString()] = round;

        bool allPlayersHaveAnswered = answeredDict.Values.All(r => r == round) && answeredDict.Count == totalPlayersCount;

        var tran = db.CreateTransaction();

        tran.AddCondition(Condition.HashLengthEqual(lastAnsweredKey, lastAnsweredHash.Length));

        _ = tran.SortedSetIncrementAsync(playerScoresKey, playerId.ToString(), scoreIncrease);
        _ = tran.HashSetAsync(lastAnsweredKey, playerId.ToString(), round);

        await tran.ExecuteAsync();
    }

    public async Task<Dictionary<string, int>> IncrementRoundAndFetchLeaderboardAsync(Guid sessionId, long updateDue)
    {
        var db = redis.GetDatabase();
        var sessionDataKey = SessionCacheHelper.GetSessionDataKey(sessionId);
        var playerScoresKey = SessionCacheHelper.GetPlayerScoresKey(sessionId);
        var playerNamesKey = SessionCacheHelper.GetPlayerNamesKey(sessionId);

        var tran = db.CreateTransaction();
        _ = tran.HashIncrementAsync(sessionDataKey, "Round", 1);
        _ = tran.HashSetAsync(sessionDataKey, "Hint", 0);
        _ = tran.HashSetAsync(sessionDataKey, "UpdateDue", updateDue);

        var ttlTimespan = TimeSpan.FromMinutes(SessionCacheHelper.ttl);
        _ = tran.KeyExpireAsync(playerScoresKey, ttlTimespan);
        _ = tran.KeyExpireAsync(playerNamesKey, ttlTimespan);
        _ = tran.KeyExpireAsync(SessionCacheHelper.GetLastAnsweredKey(sessionId), ttlTimespan);
        _ = tran.KeyExpireAsync(sessionDataKey, ttlTimespan);

        await tran.ExecuteAsync();

        var scoresTask = db.SortedSetRangeByRankWithScoresAsync(playerScoresKey, order: Order.Descending);
        var namesTask = db.HashGetAllAsync(playerNamesKey);

        await Task.WhenAll(scoresTask, namesTask);

        var nameMap = (await namesTask).ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

        return (await scoresTask).ToDictionary(
            x => nameMap.TryGetValue(x.Element.ToString(), out var name) ? name : x.Element.ToString(),
            x => (int)x.Score);
    }
}
