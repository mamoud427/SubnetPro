using SubnetPro.Core.Calculators;
using SubnetPro.Core.History;
using SubnetPro.Core.Models;
using Xunit;

namespace SubnetPro.Core.Tests;

public class HistoryFactoryTests
{
    private static readonly DateTime Now = new(2026, 10, 18, 9, 32, 0);

    [Fact]
    public void FromSubnet_MatchesFigmaHistoryCard()
    {
        var entry = HistoryFactory.FromSubnet(SubnetCalculator.Calculate("192.168.10.34", 26), Now);

        Assert.Equal(HistoryKind.Subnet, entry.Kind);
        Assert.Equal("192.168.10.0/26", entry.Title);
        Assert.Equal("62 hôtes · IP saisie : 192.168.10.34", entry.Summary);
        Assert.Equal("192.168.10.34/26", entry.Input);
        Assert.Equal("Sous-réseau", entry.KindLabel);
    }

    [Fact]
    public void FromVlsm_MatchesFigmaHistoryCard()
    {
        var requirements = new[]
        {
            new VlsmRequirement("Admin", 50),
            new VlsmRequirement("Salle TP", 25),
            new VlsmRequirement("Serveurs", 10),
        };

        var entry = HistoryFactory.FromVlsm("192.168.10.0", 24, requirements, Now);

        Assert.Equal(HistoryKind.Vlsm, entry.Kind);
        Assert.Equal("192.168.10.0/24", entry.Title);
        Assert.Equal("3 segments · Besoins : 50, 25 et 10 hôtes", entry.Summary);
        Assert.Equal("VLSM", entry.KindLabel);
    }

    [Theory]
    [InlineData(1, 10, "1 segment · Besoins : 10 hôtes")]
    [InlineData(1, 1, "1 segment · Besoins : 1 hôte")]
    public void FromVlsm_SingleRequirement_UsesSingular(int count, int hosts, string expected)
    {
        var requirements = Enumerable.Range(0, count).Select(i => new VlsmRequirement($"S{i}", hosts)).ToList();

        Assert.Equal(expected, HistoryFactory.FromVlsm("10.0.0.0", 24, requirements, Now).Summary);
    }

    [Fact]
    public void FromVlsm_NormalizesNetworkAddress()
    {
        var entry = HistoryFactory.FromVlsm("192.168.10.77", 24, new[] { new VlsmRequirement("A", 10) }, Now);

        Assert.Equal("192.168.10.0/24", entry.Title);
    }
}

public class HistoryReaderTests
{
    [Fact]
    public void Subnet_RoundTrip_RebuildsTheSameResult()
    {
        var original = SubnetCalculator.Calculate("192.168.10.34", 26);
        var entry = HistoryFactory.FromSubnet(original, DateTime.Now);

        Assert.Equal(original, HistoryReader.ToSubnetResult(entry));
    }

    [Fact]
    public void Vlsm_RoundTrip_RebuildsTheSameAllocations()
    {
        var requirements = new List<VlsmRequirement>
        {
            new("Salle TP", 25), new("Admin", 50), new("Serveurs", 10),
        };
        var expected = VlsmCalculator.Allocate("192.168.10.0", 24, requirements);

        var entry = HistoryFactory.FromVlsm("192.168.10.0", 24, requirements, DateTime.Now);
        var rebuilt = HistoryReader.ToVlsmResult(entry);

        Assert.Equal(
            expected.Allocations.Select(a => (a.Name, a.Subnet.NetworkCidr)),
            rebuilt.Allocations.Select(a => (a.Name, a.Subnet.NetworkCidr)));
        Assert.Equal(expected.AddressesRemaining, rebuilt.AddressesRemaining);
    }

    [Fact]
    public void InvalidSubnetInput_Throws()
    {
        var entry = new HistoryEntry(Guid.NewGuid(), HistoryKind.Subnet, "x", "x", "pas-une-adresse", DateTime.Now);

        Assert.Throws<FormatException>(() => HistoryReader.ToSubnetResult(entry));
    }
}

public class HistoryGrouperTests
{
    private static readonly DateTime Now = new(2026, 10, 18, 14, 0, 0);

    private static HistoryEntry Make(string title, HistoryKind kind, DateTime at, string summary = "") =>
        new(Guid.NewGuid(), kind, title, summary, "", at);

