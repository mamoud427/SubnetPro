using System.Globalization;
using System.Text;
using SubnetPro.Core.Models;

namespace SubnetPro.Core.Formatting;

/// <summary>Une ligne « libellé / valeur » prête à afficher.</summary>
public sealed record ResultItem(string Caption, string Value);

/// <summary>Met en forme un résultat de sous-réseau pour l'affichage et la copie (sans dépendance MAUI).</summary>
public static class ResultFormatter
{
    public static IReadOnlyList<ResultItem> ToItems(SubnetResult r) => new List<ResultItem>
    {
        new("Adresse saisie", $"{r.Address}/{r.Cidr}"),
        new("Masque", r.Mask),
        new("Wildcard", r.Wildcard),
        new("Bits réseau / hôte", $"{r.Cidr} / {32 - r.Cidr}"),
        new("Réseau", r.Network),
        new("Broadcast", r.Broadcast),
        new("Premier hôte", r.FirstHost),
        new("Dernier hôte", r.LastHost),
        new("Adresses totales", FormatCount(r.TotalAddresses)),
        new("Hôtes utilisables", FormatCount(r.UsableHosts)),
    };

    public static string ClassLabel(IpClassInfo info) =>
        info.Letter == "Spéciale"
            ? $"Plage spéciale · {info.Usage}"
            : $"Classe {info.Letter} · {(info.IsPrivate ? "Adresse privée" : info.Usage)}";

    /// <summary>Sépare les milliers par une espace insécable : 65 536.</summary>
    public static string FormatCount(long value) =>
        value.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');

    public static string ToClipboardText(SubnetResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Sous-réseau {r.NetworkCidr}");
        sb.AppendLine(ClassLabel(r.Class));
        foreach (var item in ToItems(r))
            sb.AppendLine($"{item.Caption} : {item.Value}");
        return sb.ToString().TrimEnd();
    }
}