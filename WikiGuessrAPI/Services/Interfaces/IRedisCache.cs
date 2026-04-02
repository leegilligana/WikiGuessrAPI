using WikiGuessrAPI.Models;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IRedisCache
{
    public Task<Question> FetchQuestionAsync(int questionId);

    public Task CacheQuestionAsync(Question question);

    public Task AddSessionToCacheAsync(Session session);

    public Task UpdateSessionAsync(Session session);

    public Task<Session> GetSessionAsync(Guid sessionId);

    public Task<bool> CheckIfSessionExistsAsync(Guid sessionId);

    public Task DeleteSessionAsync(Guid sessionId);
}
