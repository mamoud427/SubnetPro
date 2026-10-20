using SQLite;

namespace SubnetPro.Core.Data;

/// <summary>Ligne de la table « users ». Les clés normalisées portent l'unicité (insensible à la casse).</summary>
[Table("users")]
public sealed class UserRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Unique]
    public string EmailKey { get; set; } = string.Empty;

    [Unique]
    public string NameKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}