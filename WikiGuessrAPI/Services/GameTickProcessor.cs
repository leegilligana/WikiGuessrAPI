using Microsoft.AspNetCore.SignalR;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class GameTickProcessor(
    IHubContext<GameSessionHub> hubContext,
    IManageActiveSessions activeSessionManager,
    IManageInactiveSessions inactiveSessionManager,
    IManageRoundInfo roundInfoManager) : IDoGameTicks
{
    public async Task ExecuteGameTickAsync(CancellationToken ct)
    {
        var activeSessions = await activeSessionManager.GetActiveSessionsAsync();

        foreach (var session in activeSessions)
        {
            long curTimeUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (curTimeUnixMs >= session.UpdateDue)
            {
                if (session.Round >= session.RoundLimit)
                {
                    await ResolveSessionEndAsync(session);
                    continue;
                }

                await UpdateSessionAsync(session, ct);
            }
        }
    }

    private async Task UpdateSessionAsync(Session session, CancellationToken ct)
    {
        if (session.Hint > 3)
        {
            var nextUpdateDue = DateTimeOffset.UtcNow.AddSeconds(5).ToUnixTimeMilliseconds();
            var lb = await activeSessionManager.ProcessRoundEndAndGetLeaderboardAsync(session.Id, nextUpdateDue);
            await hubContext.Clients.Group(session.Id.ToString()).SendAsync("RoundEnded", lb, ct);

            if (session.Round + 2 < session.RoundLimit)
            {
                await roundInfoManager.PrepareRound(session.Round + 2, session.Seed);
            }
        }
        else
        {
            var curHint = await roundInfoManager.GetHintAsync(session.Round, session.Hint, session.Seed);
            await hubContext.Clients.Group(session.Id.ToString()).SendAsync("NewHint", curHint, ct);

            var nextUpdateDue = DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeMilliseconds();
            await activeSessionManager.IncrementHintAsync(session.Id, nextUpdateDue);
        }
    }

    private async Task ResolveSessionEndAsync(Session session)
    {
        var lb = await activeSessionManager.GetPlayerLeaderboardAsync(session.Id);
        await hubContext.Clients.Group(session.Id.ToString()).SendAsync("SessionEnded", lb, cancellationToken: CancellationToken.None);

        await inactiveSessionManager.SetSessionTTLAsync(session.Id, 30);
    }
}
