using WikiGuessrAPI.Models.Interfaces;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IFetchArticles
{
    public Task<IArticle> FetchArticle(int id);
}
