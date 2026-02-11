using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IFetchAnswers
{
    public Task<Answer> FetchAnswerByIdAsync(int id);
}
