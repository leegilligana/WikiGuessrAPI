using System.Data;

namespace WikiGuessrAPI.Services.Interfaces;

internal interface IWrapDapper
{
    public Task<int> ExecuteAsync(string sql, object? param = null);

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);

    public Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null);

    public Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action);
}
