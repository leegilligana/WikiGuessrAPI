namespace WikiGuessrAPI.Services.Interfaces;

internal interface IDealWithOldSessions
{
    public Task RemoveOldSessionsAsync();
}
