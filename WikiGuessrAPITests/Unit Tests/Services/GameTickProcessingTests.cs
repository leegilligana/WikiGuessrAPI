using Microsoft.AspNetCore.SignalR;
using Moq;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services;
using WikiGuessrAPI.Services.Interfaces;
using Xunit;

namespace WikiGuessrAPITests.Unit_Tests.Services;

public class GameTickProcessingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    public async Task TestGameTick(int seconds)
    {
        var mockActiveSessionManager = new Mock<IManageActiveSessions>();
        var mockInactiveSessionManager = new Mock<IManageInactiveSessions>();
        var mockRoundInfoManager = new Mock<IManageRoundInfo>();
        var mockHubContext = new Mock<IHubContext<GameSessionHub>>();

        var endOfGameSession = new Session()
        {
            PlayerNames = new Dictionary<Guid, string>
            {
                { Guid.NewGuid(), "Player1" },
            },
            PlayerScores = new Dictionary<Guid, int>
            {
                { Guid.NewGuid(), 10 },
            },
            Round = 10,
            RoundLimit = 10,
            Hint = 3,
            Id = Guid.NewGuid(),
            Seed = Guid.NewGuid(),
            UpdateDue = DateTimeOffset.UtcNow.AddSeconds(seconds).ToUnixTimeMilliseconds(),
        };

        var nearlyEndOfGameSession = new Session()
        {
            PlayerNames = new Dictionary<Guid, string>
            {
                { Guid.NewGuid(), "Player1" },
            },
            PlayerScores = new Dictionary<Guid, int>
            {
                { Guid.NewGuid(), 10 },
            },
            Round = 8,
            RoundLimit = 10,
            Hint = 4,
            Id = Guid.NewGuid(),
            Seed = Guid.NewGuid(),
            UpdateDue = DateTimeOffset.UtcNow.AddSeconds(seconds).ToUnixTimeMilliseconds(),
        };

        var endOfRoundSession = new Session()
        {
            PlayerNames = new Dictionary<Guid, string>
            {
                { Guid.NewGuid(), "Player1" },
            },
            PlayerScores = new Dictionary<Guid, int>
            {
                { Guid.NewGuid(), 10 },
            },
            Round = 5,
            RoundLimit = 10,
            Id = Guid.NewGuid(),
            Seed = Guid.NewGuid(),
            Hint = 4,
            UpdateDue = DateTimeOffset.UtcNow.AddSeconds(seconds).ToUnixTimeMilliseconds(),
        };

        var midRoundSession = new Session()
        {
            PlayerNames = new Dictionary<Guid, string>
            {
                { Guid.NewGuid(), "Player1" },
            },
            PlayerScores = new Dictionary<Guid, int>
            {
                { Guid.NewGuid(), 10 },
            },
            Round = 5,
            RoundLimit = 10,
            Id = Guid.NewGuid(),
            Seed = Guid.NewGuid(),
            Hint = 3,
            UpdateDue = DateTimeOffset.UtcNow.AddSeconds(seconds).ToUnixTimeMilliseconds(),
        };

        mockActiveSessionManager.Setup(sm => sm.GetActiveSessionsAsync())
            .ReturnsAsync([endOfGameSession, nearlyEndOfGameSession, endOfRoundSession, midRoundSession]);

        mockHubContext.Setup(hc => hc.Clients.Group(It.IsAny<string>()))
            .Returns(Mock.Of<IClientProxy>());

        var ticker = new GameTickProcessor(
            mockHubContext.Object,
            mockActiveSessionManager.Object,
            mockInactiveSessionManager.Object,
            mockRoundInfoManager.Object);

        var ct = CancellationToken.None;
        await ticker.ExecuteGameTickAsync(ct);

        var shouldUpdate = seconds < 0;

        mockRoundInfoManager.Verify(
            rm => rm.GetHintAsync(5, 3, It.IsAny<Guid>()),
            shouldUpdate ? Times.Once() : Times.Never());
        mockRoundInfoManager.Verify(
            rm => rm.PrepareRound(It.IsAny<int>(), It.IsAny<Guid>()),
            shouldUpdate ? Times.Once() : Times.Never());
        mockActiveSessionManager.Verify(
            asm => asm.ProcessRoundEndAndGetLeaderboardAsync(It.IsAny<Guid>(), It.IsAny<long>()),
            shouldUpdate ? Times.Exactly(2) : Times.Never());
        mockInactiveSessionManager.Verify(
            ism => ism.SetSessionTTLAsync(It.IsAny<Guid>(), 30),
            shouldUpdate ? Times.Once() : Times.Never());
    }
}
