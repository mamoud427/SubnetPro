using SubnetPro.Core.Accounts;
using SubnetPro.Core.Data;
using SubnetPro.Core.Models;

namespace SubnetPro.Core.History;

/// <summary>
/// Historique persistant dans SQLite, séparé par compte : chaque utilisateur ne voit, n'ouvre
/// et ne supprime que ses propres entrées. Sans utilisateur connecté, l'historique est vide.
/// </summary>
public sealed class SqliteHistoryRepository : IHistoryRepository
{
    private readonly AppDatabase database;
    private readonly IUserContext user;

    public SqliteHistoryRepository(AppDatabase database, IUserContext user)
    {
        this.database = database;
        this.user = user;
    }

    public async Task<IReadOnlyList<HistoryEntry>> GetAllAsync()
    {
        var userId = user.CurrentUserId;
        if (userId is null)
            return Array.Empty<HistoryEntry>();

        var db = await database.GetConnectionAsync();
        var rows = await db.Table<HistoryRow>()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return rows.Select(ToEntry).ToList();
    }

    public async Task<HistoryEntry?> GetAsync(Guid id)
    {
        var userId = user.CurrentUserId;
        if (userId is null)
            return null;

        var db = await database.GetConnectionAsync();
        var row = await db.FindAsync<HistoryRow>(id.ToString());

        return row is null || row.UserId != userId ? null : ToEntry(row);
    }

    public async Task AddAsync(HistoryEntry entry)
    {
        var userId = user.CurrentUserId;
        if (userId is null)
            return;

        var db = await database.GetConnectionAsync();

        // « OR IGNORE » : sans effet si une entrée de même identifiant existe déjà.
        await db.InsertAsync(ToRow(entry, userId), "OR IGNORE");
    }

    public async Task DeleteAsync(Guid id)
    {
        var userId = user.CurrentUserId;
        if (userId is null)
            return;

        var db = await database.GetConnectionAsync();
        await db.ExecuteAsync("DELETE FROM history WHERE Id = ? AND UserId = ?", id.ToString(), userId);
    }

    private static HistoryRow ToRow(HistoryEntry entry, string userId) => new()
    {
        Id = entry.Id.ToString(),
        Kind = (int)entry.Kind,
        Title = entry.Title,
        Summary = entry.Summary,
        Input = entry.Input,
        CreatedAt = entry.CreatedAt,
        UserId = userId
    };

    private static HistoryEntry ToEntry(HistoryRow row) =>
        new(
            Guid.Parse(row.Id),
            (HistoryKind)row.Kind,
            row.Title,
            row.Summary,
            row.Input,
            row.CreatedAt);
}