using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IDeterminePointsForGuess
{
    public Task<int> DeterminePointsForGuess(Guess guess, Answer answer);
}
