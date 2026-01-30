using WikiGuessrAPI.Models.Interfaces;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface ICreateAndFetchQuestionListQuestions
{
    public Task<IEnumerable<IQuestion>> FetchQuestionsInList(Guid questionListSeed);

    public Task<IQuestion> FetchQuestionInList(Guid questionListSeed, int questionNumber);
}
