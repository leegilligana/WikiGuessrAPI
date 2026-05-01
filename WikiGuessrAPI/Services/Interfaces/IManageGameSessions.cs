namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageGameSessions
{
    public Task AddPlayerToSessionAsync(Guid sessionId, Guid playerId);

    public Task DeleteSessionIfExistsAsync(Guid sessionId);

    public Task RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId);

    public Task<Dictionary<Guid, int>> GetPlayerScoresAsync(Guid sessionId);

    public Task<Dictionary<Guid, string>> GetPlayerNamesAsync(Guid sessionId);

    public Task CreateNewGameSessionAsync(int numberOfQuestions);

    public Task<bool> DoesGameSessionExistAsync(Guid sessionId);
}
