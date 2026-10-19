using SubnetPro.Core.Models;

namespace SubnetPro.Core.History;

/// <summary>Stockage de l'historique. Jour 4 : version en mémoire ; jour 5 : version SQLite dans l'application.</summary>
public interface IHistoryRepository
{
    /// <summary>Toutes les entrées, de la plus récente à la plus ancienne.</summary>
    Task<IReadOnlyList<HistoryEntry>> GetAllAsync();

    Task<HistoryEntry?> GetAsync(Guid id);

    /// <summary>Ajoute l'entrée ; sans effet si une entrée de même identifiant existe déjà.</summary>
    Task AddAsync(HistoryEntry entry);

    Task DeleteAsync(Guid id);
}

public sealed class InMemoryHistoryRepository : IHistoryRepository
{
    private readonly object gate = new();
    private readonly List<HistoryEntry> entries = new();

    public Task<IReadOnlyList<HistoryEntry>> GetAllAsync()
    {
        lock (gate)
        {
            IReadOnlyList<HistoryEntry> copy = entries.OrderByDescending(e => e.CreatedAt).ToList();
            return Task.FromResult(copy);
        }
    }

    public Task<HistoryEntry?> GetAsync(Guid id)
    {
        lock (gate)
            return Task.FromResult(entries.FirstOrDefault(e => e.Id == id));
    }

    public Task AddAsync(HistoryEntry entry)
    {
        lock (gate)
        {
            if (entries.All(e => e.Id != entry.Id))
                entries.Add(entry);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        lock (gate)
            entries.RemoveAll(e => e.Id == id);
        return Task.CompletedTask;
    }
}