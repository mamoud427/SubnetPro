using System.Globalization;
using SubnetPro.Core.Models;

namespace SubnetPro.Core.History;

public sealed record HistoryGroup(string Title, IReadOnlyList<HistoryEntry> Items);

/// <summary>Filtre, trie et regroupe l'historique par jour (Aujourd'hui, Hier, puis date complète).</summary>
public static class HistoryGrouper
{
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");

    public static string DayLabel(DateTime date, DateTime now)
    {
        var day = date.Date;
        var today = now.Date;

        if (day == today) return "Aujourd'hui";
        if (day == today.AddDays(-1)) return "Hier";
        return day.ToString("d MMMM yyyy", French);
    }

    private static string Time(DateTime date) => date.ToString("HH:mm", CultureInfo.InvariantCulture);

    /// <summary>« Sous-réseau · 09:32 » (liste de l'historique, le jour est donné par le groupe).</summary>
    public static string ShortLabel(HistoryEntry entry) => $"{entry.KindLabel} · {Time(entry.CreatedAt)}";

    /// <summary>« Sous-réseau · Aujourd'hui, 09:32 » (écran de détail).</summary>
    public static string DetailLabel(HistoryEntry entry, DateTime now) =>
        $"{entry.KindLabel} · {DayLabel(entry.CreatedAt, now)}, {Time(entry.CreatedAt)}";

    public static IReadOnlyList<HistoryEntry> Filter(
        IEnumerable<HistoryEntry> entries, string? query, HistoryKind? kind)
    {
        var result = entries.Where(e => kind is null || e.Kind == kind);

        var q = query?.Trim();
        if (!string.IsNullOrEmpty(q))
        {
            result = result.Where(e =>
                Contains(e.Title, q) || Contains(e.Summary, q) || Contains(e.KindLabel, q));
        }

        return result.OrderByDescending(e => e.CreatedAt).ToList();
    }

    public static IReadOnlyList<HistoryGroup> Group(IEnumerable<HistoryEntry> entries, DateTime now) =>
        entries
            .OrderByDescending(e => e.CreatedAt)
            .GroupBy(e => e.CreatedAt.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new HistoryGroup(DayLabel(g.Key, now), g.ToList()))
            .ToList();

    private static bool Contains(string text, string query) =>
        text.Contains(query, StringComparison.OrdinalIgnoreCase);
}