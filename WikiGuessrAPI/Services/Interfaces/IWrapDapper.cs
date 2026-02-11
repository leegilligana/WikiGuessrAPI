using System.Data;

namespace WikiGuessrAPI.Services.Interfaces;

public interface IWrapDapper
{
    public Task<int> ExecuteAsync(string sql, object? param = null);

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);

    public Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action);

    public Task<T> QuerySingleAsync<T>(string sql, object? param = null);
}
