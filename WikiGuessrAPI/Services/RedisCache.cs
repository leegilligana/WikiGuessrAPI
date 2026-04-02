using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services;

public class RedisCache : IRedisCache
{
    public Task AddSessionToCacheAsync(Session session) => throw new NotImplementedException();

    public Task CacheQuestionAsync(Question question) => throw new NotImplementedException();

    public Task<bool> CheckIfSessionExistsAsync(Guid sessionId) => throw new NotImplementedException();

    public Task DeleteSessionAsync(Guid sessionId) => throw new NotImplementedException();

    public Task<Question> FetchQuestionAsync(int questionId) => throw new NotImplementedException();

    public Task<Session> GetSessionAsync(Guid sessionId) => throw new NotImplementedException();

    public Task UpdateSessionAsync(Session session) => throw new NotImplementedException();
}
