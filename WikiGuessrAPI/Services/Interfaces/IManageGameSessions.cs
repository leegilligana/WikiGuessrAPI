namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageGameSessions
{
    public Task<Guid> CreateNewGameSessionAsync(int numberOfQuestions, int timePerQuestionInSeconds);

    public Task<bool> DoesGameSessionExistAsync(Guid gameSessionSeed);
}
