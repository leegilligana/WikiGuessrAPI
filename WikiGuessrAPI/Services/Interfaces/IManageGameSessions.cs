using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageGameSessions
{
    public Task<Guid> AddPlayerToSessionAsync(Guid sessionId, string playerName);

    public Task DeleteSessionIfExistsAsync(Guid sessionId);

    public Task RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId);

    public Task<Dictionary<Guid, int>> GetPlayerScoresAsync(Guid sessionId);

    public Task<Dictionary<Guid, string>> GetPlayerNamesAsync(Guid sessionId);

    public Task<bool> IncrementPlayerScoreAndCheckIfAllAnsweredAsync(Guid sessionId, Guid playerId, int scoreIncrease, int round);

    public Task<(Guid SessionGuid, Guid HostGuid)> CreateNewSessionAsync(int numberOfQuestions, string hostPlayerName);

    public Task<Session> FetchSessionAsync(Guid sessionId);

    public Task DeleteSessionIfHostAsync(Guid sessionId, Guid hostId);

    public Task RemovePlayerIfHostAsync(Guid sessionId, Guid hostId, string playerName);
}
