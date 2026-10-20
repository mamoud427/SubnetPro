using SubnetPro.Core.Accounts;
using SubnetPro.Core.Calculators;
using SubnetPro.Core.Data;
using SubnetPro.Core.History;
using SubnetPro.Core.Models;
using Xunit;

namespace SubnetPro.Core.Tests;

/// <summary>Utilisateur connecté simulé : on peut en changer pour tester la séparation des historiques.</summary>
public sealed class FixedUserContext : IUserContext
{
    public FixedUserContext(string? userId) => CurrentUserId = userId;

    public string? CurrentUserId { get; set; }
}

/// <summary>Teste l'historique sur un vrai fichier SQLite temporaire, supprimé à la fin de chaque test.</summary>
public class SqliteHistoryRepositoryTests : IAsyncLifetime
{
    private readonly string path =
        Path.Combine(Path.GetTempPath(), $"subnetpro_test_{Guid.NewGuid():N}.db3");

    private readonly List<AppDatabase> opened = new();

    private AppDatabase NewDatabase()
    {
        var db = new AppDatabase(path);
        opened.Add(db);
        return db;
    }

    private static HistoryEntry Make(DateTime at, string input = "10.0.0.1/24") =>
        new(Guid.NewGuid(), HistoryKind.Subnet, "10.0.0.0/24", "254 hôtes · IP saisie : 10.0.0.1", input, at);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var db in opened)
            await db.DisposeAsync();

        try { File.Delete(path); }
        catch (IOException) { /* fichier temporaire : sans importance */ }
    }

    [Fact]
    public async Task AddAndGetAll_ReturnsNewestFirst()
    {
        var repo = new SqliteHistoryRepository(NewDatabase(), new FixedUserContext("user-1"));
        var old = Make(new DateTime(2026, 10, 17, 10, 0, 0));
        var recent = Make(new DateTime(2026, 10, 18, 10, 0, 0));

        await repo.AddAsync(old);
        await repo.AddAsync(recent);

        Assert.Equal(new[] { recent.Id, old.Id }, (await repo.GetAllAsync()).Select(e => e.Id));
    }

    [Fact]
    public async Task Add_SameIdTwice_KeepsASingleEntry()
    {
        var repo = new SqliteHistoryRepository(NewDatabase(), new FixedUserContext("user-1"));
        var entry = Make(new DateTime(2026, 10, 18, 9, 32, 0));

        await repo.AddAsync(entry);
        await repo.AddAsync(entry);

        Assert.Single(await repo.GetAllAsync());
    }

    [Fact]
    public async Task Delete_RemovesTheEntry()
    {
        var repo = new SqliteHistoryRepository(NewDatabase(), new FixedUserContext("user-1"));
        var entry = Make(new DateTime(2026, 10, 18, 9, 32, 0));
        await repo.AddAsync(entry);

        await repo.DeleteAsync(entry.Id);

        Assert.Null(await repo.GetAsync(entry.Id));
        Assert.Empty(await repo.GetAllAsync());
    }

    [Fact]
    public async Task Entries_SurviveAnApplicationRestart()
    {
        var entry = Make(new DateTime(2026, 10, 18, 9, 32, 0), "192.168.10.34/26");

        var firstRun = NewDatabase();
        await new SqliteHistoryRepository(firstRun, new FixedUserContext("user-1")).AddAsync(entry);
        await firstRun.DisposeAsync(); // l'application est fermée

        var secondRun = new SqliteHistoryRepository(NewDatabase(), new FixedUserContext("user-1")); // relancée
        var loaded = await secondRun.GetAsync(entry.Id);

        Assert.Equal(entry, loaded);
    }

    [Fact]
    public async Task VlsmEntry_CanBeRebuiltAfterReload()
    {
        var requirements = new List<VlsmRequirement> { new("Admin", 50), new("Salle TP", 25) };
        var entry = HistoryFactory.FromVlsm("192.168.10.0", 24, requirements, new DateTime(2026, 10, 18, 9, 18, 0));

        var firstRun = NewDatabase();
        await new SqliteHistoryRepository(firstRun, new FixedUserContext("user-1")).AddAsync(entry);
        await firstRun.DisposeAsync();

        var loaded = await new SqliteHistoryRepository(NewDatabase(), new FixedUserContext("user-1")).GetAsync(entry.Id);
        var result = HistoryReader.ToVlsmResult(loaded!);

        Assert.Equal(
            new[] { "192.168.10.0/26", "192.168.10.64/27" },
            result.Allocations.Select(a => a.Subnet.NetworkCidr));
    }

    [Fact]
    public async Task EachUser_OnlySeesTheirOwnHistory()
    {
        var db = NewDatabase();
        var context = new FixedUserContext("user-1");
        var repo = new SqliteHistoryRepository(db, context);
        var entry = Make(new DateTime(2026, 10, 18, 9, 32, 0));
        await repo.AddAsync(entry);

        context.CurrentUserId = "user-2"; // un autre compte se connecte sur le même appareil

        Assert.Empty(await repo.GetAllAsync());
        Assert.Null(await repo.GetAsync(entry.Id));
        await repo.DeleteAsync(entry.Id); // ne doit pas supprimer l'entrée de user-1

        context.CurrentUserId = "user-1";
        Assert.Equal(entry.Id, Assert.Single(await repo.GetAllAsync()).Id);
    }

    [Fact]
    public async Task WithoutSignedInUser_HistoryIsEmptyAndNothingIsSaved()
    {
        var context = new FixedUserContext(null);
        var repo = new SqliteHistoryRepository(NewDatabase(), context);

        await repo.AddAsync(Make(new DateTime(2026, 10, 18, 9, 32, 0)));

        Assert.Empty(await repo.GetAllAsync());

        context.CurrentUserId = "user-1";
        Assert.Empty(await repo.GetAllAsync());
    }
}