using System.Globalization;
using StackExchange.Redis;
using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Classes;

public static class SessionCacheHelper
{
    public static int ttl = 10;

    public static async Task<Session> FetchSessionAsync(IConnectionMultiplexer redis, Guid sessionId)
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
            var playerId = Guid.Parse(scorePair.Element.ToString());
            playerScores[playerId] = (int)scorePair.Score;
        }

        var playerNamesHash = await playerNamesTask;
        foreach (var namePair in playerNamesHash)
        {
            var playerId = Guid.Parse(namePair.Name.ToString());
            playerNames[playerId] = namePair.Value.ToString();
        }

        return new Session
        {
            Id = sessionId,
            Seed = Guid.Parse(sessionDict["Seed"]),
            Round = int.Parse(sessionDict["Round"], CultureInfo.InvariantCulture),
            Hint = int.Parse(sessionDict["Hint"], CultureInfo.InvariantCulture),
            UpdateDue = long.Parse(sessionDict["UpdateDue"], CultureInfo.InvariantCulture),
            RoundLimit = int.Parse(sessionDict["RoundLimit"], CultureInfo.InvariantCulture),
            HostId = Guid.Parse(sessionDict["HostId"]),
            PlayerNames = playerNames,
            PlayerScores = playerScores,
        };
    }

    public static async Task<IEnumerable<Session>> GetAllSessions(IConnectionMultiplexer redis, bool shouldBeActive)
    {
        var db = redis.GetDatabase();
        var sessionIds = await db.SetMembersAsync("sessions:index");
        List<Session> validSessions = [];

        foreach (var id in sessionIds)
        {
            var session = await FetchSessionAsync(redis, Guid.Parse(id.ToString()));

            if (session != null)
            {
                if (session.IsActive == shouldBeActive)
                {
                    validSessions.Add(session);
                }
            }
            else
            {
                await db.SetRemoveAsync("sessions:index", id);
            }
        }

        return validSessions;
    }

    public static string GetSessionDataKey(Guid sessionId) => $"session:{sessionId}";

    public static string GetPlayerScoresKey(Guid sessionId) => $"session:{sessionId}:scores";

    public static string GetPlayerNamesKey(Guid sessionId) => $"session:{sessionId}:names";

    public static string GetLastAnsweredKey(Guid sessionId) => $"session:{sessionId}:lastAnswered";
}
