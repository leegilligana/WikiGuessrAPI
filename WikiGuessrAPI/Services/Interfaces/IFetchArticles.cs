using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IFetchArticles
{
    public Task<Article> FetchArticleAsync(int id);
}
