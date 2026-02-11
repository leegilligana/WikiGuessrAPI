using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IFetchArticles
{
    public Task<Article> FetchArticleAsync(int id);
}
