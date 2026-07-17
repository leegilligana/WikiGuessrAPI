using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageInactiveSessionCache
{
    public Task AddSessionToCacheAsync(Session session);

    public Task DeleteSessionAsync(Guid sessionId);

    public Task<Session?> FetchSessionAsync(Guid sessionId);

    public Task RemovePlayerFromSession(Guid sessionId, Guid playerId);

    public Task AddPlayerToSession(Guid sessionId, Guid playerId, string playerName);
}
