using RedLockNet.SERedis;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class GameSessionOrchestrator(
    IServiceScopeFactory scopeFactory,
    RedLockFactory lockFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var locked = await lockFactory.CreateLockAsync(
                "game_orchestrator_lock",
                TimeSpan.FromSeconds(10),
                TimeSpan.Zero,
                TimeSpan.Zero))
            {
                if (locked.IsAcquired)
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var gameTickService = scope.ServiceProvider.GetRequiredService<IDoGameTicks>();
                        await gameTickService.ExecuteGameTickAsync(stoppingToken);
                    }
                }
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
