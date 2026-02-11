using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IProcessPlayerAnswers
{
    public Task ProcessPlayerAnswer(int gameId, GuessSubmission submissions);
}
