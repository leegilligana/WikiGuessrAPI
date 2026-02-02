using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IFetchQuestions
{
    public Task<Question> FetchQuestionAsync(int id);
}
