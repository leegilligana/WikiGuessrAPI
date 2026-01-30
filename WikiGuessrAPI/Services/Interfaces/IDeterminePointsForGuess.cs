using WikiGuessrAPI.Models.Interfaces;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IDeterminePointsForGuess
{
    public Task<int> DeterminePointsForGuess(IGuess guess, IAnswer answer);
}
