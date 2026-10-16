using SubnetPro.Core.Models;

namespace SubnetPro.Core.Calculators;

public static class IpClassifier
{
    public static IpClassInfo Classify(string address) => Classify(IPv4.Parse(address));

    public static IpClassInfo Classify(uint address)
    {
        var first = (int)(address >> 24);
        var second = (int)((address >> 16) & 255);

        string? privateRange = null;
        if (first == 10)
            privateRange = "10.0.0.0/8 (RFC 1918)";
        else if (first == 172 && second >= 16 && second <= 31)
            privateRange = "172.16.0.0/12 (RFC 1918)";
        else if (first == 192 && second == 168)
            privateRange = "192.168.0.0/16 (RFC 1918)";

        var isPrivate = privateRange is not null;

        if (first == 0) return new IpClassInfo("Spéciale", "Réseau par défaut", false, null);
        if (first == 127) return new IpClassInfo("Spéciale", "Loopback", false, null);
        if (first <= 126) return new IpClassInfo("A", "Unicast", isPrivate, privateRange);
        if (first <= 191) return new IpClassInfo("B", "Unicast", isPrivate, privateRange);
        if (first <= 223) return new IpClassInfo("C", "Unicast", isPrivate, privateRange);
        if (first <= 239) return new IpClassInfo("D", "Multicast", false, null);
        return new IpClassInfo("E", "Réservé", false, null);
    }
}