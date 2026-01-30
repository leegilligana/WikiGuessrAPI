using WikiGuessrAPI.Models.Interfaces;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IFetchAnswers
{
    public Task<IAnswer> FetchAnswer(int id);
}
