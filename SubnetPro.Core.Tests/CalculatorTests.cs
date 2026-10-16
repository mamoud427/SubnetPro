using SubnetPro.Core.Calculators;
using SubnetPro.Core.Models;
using Xunit;

namespace SubnetPro.Core.Tests;

public class SubnetCalculatorTests
{
    [Fact]
    public void Calculate_Slash26_MatchesFigmaDetailScreen()
    {
        var r = SubnetCalculator.Calculate("192.168.10.34", 26);

        Assert.Equal("255.255.255.192", r.Mask);
        Assert.Equal("0.0.0.63", r.Wildcard);
        Assert.Equal("192.168.10.0", r.Network);
        Assert.Equal("192.168.10.63", r.Broadcast);
        Assert.Equal("192.168.10.1", r.FirstHost);
        Assert.Equal("192.168.10.62", r.LastHost);
        Assert.Equal(64, r.TotalAddresses);
        Assert.Equal(62, r.UsableHosts);
        Assert.Equal("192.168.10.0/26", r.NetworkCidr);
    }

    [Theory]
    [InlineData(24, 254)]
    [InlineData(30, 2)]
    [InlineData(31, 2)]
    [InlineData(32, 1)]
    [InlineData(16, 65534)]
    public void Calculate_UsableHosts(int cidr, long expected) =>
        Assert.Equal(expected, SubnetCalculator.Calculate("10.0.0.0", cidr).UsableHosts);

    [Theory]
    [InlineData("256.1.1.1")]
    [InlineData("192.168.1")]
    [InlineData("a.b.c.d")]
    [InlineData("")]
    public void Calculate_InvalidAddress_Throws(string address) =>
        Assert.Throws<FormatException>(() => SubnetCalculator.Calculate(address, 24));

    [Theory]
    [InlineData(-1)]
    [InlineData(33)]
    public void Calculate_InvalidCidr_Throws(int cidr) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => SubnetCalculator.Calculate("10.0.0.0", cidr));

    [Fact]
    public void Split_Slash24IntoFourSlash26()
    {
        var parts = SubnetCalculator.Split("192.168.10.0", 24, 26);

        Assert.Equal(4, parts.Count);
        Assert.Equal(
            new[] { "192.168.10.0/26", "192.168.10.64/26", "192.168.10.128/26", "192.168.10.192/26" },
            parts.Select(p => p.NetworkCidr));
    }

    [Fact]
    public void SplitInto_FiveSubnets_RoundsUpToEight()
    {
        var parts = SubnetCalculator.SplitInto("192.168.10.0", 24, 5);
        Assert.Equal(8, parts.Count);
        Assert.All(parts, p => Assert.Equal(27, p.Cidr));
    }
}

public class VlsmCalculatorTests
{
    [Theory]
    [InlineData(1, 30)]
    [InlineData(2, 30)]
    [InlineData(10, 28)]
    [InlineData(25, 27)]
    [InlineData(50, 26)]
    [InlineData(62, 26)]
    [InlineData(63, 25)]
    public void PrefixForHosts(int hosts, int expected) =>
        Assert.Equal(expected, VlsmCalculator.PrefixForHosts(hosts));

    [Fact]
    public void Allocate_MatchesFigmaExample()
    {
        var result = VlsmCalculator.Allocate("192.168.10.0", 24, new[]
        {
            new VlsmRequirement("Salle TP", 25),
            new VlsmRequirement("Admin", 50),
            new VlsmRequirement("Serveurs", 10),
        });

        Assert.True(result.IsComplete);
        Assert.Equal(
            new[] { "192.168.10.0/26", "192.168.10.64/27", "192.168.10.96/28" },
            result.Allocations.Select(a => a.Subnet.NetworkCidr));
        Assert.Equal(new[] { "Admin", "Salle TP", "Serveurs" }, result.Allocations.Select(a => a.Name));
        Assert.Equal(256 - 64 - 32 - 16, result.AddressesRemaining);
    }

    [Fact]
    public void Allocate_WhenNetworkTooSmall_ReportsUnallocated()
    {
        var result = VlsmCalculator.Allocate("192.168.10.0", 24, new[]
        {
            new VlsmRequirement("A", 200),
            new VlsmRequirement("B", 100),
        });

        Assert.False(result.IsComplete);
        Assert.Single(result.Allocations);
        Assert.Equal("B", Assert.Single(result.Unallocated).Name);
    }

    [Fact]
    public void Allocate_AllocationsNeverOverlap()
    {
        var result = VlsmCalculator.Allocate("10.0.0.0", 16, new[]
        {
            new VlsmRequirement("A", 500), new VlsmRequirement("B", 120),
            new VlsmRequirement("C", 60), new VlsmRequirement("D", 2),
        });

        var ranges = result.Allocations
            .Select(a => (Start: IPv4.Parse(a.Subnet.Network), End: IPv4.Parse(a.Subnet.Broadcast)))
            .OrderBy(r => r.Start).ToList();

        for (var i = 1; i < ranges.Count; i++)
            Assert.True(ranges[i].Start > ranges[i - 1].End);
    }
}

public class IpClassifierTests
{
    [Theory]
    [InlineData("10.0.0.1", "A", true)]
    [InlineData("8.8.8.8", "A", false)]
    [InlineData("172.16.5.4", "B", true)]
    [InlineData("172.32.0.1", "B", false)]
    [InlineData("192.168.10.34", "C", true)]
    [InlineData("224.0.0.5", "D", false)]
    [InlineData("240.0.0.1", "E", false)]
    [InlineData("127.0.0.1", "Spéciale", false)]
    public void Classify(string ip, string letter, bool isPrivate)
    {
        var info = IpClassifier.Classify(ip);
        Assert.Equal(letter, info.Letter);
        Assert.Equal(isPrivate, info.IsPrivate);
    }
}

public class BaseConverterTests
{
    [Theory]
    [InlineData(192, "11000000")]
    [InlineData(0, "00000000")]
    [InlineData(255, "11111111")]
    public void DecimalToBinary(int value, string expected) =>
        Assert.Equal(expected, BaseConverter.DecimalToBinary(value));

    [Theory]
    [InlineData("11000000", 192)]
    [InlineData("101", 5)]
    public void BinaryToDecimal(string bits, int expected) =>
        Assert.Equal(expected, BaseConverter.BinaryToDecimal(bits));

    [Fact]
    public void InvalidInputs_AreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BaseConverter.DecimalToBinary(256));
        Assert.Throws<FormatException>(() => BaseConverter.BinaryToDecimal("102"));
        Assert.Throws<FormatException>(() => BaseConverter.BinaryToDecimal("111111111"));
        Assert.False(BaseConverter.TryDecimalToBinary("abc", out _));
    }
}