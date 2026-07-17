using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IFetchQuestions
{
    public Task<Hint> FetchQuestionAsync(int id);
}
