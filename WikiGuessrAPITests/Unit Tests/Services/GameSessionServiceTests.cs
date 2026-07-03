using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Models.Exceptions;
using WikiGuessrAPI.Services;
using WikiGuessrAPI.Services.Interfaces;
using Xunit;

namespace WikiGuessrAPITests.Services;

public class GameSessionServiceTests
{
    private readonly Guid toxicPlayer = Guid.NewGuid();

    [Fact]
    public async Task AddPlayerToFullSession()
    {
        var redisCacheMock = new Mock<IManageCachedSessionInfo>();
        var loggerMock = new Mock<ILogger<GameSessionManager>>();
        var gameSessionService = new GameSessionManager(loggerMock.Object, redisCacheMock.Object);
        var session = new Session
        {
            Id = Guid.NewGuid(),
            PlayerScores = new()
            {
                { Guid.NewGuid(), 0 },
                { Guid.NewGuid(), 0 },
                { Guid.NewGuid(), 0 },
                { Guid.NewGuid(), 0 },
            },
            PlayerNames = new()
            {
                { Guid.NewGuid(), "Player1" },
                { Guid.NewGuid(), "Player2" },
                { Guid.NewGuid(), "Player3" },
                { Guid.NewGuid(), "Player4" },
            },
        };

        redisCacheMock.Setup(x => x.FetchSessionAsync(It.IsAny<Guid>())).ReturnsAsync(session);
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var playerGuid = Guid.NewGuid();
        var act = async () => await gameSessionService.AddPlayerToSessionAsync(Guid.NewGuid(), playerGuid);

        await act.Should().ThrowAsync<SessionIsFullException>()
            .WithMessage("Session full*");

        loggerMock.VerifyLog(LogLevel.Information, "Adding player");
    }

    [Fact]
    public async Task AddDuplicatePlayerToSession()
    {
        var duplicateGuid = Guid.NewGuid();
        var redisCacheMock = new Mock<IManageCachedSessionInfo>();
        var loggerMock = new Mock<ILogger<GameSessionManager>>();
        var gameSessionService = new GameSessionManager(loggerMock.Object, redisCacheMock.Object);
        var session = new Session
        {
            Id = Guid.NewGuid(),
            PlayerScores = new()
            {
                { duplicateGuid, 0 },
                { Guid.NewGuid(), 0 },
                { Guid.NewGuid(), 0 },
            },
            PlayerNames = new()
            {
                { duplicateGuid, "Player1" },
                { Guid.NewGuid(), "Player2" },
                { Guid.NewGuid(), "Player3" },
            },
        };

        redisCacheMock.Setup(x => x.FetchSessionAsync(It.IsAny<Guid>())).ReturnsAsync(session);
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var act = async () => await gameSessionService.AddPlayerToSessionAsync(Guid.NewGuid(), duplicateGuid);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Player is already in session");

        loggerMock.VerifyLog(LogLevel.Information, "Adding player");
    }

    [Theory]
    [InlineData(10, false, true)]
    [InlineData(10, false, false)]
    [InlineData(-1, true, false)]
    [InlineData(0, true, false)]
    [InlineData(100, true, false)]
    public async Task CreateNewGameTest(int numQuestions, bool invalidQuestionCount, bool redisError)
    {
        var redisCacheMock = new Mock<IManageCachedSessionInfo>();
        var loggerMock = new Mock<ILogger<GameSessionManager>>();
        var gameSessionService = new GameSessionManager(loggerMock.Object, redisCacheMock.Object);

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        if (redisError)
        {
            redisCacheMock.Setup(x => x.AddSessionToCacheAsync(It.IsAny<Session>())).ThrowsAsync(new ArgumentException("Redis error"));
        }
        else
        {
            redisCacheMock.Setup(x => x.AddSessionToCacheAsync(It.IsAny<Session>())).Returns(Task.CompletedTask);
        }

        if (invalidQuestionCount || redisError)
        {
            var act = async () => await gameSessionService.CreateNewSessionAsync(numQuestions, "Rosetta");
            await act.Should().ThrowAsync<ArgumentException>();
        }
        else
        {
            await gameSessionService.CreateNewSessionAsync(numQuestions, "Eevee");
            redisCacheMock.Verify(x => x.AddSessionToCacheAsync(It.Is<Session>(s => s.RoundLimit == numQuestions)), Times.Once);
        }

        loggerMock.VerifyLog(LogLevel.Information, "Creating new session");

        if (!invalidQuestionCount && !redisError)
        {
            loggerMock.VerifyLog(LogLevel.Information, "Created new session");
        }
    }

    [Fact]
    public async Task KickPlayerTest()
    {
        var redisCacheMock = new Mock<IManageCachedSessionInfo>();
        var loggerMock = new Mock<ILogger<GameSessionManager>>();
        var gameSessionService = new GameSessionManager(loggerMock.Object, redisCacheMock.Object);

        var thisGuy = Guid.NewGuid();
        var session = new Session
        {
            Id = Guid.NewGuid(),
            PlayerScores = new()
            {
                { toxicPlayer, 999 },
                { Guid.NewGuid(), 000 },
                { Guid.NewGuid(), 323 },
                { Guid.NewGuid(), 0 },
            },
            PlayerNames = new()
            {
                { toxicPlayer, "ToxicPlayer" },
                { Guid.NewGuid(), "Player2" },
                { Guid.NewGuid(), "Player3" },
                { Guid.NewGuid(), "Player4" },
            },
        };

        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        redisCacheMock.Setup(x => x.FetchSessionAsync(It.IsAny<Guid>())).ReturnsAsync(session);

        await gameSessionService.RemovePlayerFromSessionAsync(Guid.NewGuid(), toxicPlayer);
        redisCacheMock.Verify(rcm => rcm.RemovePlayerFromSession(It.IsAny<Guid>(), It.Is<Guid>(g => g == toxicPlayer)));
        loggerMock.VerifyLog(LogLevel.Information, "Kicking player");
    }
}
