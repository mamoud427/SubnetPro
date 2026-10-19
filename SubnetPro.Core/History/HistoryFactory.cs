using System.Text.Json;
using SubnetPro.Core.Calculators;
using SubnetPro.Core.Formatting;
using SubnetPro.Core.Models;

namespace SubnetPro.Core.History;

/// <summary>Crée une entrée d'historique à partir d'un résultat de calcul.</summary>
public static class HistoryFactory
{
    public static HistoryEntry FromSubnet(SubnetResult result, DateTime now) =>
        new(
            Guid.NewGuid(),
            HistoryKind.Subnet,
            result.NetworkCidr,
            $"{ResultFormatter.FormatCount(result.UsableHosts)} hôtes · IP saisie : {result.Address}",
            $"{result.Address}/{result.Cidr}",
            now);

    public static HistoryEntry FromVlsm(
        string network, int cidr, IReadOnlyList<VlsmRequirement> requirements, DateTime now)
    {
        var baseNetwork = IPv4.ToText(IPv4.Parse(network) & IPv4.MaskFromCidr(cidr));
        var input = new VlsmInput(baseNetwork, cidr, requirements.ToList());

        var segments = requirements.Count == 1 ? "1 segment" : $"{requirements.Count} segments";

        return new HistoryEntry(
            Guid.NewGuid(),
            HistoryKind.Vlsm,
            $"{baseNetwork}/{cidr}",
            $"{segments} · Besoins : {HostsList(requirements)}",
            JsonSerializer.Serialize(input),
            now);
    }

    /// <summary>« 50, 25 et 10 hôtes ».</summary>
    private static string HostsList(IReadOnlyList<VlsmRequirement> requirements)
    {
        var values = requirements.Select(r => r.Hosts.ToString()).ToList();
        var joined = values.Count switch
        {
            0 => string.Empty,
            1 => values[0],
            _ => string.Join(", ", values.Take(values.Count - 1)) + " et " + values[^1]
        };
        var word = requirements.Count == 1 && requirements[0].Hosts == 1 ? "hôte" : "hôtes";
        return $"{joined} {word}";
    }
}