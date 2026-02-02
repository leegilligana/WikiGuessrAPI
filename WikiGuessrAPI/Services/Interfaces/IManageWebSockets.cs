namespace WikiGuessrAPI.Services.Interfaces;

internal interface IManageWebSockets
{
    public Task SendMessageToAllPlayersInSessionAsync(Guid gameSessionSeed, string message);

    public Task SendQuestionToAllPlayersAsync(Guid gameSessionSeed, int questionNumber);
}
