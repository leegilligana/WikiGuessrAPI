namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageGameSessions
{
    public Task AddPlayerToSessionAsync(Guid sessionId, Guid playerId);

    public Task DeleteSessionIfExistsAsync(Guid sessionId);

    public Task RemovePlayerFromSessionAsync(Guid sessionId, Guid playerId);

    public Task<Dictionary<Guid, int>> GetPlayerScoresAsync(Guid sessionId);

    public Task<Dictionary<Guid, string>> GetPlayerNamesAsync(Guid sessionId);

    public Task<bool> IncrementPlayerScoreAndCheckIfAllAnsweredAsync(Guid sessionId, Guid playerId, int scoreIncrease, int round);

    public Task CreateNewGameSessionAsync(int numberOfQuestions, string hostPlayerName);

    public Task<bool> DoesGameSessionExistAsync(Guid sessionId);
}