    [Fact]
    public void DayLabel_TodayYesterdayAndOlder()
    {
        Assert.Equal("Aujourd'hui", HistoryGrouper.DayLabel(Now.AddHours(-5), Now));
        Assert.Equal("Hier", HistoryGrouper.DayLabel(Now.AddDays(-1), Now));
        Assert.Equal("13 octobre 2026", HistoryGrouper.DayLabel(Now.AddDays(-5), Now));
    }

    [Fact]
    public void Labels_MatchFigmaWording()
    {
        var entry = Make("192.168.10.0/26", HistoryKind.Subnet, new DateTime(2026, 10, 18, 9, 32, 0));

        Assert.Equal("Sous-réseau · 09:32", HistoryGrouper.ShortLabel(entry));
        Assert.Equal("Sous-réseau · Aujourd'hui, 09:32", HistoryGrouper.DetailLabel(entry, Now));
    }

    [Fact]
    public void Group_OrdersGroupsAndEntriesFromNewestToOldest()
    {
        var entries = new[]
        {
            Make("hier", HistoryKind.Subnet, Now.AddDays(-1)),
            Make("matin", HistoryKind.Subnet, Now.AddHours(-5)),
            Make("soir", HistoryKind.Vlsm, Now.AddHours(-1)),
            Make("ancien", HistoryKind.Subnet, Now.AddDays(-5)),
        };

        var groups = HistoryGrouper.Group(entries, Now);

        Assert.Equal(new[] { "Aujourd'hui", "Hier", "13 octobre 2026" }, groups.Select(g => g.Title));
        Assert.Equal(new[] { "soir", "matin" }, groups[0].Items.Select(e => e.Title));
    }

    [Fact]
    public void Filter_ByKind()
    {
        var entries = new[]
        {
            Make("a", HistoryKind.Subnet, Now), Make("b", HistoryKind.Vlsm, Now.AddMinutes(-1)),
        };

        Assert.Equal("b", Assert.Single(HistoryGrouper.Filter(entries, null, HistoryKind.Vlsm)).Title);
        Assert.Equal(2, HistoryGrouper.Filter(entries, null, null).Count);
    }

    [Theory]
    [InlineData("10.0.0", 1)]
    [InlineData("192.168.10.34", 1)]
    [InlineData("vlsm", 1)]
    [InlineData("SOUS", 1)]
    [InlineData("zzz", 0)]
    [InlineData("  ", 2)]
    public void Filter_BySearchText_IsCaseInsensitive(string query, int expectedCount)
    {
        var entries = new[]
        {
            Make("192.168.10.0/26", HistoryKind.Subnet, Now, "62 hôtes · IP saisie : 192.168.10.34"),
            Make("10.0.0.0/24", HistoryKind.Vlsm, Now.AddMinutes(-1), "3 segments"),
        };

        Assert.Equal(expectedCount, HistoryGrouper.Filter(entries, query, null).Count);
    }
}

public class InMemoryHistoryRepositoryTests
{
    private static HistoryEntry Make(DateTime at) =>
        new(Guid.NewGuid(), HistoryKind.Subnet, "t", "s", "10.0.0.1/24", at);

    [Fact]
    public async Task AddAndGetAll_ReturnsNewestFirst()
    {
        var repo = new InMemoryHistoryRepository();
        var old = Make(new DateTime(2026, 10, 17, 10, 0, 0));
        var recent = Make(new DateTime(2026, 10, 18, 10, 0, 0));

        await repo.AddAsync(old);
        await repo.AddAsync(recent);

        Assert.Equal(new[] { recent.Id, old.Id }, (await repo.GetAllAsync()).Select(e => e.Id));
    }

    [Fact]
    public async Task Add_SameIdTwice_KeepsASingleEntry()
    {
        var repo = new InMemoryHistoryRepository();
        var entry = Make(DateTime.Now);

        await repo.AddAsync(entry);
        await repo.AddAsync(entry);

        Assert.Single(await repo.GetAllAsync());
    }

    [Fact]
    public async Task Delete_RemovesTheEntry()
    {
        var repo = new InMemoryHistoryRepository();
        var entry = Make(DateTime.Now);
        await repo.AddAsync(entry);

        await repo.DeleteAsync(entry.Id);

        Assert.Null(await repo.GetAsync(entry.Id));
        Assert.Empty(await repo.GetAllAsync());
    }
}