namespace WikiGuessrAPI.Services.Interfaces;

public interface IManagePlayersInSession
{
    public Task<bool> AddPlayerToSessionAsync(Guid gameSessionSeed, Guid playerId);
}
