namespace SubnetPro.Core.Models;

public enum HistoryKind
{
    Subnet,
    Vlsm
}

/// <summary>
/// Un calcul de l'historique. <see cref="Input"/> contient les données de saisie
/// (pour un sous-réseau : « 192.168.10.34/26 », pour un VLSM : un JSON), ce qui permet de
/// recalculer le résultat complet à l'ouverture sans stocker les résultats eux-mêmes.
/// </summary>
public sealed record HistoryEntry(
    Guid Id,
    HistoryKind Kind,
    string Title,
    string Summary,
    string Input,
    DateTime CreatedAt)
{
    public string KindLabel => Kind == HistoryKind.Vlsm ? "VLSM" : "Sous-réseau";
}

/// <summary>Données de saisie d'un calcul VLSM, sérialisées dans <see cref="HistoryEntry.Input"/>.</summary>
public sealed record VlsmInput(string Network, int Cidr, List<VlsmRequirement> Requirements);