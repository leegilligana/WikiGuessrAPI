using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class QuestionListManager : ICreateAndFetchQuestionListQuestions
{
    public Task<Question> FetchQuestionFromListSeedAsync(Guid questionListSeed, int questionNumber) => throw new NotImplementedException();

    public Task<IEnumerable<Question>> FetchQuestionsListAsync(Guid questionListSeed) => throw new NotImplementedException();
}
