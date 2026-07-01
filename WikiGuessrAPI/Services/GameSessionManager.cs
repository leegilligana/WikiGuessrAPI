using WikiGuessrAPI.Models;
using WikiGuessrAPI.Models.Exceptions;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class GameSessionManager(
    ILogger<GameSessionManager> logger,
    IManageCachedSessionInfo redisCache) : IManageGameSessions
{
    public async Task AddPlayerToSessionAsync(Guid sessionId, Guid playerId)
    {
        LoggingEvents.LogAddingPlayer(logger, playerId, sessionId);

        var sessionPlayers = (await redisCache.FetchSessionAsync(sessionId))?.PlayerScores?.Keys
            ?? throw new SessionNotFoundException(sessionId);

        if (sessionPlayers.Contains(playerId))
        {
            throw new InvalidOperationException("Player is already in session");
        }
        else if (sessionPlayers.Count >= 4)
        {
            throw new SessionIsFullException(sessionId, playerId);
        }

        await AddPlayerToSessionAsync(sessionId, playerId);
    }

    public async Task<(Guid SessionGuid, Guid HostGuid)> CreateNewGameSessionAsync(int numberOfQuestions, string hostPlayerName)
    {
        LoggingEvents.LogNewSessionAttempt(logger, numberOfQuestions);

        if (numberOfQuestions is <= 0 or > 20)
        {
            throw new ArgumentException("Number of questions must be between 1 and 20");
        }

        var hostGuid = Guid.NewGuid();
        var sessionGuid = Guid.NewGuid();
        var session = new Session
        {
            Id = sessionGuid,
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

        return (sessionGuid, hostGuid);
    }

    public async Task<bool> DoesGameSessionExistAsync(Guid sessionId) => await redisCache.CheckIfSessionExistsAsync(sessionId);

    public async Task<Dictionary<Guid, int>> GetPlayerScoresAsync(Guid sessionId) =>
        (await redisCache.FetchSessionAsync(sessionId))?.PlayerScores
        ?? throw new SessionNotFoundException(sessionId);

    public async Task<Dictionary<Guid, string>> GetPlayerNamesAsync(Guid sessionId) =>
        (await redisCache.FetchSessionAsync(sessionId))?.PlayerNames
        ?? throw new SessionNotFoundException(sessionId);

    public async Task DeleteSessionIfExistsAsync(Guid sessionId) => await redisCache.DeleteSessionAsync(sessionId);

    public async Task RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId)
    {
        LoggingEvents.LogKickPlayer(logger, playerId, sessionId);

        await redisCache.RemovePlayerFromSession(sessionId, playerId);
    }

    public async Task<bool> IncrementPlayerScoreAndCheckIfAllAnsweredAsync(Guid sessionId, Guid playerId, int scoreIncrease, int round) => await redisCache.IncrementPlayerScoreAndCheckIfAllPlayersAnswered(sessionId, playerId, scoreIncrease, round);
}
