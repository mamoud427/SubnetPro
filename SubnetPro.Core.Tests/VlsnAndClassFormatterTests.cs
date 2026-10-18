using SubnetPro.Core.Calculators;
using SubnetPro.Core.Formatting;
using SubnetPro.Core.Models;
using Xunit;

namespace SubnetPro.Core.Tests;

public class VlsmFormatterTests
{
    private static VlsmResult FigmaExample() =>
        VlsmCalculator.Allocate("192.168.10.0", 24, new[]
        {
            new VlsmRequirement("Salle TP", 25),
            new VlsmRequirement("Admin", 50),
            new VlsmRequirement("Serveurs", 10),
        });

    [Fact]
    public void ToRows_MatchesFigmaVlsmScreen()
    {
        var rows = VlsmFormatter.ToRows(FigmaExample());

        Assert.Equal(new[] { "Admin", "Salle TP", "Serveurs" }, rows.Select(r => r.Name));
        Assert.Equal(
            new[] { "192.168.10.0/26", "192.168.10.64/27", "192.168.10.96/28" },
            rows.Select(r => r.NetworkCidr));
        Assert.Equal("Hôtes : 192.168.10.1 - 192.168.10.62", rows[0].HostRange);
        Assert.Equal(new[] { "62", "30", "14" }, rows.Select(r => r.UsableHosts));
    }

    [Fact]
    public void Summary_ReportsRemainingAddresses()
    {
        Assert.Equal(
            "Allocation du plus grand besoin au plus petit. 144 adresses restent non attribuées.",
            VlsmFormatter.Summary(FigmaExample()));
        Assert.Null(VlsmFormatter.UnallocatedWarning(FigmaExample()));
    }

    [Fact]
    public void Summary_WhenNothingRemains()
    {
        var result = VlsmCalculator.Allocate("10.0.0.0", 30, new[] { new VlsmRequirement("Lien", 1) });

        Assert.Equal(
            "Allocation du plus grand besoin au plus petit. Aucune adresse ne reste disponible.",
            VlsmFormatter.Summary(result));
    }

    [Fact]
    public void UnallocatedWarning_NamesTheMissingRequirement()
    {
        var result = VlsmCalculator.Allocate("192.168.10.0", 24, new[]
        {
            new VlsmRequirement("A", 200),
            new VlsmRequirement("B", 100),
        });

        var warning = VlsmFormatter.UnallocatedWarning(result);

        Assert.NotNull(warning);
        Assert.Contains("B (100 hôtes)", warning);
    }

    [Fact]
    public void ToClipboardText_ListsEachAllocation()
    {
        var text = VlsmFormatter.ToClipboardText(FigmaExample());

        Assert.StartsWith("Répartition VLSM", text);
        Assert.Contains("Admin : 192.168.10.0/26 (192.168.10.1 - 192.168.10.62, 62 hôtes)", text);
    }
}

public class ClassTableTests
{
    [Fact]
    public void Build_HighlightsOnlyTheCurrentClass()
    {
        var rows = ClassTable.Build("C");

        Assert.Equal(new[] { "A", "B", "C", "D", "E" }, rows.Select(r => r.Letter));
        Assert.Equal("C", Assert.Single(rows, r => r.IsCurrent).Letter);
    }

    [Fact]
    public void Build_WithoutClass_HighlightsNothing() =>
        Assert.DoesNotContain(ClassTable.Build(null), r => r.IsCurrent);
}