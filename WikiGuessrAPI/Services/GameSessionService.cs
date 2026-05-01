using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class GameSessionService(
    ILogger<GameSessionService> logger,
    ICacheAndRetrieveGameSessions redisCache) : IManageGameSessions
{
    public async Task AddPlayerToSessionAsync(Guid sessionId, Guid playerId)
    {
        LoggingEvents.LogAddingPlayer(logger, playerId, sessionId);

        var sessionPlayers = (await redisCache.GetSessionAsync(sessionId)).PlayerScores.Keys;

        if (sessionPlayers.Contains(playerId))
        {
            throw new InvalidOperationException("Player is already in session");
        }
        else if (sessionPlayers.Count >= 4)
        {
            throw new InvalidOperationException("Session is full");
        }

        await AddPlayerToSessionAsync(sessionId, playerId);
    }

    public async Task CreateNewGameSessionAsync(int numberOfQuestions, string hostPlayerName)
    {
        LoggingEvents.LogNewSessionAttempt(logger, numberOfQuestions);

        if (numberOfQuestions is <= 0 or > 20)
        {
            throw new ArgumentException("Number of questions must be between 1 and 20");
        }

        var hostGuid = Guid.NewGuid();
        var session = new Session
        {
            Id = Guid.NewGuid(),
            Seed = Guid.NewGuid(),
            PlayerScores = new()
            {
                { hostGuid, 0 },
            },
            PlayerNames = new()
            {
                { hostGuid, hostPlayerName },
            },
            Round = 0,
            RoundLimit = numberOfQuestions,
        };

        await redisCache.AddSessionToCacheAsync(session);

        LoggingEvents.LogNewSessionSuccess(logger, session.Id, session.PlayerScores.Keys.First());
    }

    public async Task IncrementSessionScoreboardAsync(Session session, Dictionary<Guid, int> increases)
    {

    }

    public async Task<bool> DoesGameSessionExistAsync(Guid sessionId) => await redisCache.CheckIfSessionExistsAsync(sessionId);

    public async Task<Dictionary<Guid, int>> GetPlayerScoresAsync(Guid sessionId) => (await redisCache.GetSessionAsync(sessionId)).PlayerScores;

    public async Task<Dictionary<Guid, string>> GetPlayerNamesAsync(Guid sessionId) => (await redisCache.GetSessionAsync(sessionId)).PlayerNames;

    public async Task DeleteSessionIfExistsAsync(Guid sessionId) => await redisCache.DeleteSessionAsync(sessionId);

    public async Task RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId)
    {
        LoggingEvents.LogKickPlayer(logger, playerId, sessionId);

        await redisCache.RemovePlayerFromSession(sessionId, playerId);
    }

    public Task CreateNewGameSessionAsync(int numberOfQuestions) => throw new NotImplementedException();
}
