using FluentAssertions;
using WikiGuessrAPI.Services.Classes;
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

        a.Should().BeEquivalentTo(b)
            .And.NotBeInAscendingOrder()
            .And.HaveCount(10)
            .And.AllSatisfy(questionId =>
            {
                questionId.Should().BeInRange(0, 100);
            });
    }
}
