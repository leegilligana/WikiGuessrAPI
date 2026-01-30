using WikiGuessrAPI.Models.Interfaces;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IFetchQuestions
{
    public Task<IQuestion> FetchQuestion(int id);
}
