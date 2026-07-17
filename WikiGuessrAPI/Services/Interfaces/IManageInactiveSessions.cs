using WikiGuessrAPI.Models;
using WikiGuessrAPI.Models.DTO;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageInactiveSessions
{
    public Task<Guid> AddPlayerToSessionAsync(Guid sessionId, string playerName);

    public Task DeleteSessionIfExistsAsync(Guid sessionId);

    public Task RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId);

    public Task<Dictionary<Guid, string>> GetPlayerNamesAsync(Guid sessionId);

    public Task<IEnumerable<Session>> GetAllInactiveSessionsAsync();

    public Task<(Guid SessionGuid, Guid HostGuid)> CreateNewSessionAsync(int numberOfQuestions, string hostPlayerName);

    public Task<Session> FetchSessionAsync(Guid sessionId);

    public Task DeleteSessionIfHostAsync(Guid sessionId, Guid hostId);

    public Task RemovePlayerIfHostAsync(Guid sessionId, Guid hostId, string playerName);

    public Task SetSessionTTLAsync(Guid sessionId, int seconds);
}
