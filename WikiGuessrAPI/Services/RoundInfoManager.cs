using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class RoundInfoManager : IManageRoundInfo
{
    public Task<(double Latitude, double Longitude)> GetAnswerAsync(int round, Guid seed) => throw new NotImplementedException();

    public Task<IEnumerable<Article>> GetArticlesForSeed(Guid seed) => throw new NotImplementedException();

    public Task<Hint> GetHintAsync(int round, int hint, Guid seed) => throw new NotImplementedException();

    public Task PrepareRound(int round, Guid seed) => throw new NotImplementedException();
}
