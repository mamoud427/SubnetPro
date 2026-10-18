using System.Text;
using SubnetPro.Core.Models;

namespace SubnetPro.Core.Formatting;

/// <summary>Ligne du tableau « Répartition obtenue ».</summary>
public sealed record VlsmRow(string Name, string NetworkCidr, string HostRange, string UsableHosts);

/// <summary>Met en forme un résultat VLSM pour l'affichage et la copie (sans dépendance MAUI).</summary>
public static class VlsmFormatter
{
    public static IReadOnlyList<VlsmRow> ToRows(VlsmResult result) =>
        result.Allocations
            .Select(a => new VlsmRow(
                a.Name,
                a.Subnet.NetworkCidr,
                $"Hôtes : {a.Subnet.FirstHost} - {a.Subnet.LastHost}",
                ResultFormatter.FormatCount(a.Subnet.UsableHosts)))
            .ToList();

    public static string Summary(VlsmResult result)
    {
        var remaining = result.AddressesRemaining;
        var tail = remaining switch
        {
            0 => "Aucune adresse ne reste disponible.",
            1 => "1 adresse reste non attribuée.",
            _ => $"{ResultFormatter.FormatCount(remaining)} adresses restent non attribuées."
        };
        return $"Allocation du plus grand besoin au plus petit. {tail}";
    }

    /// <summary>Message d'alerte si certains besoins n'ont pas pu être placés, sinon null.</summary>
    public static string? UnallocatedWarning(VlsmResult result)
    {
        if (result.IsComplete)
            return null;

        var names = string.Join(", ", result.Unallocated.Select(u => $"{u.Name} ({u.Hosts} hôtes)"));
        return $"Place insuffisante dans le réseau de départ pour : {names}.";
    }

    public static string ToClipboardText(VlsmResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Répartition VLSM");
        foreach (var a in result.Allocations)
        {
            sb.AppendLine(
                $"{a.Name} : {a.Subnet.NetworkCidr} ({a.Subnet.FirstHost} - {a.Subnet.LastHost}, " +
                $"{ResultFormatter.FormatCount(a.Subnet.UsableHosts)} hôtes)");
        }

        sb.AppendLine(Summary(result));
        var warning = UnallocatedWarning(result);
        if (warning is not null)
            sb.AppendLine(warning);

        return sb.ToString().TrimEnd();
    }
}