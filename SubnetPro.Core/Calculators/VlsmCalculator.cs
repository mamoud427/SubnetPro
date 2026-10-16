using SubnetPro.Core.Models;

namespace SubnetPro.Core.Calculators;

public static class VlsmCalculator
{
    /// <summary>Plus long préfixe permettant d'accueillir <paramref name="hosts"/> hôtes (+ réseau + broadcast).</summary>
    public static int PrefixForHosts(int hosts)
    {
        if (hosts < 1)
            throw new ArgumentOutOfRangeException(nameof(hosts), "Un besoin doit comporter au moins 1 hôte.");

        var hostBits = 2;
        while ((1L << hostBits) < (long)hosts + 2) hostBits++;

        return hostBits > 32 ? throw new ArgumentOutOfRangeException(nameof(hosts)) : 32 - hostBits;
    }

    /// <summary>
    /// Répartit les besoins dans le réseau de départ : plus grand besoin en premier,
    /// blocs placés les uns à la suite des autres (alignement garanti par le tri décroissant).
    /// </summary>
    public static VlsmResult Allocate(string network, int cidr, IEnumerable<VlsmRequirement> requirements)
    {
        var mask = IPv4.MaskFromCidr(cidr);
        var baseNetwork = IPv4.Parse(network) & mask;
        var end = (long)baseNetwork + (1L << (32 - cidr));

        var allocations = new List<VlsmAllocation>();
        var unallocated = new List<VlsmRequirement>();
        long cursor = baseNetwork;

        foreach (var req in requirements.OrderByDescending(r => r.Hosts))
        {
            var prefix = PrefixForHosts(req.Hosts);
            var size = 1L << (32 - prefix);

            if (prefix < cidr || cursor + size > end)
            {
                unallocated.Add(req);
                continue;
            }

            var subnet = SubnetCalculator.Calculate(IPv4.ToText((uint)cursor), prefix);
            allocations.Add(new VlsmAllocation(req.Name, req.Hosts, subnet));
            cursor += size;
        }

        return new VlsmResult(allocations, unallocated, end - cursor);
    }
}