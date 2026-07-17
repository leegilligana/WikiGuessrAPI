using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageInactiveSessionCache
{
    public Task AddSessionToCacheAsync(Session session);

    public Task DeleteSessionAsync(Guid sessionId);

    public Task<Session?> FetchSessionAsync(Guid sessionId);

    public Task RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId);

    public Task AddPlayerToSessionAsync(Guid sessionId, Guid playerId, string playerName);

    public Task<IEnumerable<Session>> GetAllInactiveSessionsAsync();

    public Task SetSessionTTLInSecondsAsync(Guid sessionId, int ttl);
}
