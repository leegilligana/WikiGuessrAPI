using Microsoft.AspNetCore.SignalR;
using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IDoGameTicks
{
    public Task ExecuteGameTickAsync(CancellationToken ct);
}
