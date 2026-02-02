using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IFetchAnswers
{
    public Task<Answer> FetchAnswerAsync(int id);
}
