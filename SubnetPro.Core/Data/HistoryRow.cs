using SQLite;

namespace SubnetPro.Core.Data;

/// <summary>Ligne de la table « history » (une entrée d'historique à plat, pour SQLite).</summary>
[Table("history")]
public sealed class HistoryRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public int Kind { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Input { get; set; } = string.Empty;

    [Indexed]
    public DateTime CreatedAt { get; set; }

    /// <summary>Identifiant du compte propriétaire : chaque utilisateur ne voit que son historique.</summary>
    [Indexed]
    public string UserId { get; set; } = string.Empty;
}