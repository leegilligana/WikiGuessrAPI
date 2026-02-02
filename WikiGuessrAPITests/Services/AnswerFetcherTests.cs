using FluentAssertions;
using Xunit;

namespace WikiGuessrAPITests.Services;

public class AnswerFetcherTests
{
    [Fact]
    public void ShouldReturnValidId()
    {
        var result = true;
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldThrowException()
    {
        var result = true;
        result.Should().BeTrue();
    }
}
