namespace WikiGuessrAPI.Services.Interfaces;

public interface IDealWithOldSessions
{
    public Task RemoveOldSessionsAsync();
}
