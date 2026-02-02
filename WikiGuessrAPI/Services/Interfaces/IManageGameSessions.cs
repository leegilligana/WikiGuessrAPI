namespace WikiGuessrAPI.Services.Interfaces;

internal interface IManageGameSessions
{
    public Task<Guid> CreateNewGameSessionAsync(int numberOfQuestions, int timePerQuestionInSeconds);

    public Task<bool> DoesGameSessionExistAsync(Guid gameSessionSeed);
}
