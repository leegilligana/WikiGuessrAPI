using System.Globalization;
using StackExchange.Redis;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class GameSessionCacher(
    IConnectionMultiplexer redis,
    ICreateAndFetchQuestionListQuestions questionListQuestionsService) : IManageCachedSessionInfo
{
    private readonly int ttl = 10;

    public async Task AddSessionToCacheAsync(Session session)
    {
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();
        var ttlTimespan = TimeSpan.FromMinutes(ttl);

        _ = batch.HashSetAsync(GetSessionDataKey(session.Id), [
            new("Seed", session.Seed.ToString()),
            new("Round", session.Round),
            new("RoundLimit", session.RoundLimit),
            new("HostId", session.HostId.ToString())]);

        var scoreEntries = session.PlayerScores
            .Select(kvp => new SortedSetEntry(kvp.Key.ToString(), kvp.Value))
            .ToArray();

        if (scoreEntries.Length > 0)
        {
            _ = batch.SortedSetAddAsync(GetPlayerScoresKey(session.Id), scoreEntries);
        }

        var nameEntries = session.PlayerNames
            .Select(kvp => new HashEntry(kvp.Key.ToString(), kvp.Value))
            .ToArray();

        if (nameEntries.Length > 0)
        {
            _ = batch.HashSetAsync(GetPlayerNamesKey(session.Id), nameEntries);
        }

        var lastAnsweredEntries = session.PlayerScores.Keys
            .Select(playerId => new HashEntry(playerId.ToString(), 0))
            .ToArray();

        if (lastAnsweredEntries.Length > 0)
        {
            _ = batch.HashSetAsync(GetLastAnsweredKey(session.Id), lastAnsweredEntries);
        }

        _ = batch.KeyExpireAsync(GetSessionDataKey(session.Id), ttlTimespan);
        _ = batch.KeyExpireAsync(GetPlayerScoresKey(session.Id), ttlTimespan);
        _ = batch.KeyExpireAsync(GetPlayerNamesKey(session.Id), ttlTimespan);
        _ = batch.KeyExpireAsync(GetLastAnsweredKey(session.Id), ttlTimespan);

        batch.Execute();
    }

    public async Task DeleteSessionAsync(Guid sessionId)
    {
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();

        _ = batch.KeyDeleteAsync(GetSessionDataKey(sessionId));
        _ = batch.KeyDeleteAsync(GetPlayerScoresKey(sessionId));
        _ = batch.KeyDeleteAsync(GetPlayerNamesKey(sessionId));
        _ = batch.KeyDeleteAsync(GetLastAnsweredKey(sessionId));
        _ = batch.KeyDeleteAsync(GetUpcomingQuestionsKey(sessionId));
        _ = batch.KeyDeleteAsync(GetUpcomingAnswersKey(sessionId));

        batch.Execute();
    }

    public async Task<Session?> FetchSessionAsync(Guid sessionId)
    {
        var db = redis.GetDatabase();

        var sessionDataTask = db.HashGetAllAsync(GetSessionDataKey(sessionId));
        var playerScoresTask = db.SortedSetRangeByRankWithScoresAsync(GetPlayerScoresKey(sessionId));
        var playerNamesTask = db.HashGetAllAsync(GetPlayerNamesKey(sessionId));

        await Task.WhenAll(sessionDataTask, playerScoresTask, playerNamesTask);

        var sessionData = await sessionDataTask;
        if (sessionData.Length == 0)
        {
            return null;
        }

        var sessionDict = sessionData.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

        Dictionary<Guid, string> playerNames = [];
        Dictionary<Guid, int> playerScores = [];

        var playerScoresHash = await playerScoresTask;
        foreach (var scorePair in playerScoresHash)
        {
            var playerId = Guid.Parse(scorePair.Element!.ToString());
            playerScores[playerId] = (int)scorePair.Score;
        }

        var playerNamesHash = await playerNamesTask;
        foreach (var namePair in playerNamesHash)
        {
            var playerId = Guid.Parse(namePair.Name.ToString());
            playerNames[playerId] = namePair.Value!.ToString();
        }

        return new Session
        {
            Id = sessionId,
            Seed = Guid.Parse(sessionDict["Seed"]),
            Round = int.Parse(sessionDict["Round"], CultureInfo.InvariantCulture),
            RoundLimit = int.Parse(sessionDict["RoundLimit"], CultureInfo.InvariantCulture),
            HostId = Guid.Parse(sessionDict["HostId"]),
            PlayerNames = playerNames,
            PlayerScores = playerScores,
        };
    }

    public async Task<bool> IncrementPlayerScoreAndCheckIfAllPlayersAnswered(Guid sessionId, Guid playerId, int points, int round)
    {
        var db = redis.GetDatabase();

        var playerScoresKey = GetPlayerScoresKey(sessionId);
        var lastAnsweredKey = GetLastAnsweredKey(sessionId);

        var playerScoresTask = db.SortedSetRangeByRankAsync(playerScoresKey);
        var lastAnsweredTask = db.HashGetAllAsync(lastAnsweredKey);

        await Task.WhenAll(playerScoresTask, lastAnsweredTask);

        var totalPlayersCount = (await playerScoresTask).Length;
        var lastAnsweredHash = await lastAnsweredTask;

        var answeredDict = lastAnsweredHash.ToDictionary(
            x => x.Name.ToString(),
            x => int.Parse((string)x.Value!, CultureInfo.InvariantCulture));

        answeredDict[playerId.ToString()] = round;

        bool allPlayersHaveAnswered = answeredDict.Values.All(r => r == round) && answeredDict.Count == totalPlayersCount;

        var tran = db.CreateTransaction();

        tran.AddCondition(Condition.HashLengthEqual(lastAnsweredKey, lastAnsweredHash.Length));

        _ = tran.SortedSetIncrementAsync(playerScoresKey, playerId.ToString(), points);
        _ = tran.HashSetAsync(lastAnsweredKey, playerId.ToString(), round);

        var ttlTimespan = TimeSpan.FromMinutes(ttl);
        _ = tran.KeyExpireAsync(playerScoresKey, ttlTimespan);
        _ = tran.KeyExpireAsync(lastAnsweredKey, ttlTimespan);

        bool success = await tran.ExecuteAsync();

        return !success
            ? await IncrementPlayerScoreAndCheckIfAllPlayersAnswered(sessionId, playerId, points, round)
            : allPlayersHaveAnswered;
    }

    public async Task RemovePlayerFromSession(Guid sessionId, Guid playerId)
    {
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();

        var playerIdStr = playerId.ToString();
        var ttlTimespan = TimeSpan.FromMinutes(ttl);

        var playerScoresKey = GetPlayerScoresKey(sessionId);
        var playerNamesKey = GetPlayerNamesKey(sessionId);
        var lastAnsweredKey = GetLastAnsweredKey(sessionId);

        _ = batch.SortedSetRemoveAsync(playerScoresKey, playerIdStr);
        _ = batch.HashDeleteAsync(playerNamesKey, playerIdStr);
        _ = batch.HashDeleteAsync(lastAnsweredKey, playerIdStr);

        _ = batch.KeyExpireAsync(playerScoresKey, ttlTimespan);
        _ = batch.KeyExpireAsync(playerNamesKey, ttlTimespan);
        _ = batch.KeyExpireAsync(lastAnsweredKey, ttlTimespan);

        batch.Execute();
        await Task.CompletedTask;
    }

    public async Task AddPlayerToSession(Guid sessionId, Guid playerId, string playerName)
    {
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();

        var playerIdStr = playerId.ToString();
        var ttlTimespan = TimeSpan.FromMinutes(ttl);

        var playerScoresKey = GetPlayerScoresKey(sessionId);
        var playerNamesKey = GetPlayerNamesKey(sessionId);
        var lastAnsweredKey = GetLastAnsweredKey(sessionId);

        _ = batch.SortedSetAddAsync(playerScoresKey, playerIdStr, 0);
        _ = batch.HashSetAsync(playerNamesKey, playerIdStr, playerName);
        _ = batch.HashSetAsync(lastAnsweredKey, playerIdStr, 0);
        _ = batch.KeyExpireAsync(playerScoresKey, ttlTimespan);
        _ = batch.KeyExpireAsync(playerNamesKey, ttlTimespan);
        _ = batch.KeyExpireAsync(lastAnsweredKey, ttlTimespan);

        batch.Execute();
        await Task.CompletedTask;
    }

    private static string GetSessionDataKey(Guid sessionId) => $"session:{sessionId}";

    private static string GetPlayerScoresKey(Guid sessionId) => $"session:{sessionId}:scores";

    private static string GetPlayerNamesKey(Guid sessionId) => $"session:{sessionId}:names";

    private static string GetLastAnsweredKey(Guid sessionId) => $"session:{sessionId}:lastAnswered";

    private static string GetUpcomingQuestionsKey(Guid sessionId) => $"session:{sessionId}:upcomingAnswers";

    private static string GetUpcomingAnswersKey(Guid sessionId) => $"session:{sessionId}:upcomingQuestions";

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
