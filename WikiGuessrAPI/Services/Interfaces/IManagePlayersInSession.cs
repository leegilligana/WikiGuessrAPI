namespace WikiGuessrAPI.Services.Interfaces;

internal interface IManagePlayersInSession
{
    public Task<bool> AddPlayerToSessionAsync(Guid gameSessionSeed, Guid playerId);
}
