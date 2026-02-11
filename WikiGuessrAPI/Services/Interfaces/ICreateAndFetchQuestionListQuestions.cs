using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface ICreateAndFetchQuestionListQuestions
{
    public Task<IEnumerable<Question>> FetchQuestionsListAsync(Guid questionListSeed);

    public Task<Question> FetchQuestionFromListSeedAsync(Guid questionListSeed, int questionNumber);
}
