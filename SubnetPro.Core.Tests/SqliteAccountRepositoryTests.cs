using SubnetPro.Core.Accounts;
using SubnetPro.Core.Data;
using Xunit;

namespace SubnetPro.Core.Tests;

/// <summary>Teste les comptes sur un vrai fichier SQLite temporaire, supprimé à la fin de chaque test.</summary>
public class SqliteAccountRepositoryTests : IAsyncLifetime
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

    private static UserAccount Make() =>
        new(Guid.NewGuid(), "Mamoud", "Mamoud@Exemple.com", "pbkdf2-sha256$1000$AAAA$AAAA", new DateTime(2026, 10, 20, 8, 0, 0));

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var db in opened)
            await db.DisposeAsync();

        try { File.Delete(path); }
        catch (IOException) { /* fichier temporaire : sans importance */ }
    }

    [Fact]
    public async Task AddedAccount_CanBeFoundByEmailNameAndId_IgnoringCase()
    {
        var repo = new SqliteAccountRepository(NewDatabase());
        var account = Make();
        await repo.AddAsync(account);

        Assert.Equal(account, await repo.FindByIdentifierAsync("mamoud@exemple.com"));
        Assert.Equal(account, await repo.FindByIdentifierAsync("  MAMOUD  "));
        Assert.Equal(account, await repo.GetByIdAsync(account.Id));
        Assert.Null(await repo.FindByIdentifierAsync("inconnu"));
    }

    [Fact]
    public async Task ExistsChecks_IgnoreCase()
    {
        var repo = new SqliteAccountRepository(NewDatabase());
        await repo.AddAsync(Make());

        Assert.True(await repo.EmailExistsAsync("MAMOUD@exemple.com"));
        Assert.True(await repo.NameExistsAsync("mamoud"));
        Assert.False(await repo.EmailExistsAsync("autre@exemple.com"));
        Assert.False(await repo.NameExistsAsync("autre"));
    }

    [Fact]
    public async Task Accounts_SurviveAnApplicationRestart()
    {
        var account = Make();

        var firstRun = NewDatabase();
        await new SqliteAccountRepository(firstRun).AddAsync(account);
        await firstRun.DisposeAsync(); // l'application est fermée

        var loaded = await new SqliteAccountRepository(NewDatabase()).FindByIdentifierAsync("Mamoud");

        Assert.Equal(account, loaded);
    }
}