namespace WikiGuessrAPI.Services.Interfaces;

internal interface ICheckIfPlayerNameIsAppropriate
{
    public Task<bool> CheckIfPlayerNameIsAppropriateAsync(string playerName);
}
