using FluentAssertions;
using Moq;
using WikiGuessrAPI.Services;
using WikiGuessrAPI.Services.Interfaces;
using Xunit;

namespace WikiGuessrAPITests.Services;

public class AnswerFetcherTests
{
    [Fact]
    public async Task ShouldReturnValidId()
    {
        var dapperMock = new Mock<IWrapDapper>();
        dapperMock.Setup(d => d.QuerySingleAsync<WikiGuessrAPI.Models.Answer>(
            It.Is<string>(s => s.Contains("Answers")),
            It.IsAny<object?>()))
            .ReturnsAsync(new WikiGuessrAPI.Models.Answer { Id = 4, Latitude = 0.0F, Longitude = 1.4F });

        var answerFetcher = new AnswerFetcher(dapperMock.Object);
        var answer = await answerFetcher.FetchAnswerByIdAsync(4);

        answer.Should().NotBeNull();
        answer.Id.Should().Be(4);
    }
}
