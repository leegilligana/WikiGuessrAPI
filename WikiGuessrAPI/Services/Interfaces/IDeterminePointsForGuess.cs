using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IDeterminePointsForGuess
{
    public Task<int> DeterminePointsForGuess(Guess guess, Answer answer);
}
