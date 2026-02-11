using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class AnswerFetcher(IWrapDapper dapperWrapper) : IFetchAnswers
{
    private const string GetAnswerById = """
        SELECT 
            Id, 
            QuestionId, 
            Latitude, 
            Longitude, 
            Radius 
        FROM Answers 
        WHERE Id = @Id
        """;

    public async Task<Answer> FetchAnswerByIdAsync(int id) => await dapperWrapper.QuerySingleAsync<Answer>(GetAnswerById, new { Id = id });
}
