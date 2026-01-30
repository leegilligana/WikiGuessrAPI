namespace WikiGuessrAPI.Services.Interfaces;

internal interface ICheckIfPlayerNameIsAppropriate
{
    public Task<bool> CheckIfPlayerNameIsAppropriate(string playerName);
}
