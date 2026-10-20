using SQLite;

namespace SubnetPro.Core.Data;

/// <summary>
/// Base SQLite unique de l'application. La connexion est ouverte à la première utilisation
/// et les tables sont créées si elles n'existent pas encore (historique et comptes).
/// </summary>
public sealed class AppDatabase : IAsyncDisposable
{
    private readonly string databasePath;
    private readonly SemaphoreSlim initLock = new(1, 1);
    private SQLiteAsyncConnection? connection;

    public AppDatabase(string databasePath)
    {
        this.databasePath = databasePath;
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (connection is not null)
            return connection;

        await initLock.WaitAsync();
        try
        {
            if (connection is null)
            {
                var created = new SQLiteAsyncConnection(
                    databasePath,
                    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

                await created.CreateTableAsync<HistoryRow>();
                await created.CreateTableAsync<UserRow>();
                connection = created;
            }

            return connection;
        }
        finally
        {
            initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
        {
            await connection.CloseAsync();
            connection = null;
        }
    }
}