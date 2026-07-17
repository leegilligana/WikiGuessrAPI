using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using WikiGuessrAPI.Services.Interfaces;

namespace WikiGuessrAPI.Services.Classes;

public class DapperWrapper(string connectionString) : IWrapDapper
{
    private readonly string connectionString = connectionString;

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        using var connection = new SqlConnection(connectionString);
        return await connection.ExecuteAsync(sql, param);
    }

    public async Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action)
    {
        using var connection = new SqlConnection(connectionString);
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await action(transaction);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        using var connection = new SqlConnection(connectionString);
        return await connection.QueryAsync<T>(sql, param);
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? param = null)
    {
        using var connection = new SqlConnection(connectionString);
        return await connection.QuerySingleAsync<T>(sql, param);
    }
}
