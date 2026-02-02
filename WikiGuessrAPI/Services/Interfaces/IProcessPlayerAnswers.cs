using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IProcessPlayerAnswers
{
    public Task ProcessPlayerAnswer(int gameId, GuessSubmission submissions);
}
