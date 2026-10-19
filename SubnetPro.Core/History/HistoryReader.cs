using System.Text.Json;
using SubnetPro.Core.Calculators;
using SubnetPro.Core.Models;

namespace SubnetPro.Core.History;

/// <summary>Recalcule le résultat complet d'une entrée à partir de sa saisie.</summary>
public static class HistoryReader
{
    public static SubnetResult ToSubnetResult(HistoryEntry entry)
    {
        var parts = entry.Input.Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var cidr))
            throw new FormatException($"Saisie d'historique invalide : « {entry.Input} ».");

        return SubnetCalculator.Calculate(parts[0], cidr);
    }

    public static VlsmResult ToVlsmResult(HistoryEntry entry)
    {
        var input = JsonSerializer.Deserialize<VlsmInput>(entry.Input)
                    ?? throw new FormatException("Saisie VLSM d'historique invalide.");

        return VlsmCalculator.Allocate(input.Network, input.Cidr, input.Requirements);
    }
}