using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface ICreateAndFetchQuestionListQuestions
{
    public Task<IEnumerable<Question>> FetchQuestionsListAsync(Guid questionListSeed);

    public Task<Question> FetchQuestionFromListSeedAsync(Guid questionListSeed, int questionNumber);
}
