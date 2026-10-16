using SubnetPro.Core.Models;

namespace SubnetPro.Core.Calculators;

public static class SubnetCalculator
{
    private const int MaxSplitCount = 65536;

    /// <summary>Calcule masque, réseau, broadcast, plage d'hôtes... pour une adresse et un préfixe.</summary>
    public static SubnetResult Calculate(string address, int cidr)
    {
        var ip = IPv4.Parse(address);
        var mask = IPv4.MaskFromCidr(cidr);

        var network = ip & mask;
        var broadcast = network | ~mask;
        var total = 1L << (32 - cidr);

        // /32 : un seul hôte. /31 : liaison point à point (RFC 3021). Sinon on retire réseau + broadcast.
        var usable = cidr switch
        {
            32 => 1L,
            31 => 2L,
            _ => total - 2
        };

        var first = cidr >= 31 ? network : network + 1;
        var last = cidr >= 31 ? broadcast : broadcast - 1;

        return new SubnetResult(
            Address: IPv4.ToText(ip),
            Cidr: cidr,
            Mask: IPv4.ToText(mask),
            Wildcard: IPv4.ToText(~mask),
            Network: IPv4.ToText(network),
            Broadcast: IPv4.ToText(broadcast),
            FirstHost: IPv4.ToText(first),
            LastHost: IPv4.ToText(last),
            TotalAddresses: total,
            UsableHosts: usable,
            Class: IpClassifier.Classify(ip));
    }

    /// <summary>Découpe un réseau en sous-réseaux de taille égale (nouveau préfixe plus long).</summary>
    public static IReadOnlyList<SubnetResult> Split(string network, int cidr, int newCidr)
    {
        if (newCidr <= cidr || newCidr > 32)
            throw new ArgumentOutOfRangeException(nameof(newCidr), "Le nouveau préfixe doit être supérieur au préfixe de départ et ne pas dépasser 32.");

        var count = 1L << (newCidr - cidr);
        if (count > MaxSplitCount)
            throw new ArgumentOutOfRangeException(nameof(newCidr), $"Découpage limité à {MaxSplitCount} sous-réseaux.");

        var baseNetwork = IPv4.Parse(network) & IPv4.MaskFromCidr(cidr);
        var step = 1L << (32 - newCidr);

        var result = new List<SubnetResult>((int)count);
        for (long i = 0; i < count; i++)
        {
            var start = (uint)(baseNetwork + i * step);
            result.Add(Calculate(IPv4.ToText(start), newCidr));
        }
        return result;
    }

    /// <summary>Découpe un réseau en au moins <paramref name="subnetCount"/> sous-réseaux égaux.</summary>
    public static IReadOnlyList<SubnetResult> SplitInto(string network, int cidr, int subnetCount)
    {
        if (subnetCount < 1)
            throw new ArgumentOutOfRangeException(nameof(subnetCount), "Il faut au moins un sous-réseau.");

        var extraBits = 0;
        while ((1L << extraBits) < subnetCount) extraBits++;

        return extraBits == 0
            ? new[] { Calculate(network, cidr) }
            : Split(network, cidr, cidr + extraBits);
    }
}