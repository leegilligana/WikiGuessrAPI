namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageWebSockets
{
    public Task SendMessageToAllPlayersInSessionAsync(Guid gameSessionSeed, string message);

    public Task SendQuestionToAllPlayersAsync(Guid gameSessionSeed, int questionNumber);
}
