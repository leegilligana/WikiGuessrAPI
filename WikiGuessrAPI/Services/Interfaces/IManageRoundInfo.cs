using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IManageRoundInfo
{
    public Task<Hint> GetHintAsync(int round, int hint, Guid seed);

    public Task<(double Latitude, double Longitude)> GetAnswerAsync(int round, Guid seed);

    public Task<IEnumerable<Article>> GetArticlesForSeed(Guid seed);

    public Task PrepareRound(int round, Guid seed);
}
