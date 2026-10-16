namespace SubnetPro.Core.Models;

/// <summary>Classe IPv4 d'une adresse (A à E) ou plage spéciale.</summary>
/// <param name="Letter">"A", "B", "C", "D", "E" ou "Spéciale".</param>
/// <param name="Usage">Unicast, Multicast, Réservé, Loopback...</param>
/// <param name="IsPrivate">Adresse privée RFC 1918.</param>
/// <param name="PrivateRange">Plage privée correspondante, null si publique.</param>
public sealed record IpClassInfo(string Letter, string Usage, bool IsPrivate, string? PrivateRange);

public sealed record SubnetResult(
    string Address,
    int Cidr,
    string Mask,
    string Wildcard,
    string Network,
    string Broadcast,
    string FirstHost,
    string LastHost,
    long TotalAddresses,
    long UsableHosts,
    IpClassInfo Class)
{
    /// <summary>Notation « réseau/préfixe », ex. 192.168.10.0/26.</summary>
    public string NetworkCidr => $"{Network}/{Cidr}";
}

public sealed record VlsmRequirement(string Name, int Hosts);

public sealed record VlsmAllocation(string Name, int RequestedHosts, SubnetResult Subnet);

public sealed record VlsmResult(
    IReadOnlyList<VlsmAllocation> Allocations,
    IReadOnlyList<VlsmRequirement> Unallocated,
    long AddressesRemaining)
{
    public bool IsComplete => Unallocated.Count == 0;
}