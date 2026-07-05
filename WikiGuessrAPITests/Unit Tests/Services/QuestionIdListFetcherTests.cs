using FluentAssertions;
using WikiGuessrAPI.Services;
using Xunit;

namespace WikiGuessrAPITests.Unit_Tests.Services;

public class QuestionIdListFetcherTests
{
    [Fact]
    public void GuidShouldReturnSameQuetionIdListForSameSeed()
    {
        Guid seed = Guid.NewGuid();

        var a = QuestionIdListFetcher.GuidToQuestionIdList(seed, 100, 10);
        var b = QuestionIdListFetcher.GuidToQuestionIdList(seed, 100, 10);

        a.Should().BeEquivalentTo(b) // Note: BeEquivalentTo is usually preferred for matching collection contents
            .And.NotBeInAscendingOrder()
            .And.HaveCount(10)
            .And.AllSatisfy(questionId =>
            {
                questionId.Should().BeInRange(0, 100);
            });
    }
}
