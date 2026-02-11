namespace WikiGuessrAPI.Services.Interfaces;

public interface ICheckIfPlayerNameIsAppropriate
{
    public Task<bool> CheckIfPlayerNameIsAppropriateAsync(string playerName);
}
