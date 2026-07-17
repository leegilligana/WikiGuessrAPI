using WikiGuessrAPI.Models;
using WikiGuessrAPI.Models.Exceptions;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class InactiveSessionManager(
    ILogger<InactiveSessionManager> logger,
    IManageInactiveSessionCache redisCache) : IManageInactiveSessions
{
    public async Task<Guid> AddPlayerToSessionAsync(Guid sessionId, string playerName)
    {
        var playerId = Guid.NewGuid();

        LoggingEvents.LogAddingPlayer(logger, playerId, sessionId, playerName);

        var sessionPlayers = (await redisCache.FetchSessionAsync(sessionId))?.PlayerScores?.Keys
            ?? throw new SessionNotFoundException(sessionId);

        if (sessionPlayers.Contains(playerId))
        {
            throw new PlayerAlreadyInSessionException(sessionId, playerId);
        }
        else if (sessionPlayers.Count >= 4)
        {
            throw new SessionIsFullException(sessionId, playerId);
        }

        await redisCache.AddPlayerToSessionAsync(sessionId, playerId, playerName);

        return playerId;
    }

    public async Task<(Guid SessionGuid, Guid HostGuid)> CreateNewSessionAsync(int numberOfQuestions, string hostPlayerName)
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
            HostId = hostGuid,
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

        LoggingEvents.LogNewSessionSuccess(logger, session.Id, hostGuid, hostPlayerName);

        return (sessionGuid, hostGuid);
    }

    public async Task<Session> FetchSessionAsync(Guid sessionId) =>
        await redisCache.FetchSessionAsync(sessionId)
        ?? throw new SessionNotFoundException(sessionId);

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

        await redisCache.RemovePlayerFromSessionAsync(sessionId, playerId);
    }

    public async Task DeleteSessionIfHostAsync(Guid sessionId, Guid hostId)
    {
        LoggingEvents.LogDeleteSessionAttempt(logger, sessionId, hostId);
        var session = await redisCache.FetchSessionAsync(sessionId)
            ?? throw new SessionNotFoundException(sessionId);
        if (session.HostId != hostId)
        {
            throw new PlayerNotHostException(sessionId, hostId);
        }

        await redisCache.DeleteSessionAsync(sessionId);
        LoggingEvents.LogDeleteSessionSuccess(logger, sessionId);
    }

    public async Task RemovePlayerIfHostAsync(Guid sessionId, Guid hostId, string playerName)
    {
        LoggingEvents.LogRemovePlayerAttempt(logger, sessionId, hostId, hostId);
        var session = await redisCache.FetchSessionAsync(sessionId)
            ?? throw new SessionNotFoundException(sessionId);
        if (session.HostId != hostId)
        {
            throw new PlayerNotHostException(sessionId, hostId);
        }

        var playerToRemove = session.PlayerNames.FirstOrDefault(x => x.Value == playerName).Key;
        if (playerToRemove == Guid.Empty)
        {
            throw new PlayerNotInSessionException(sessionId, playerName);
        }

        await redisCache.RemovePlayerFromSessionAsync(sessionId, playerToRemove);
    }

    public async Task<IEnumerable<Session>> GetAllInactiveSessionsAsync() => await redisCache.GetAllInactiveSessionsAsync();

    public async Task SetSessionTTLAsync(Guid sessionId, int seconds) => await redisCache.SetSessionTTLInSecondsAsync(sessionId, seconds);
}
