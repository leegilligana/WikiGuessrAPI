using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IFetchQuestions
{
    public Task<Question> FetchQuestionAsync(int id);
}
