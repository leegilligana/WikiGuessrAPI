using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Classes;
using WikiGuessrAPI.Services.Interfaces;
using Xunit;

namespace WikiGuessrAPITests.E2E;

public class ActiveGameSessionTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory = factory.WithWebHostBuilder(builder =>
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IHostedService) &&
                     d.ImplementationType == typeof(GameSessionOrchestrator));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
        });
    });

    [Fact]
    public async Task IncrementPlayerScoreTest()
    {
        var sessionGuid = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var inactiveSessionCache = scope.ServiceProvider.GetService<IManageInactiveSessionCache>();
        var activeSessionCache = scope.ServiceProvider.GetService<IManageActiveSessionCache>();
        var playerGuid = Guid.NewGuid();
        var sessionToFetch = new Session()
        {
            PlayerNames = new()
            {
                { playerGuid, "Player1" },
            },
            PlayerScores = new()
            {
                { playerGuid, 0 },
            },
            Id = sessionGuid,
            Seed = Guid.NewGuid(),
            HostId = playerGuid,
            Round = -1,
            Hint = 0,
            RoundLimit = 10,
        };

        await inactiveSessionCache.AddSessionToCacheAsync(sessionToFetch);
        await activeSessionCache.IncrementPlayerScoreAsync(sessionGuid, playerGuid, 5, 0, 0);
        var fetchedSession = await activeSessionCache.FetchSessionAsync(sessionGuid);

        var playerScore = fetchedSession.PlayerScores[playerGuid];

        playerScore.Should().Be(5);
    }

    [Fact]
    public async Task IncrementHintTest()
    {
        var sessionGuid = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var inactiveSessionCache = scope.ServiceProvider.GetService<IManageInactiveSessionCache>();
        var activeSessionCache = scope.ServiceProvider.GetService<IManageActiveSessionCache>();
        var playerGuid = Guid.NewGuid();
        var sessionToFetch = new Session()
        {
            PlayerNames = new()
            {
                { playerGuid, "Player1" },
            },
            PlayerScores = new()
            {
                { playerGuid, 0 },
            },
            Id = sessionGuid,
            Seed = Guid.NewGuid(),
            HostId = playerGuid,
            Round = 0,
            Hint = 0,
            RoundLimit = 10,
        };

        await inactiveSessionCache.AddSessionToCacheAsync(sessionToFetch);
        await activeSessionCache.IncrementHintAsync(sessionGuid, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var fetchedSession = await activeSessionCache.FetchSessionAsync(sessionGuid);
        var hint = fetchedSession.Hint;

        hint.Should().Be(1);
    }

    [Fact]
    public async Task IncrementRoundAndReturnLBTest()
    {
        var sessionGuid = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var inactiveSessionCache = scope.ServiceProvider.GetService<IManageInactiveSessionCache>();
        var activeSessionCache = scope.ServiceProvider.GetService<IManageActiveSessionCache>();
        var playerGuid1 = Guid.NewGuid();
        var playerGuid2 = Guid.NewGuid();
        var sessionToFetch = new Session()
        {
            PlayerNames = new()
            {
                { playerGuid1, "Player1" },
                { playerGuid2, "Player2" },
            },
            PlayerScores = new()
            {
                { playerGuid1, 189 },
                { playerGuid2, 140 },
            },
            Id = sessionGuid,
            Seed = Guid.NewGuid(),
            HostId = playerGuid1,
            Round = 0,
            Hint = 2,
            RoundLimit = 10,
        };

        await inactiveSessionCache.AddSessionToCacheAsync(sessionToFetch);
        var lb = await activeSessionCache.IncrementRoundAndFetchLeaderboardAsync(sessionGuid, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        var session = await activeSessionCache.FetchSessionAsync(sessionGuid);

        session.Round.Should().Be(1);
        session.Hint.Should().Be(0);
        lb["Player1"].Should().Be(189);
        lb["Player2"].Should().Be(140);
    }
}
