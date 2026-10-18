using SubnetPro.Core.Calculators;
using SubnetPro.Core.Formatting;
using Xunit;

namespace SubnetPro.Core.Tests;

public class ResultFormatterTests
{
    [Fact]
    public void ToItems_Slash26_MatchesFigmaDetailScreen()
    {
        var r = SubnetCalculator.Calculate("192.168.10.34", 26);
        var items = ResultFormatter.ToItems(r).ToDictionary(i => i.Caption, i => i.Value);

        Assert.Equal("192.168.10.34/26", items["Adresse saisie"]);
        Assert.Equal("255.255.255.192", items["Masque"]);
        Assert.Equal("26 / 6", items["Bits réseau / hôte"]);
        Assert.Equal("192.168.10.0", items["Réseau"]);
        Assert.Equal("192.168.10.63", items["Broadcast"]);
        Assert.Equal("192.168.10.1", items["Premier hôte"]);
        Assert.Equal("192.168.10.62", items["Dernier hôte"]);
        Assert.Equal("64", items["Adresses totales"]);
        Assert.Equal("62", items["Hôtes utilisables"]);
    }

    [Theory]
    [InlineData("192.168.10.34", "Classe C · Adresse privée")]
    [InlineData("8.8.8.8", "Classe A · Unicast")]
    [InlineData("224.0.0.5", "Classe D · Multicast")]
    [InlineData("127.0.0.1", "Plage spéciale · Loopback")]
    public void ClassLabel(string ip, string expected) =>
        Assert.Equal(expected, ResultFormatter.ClassLabel(IpClassifier.Classify(ip)));

    [Fact]
    public void FormatCount_UsesNonBreakingSpaceForThousands() =>
        Assert.Equal("65 536", ResultFormatter.FormatCount(65536));

    [Fact]
    public void ToClipboardText_ContainsKeyLines()
    {
        var text = ResultFormatter.ToClipboardText(SubnetCalculator.Calculate("192.168.10.34", 26));

        Assert.StartsWith("Sous-réseau 192.168.10.0/26", text);
        Assert.Contains("Masque : 255.255.255.192", text);
        Assert.Contains("Broadcast : 192.168.10.63", text);
    }
}