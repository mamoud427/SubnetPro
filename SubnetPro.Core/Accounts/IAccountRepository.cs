namespace SubnetPro.Core.Accounts;

public interface IAccountRepository
{
    /// <summary>Retrouve un compte par email ou par nom (sans tenir compte des majuscules).</summary>
    Task<UserAccount?> FindByIdentifierAsync(string identifier);

    Task<UserAccount?> GetByIdAsync(Guid id);

    Task<bool> EmailExistsAsync(string email);

    Task<bool> NameExistsAsync(string name);

    Task AddAsync(UserAccount account);
}

/// <summary>Version en mémoire, pour les tests du service d'authentification.</summary>
public sealed class InMemoryAccountRepository : IAccountRepository
{
    private readonly object gate = new();
    private readonly List<UserAccount> accounts = new();

    public Task<UserAccount?> FindByIdentifierAsync(string identifier)
    {
        var key = AccountKeys.Normalize(identifier);
        lock (gate)
        {
            var found = accounts.FirstOrDefault(a => AccountKeys.Normalize(a.Email) == key)
                        ?? accounts.FirstOrDefault(a => AccountKeys.Normalize(a.Name) == key);
            return Task.FromResult(found);
        }
    }

    public Task<UserAccount?> GetByIdAsync(Guid id)
    {
        lock (gate)
            return Task.FromResult(accounts.FirstOrDefault(a => a.Id == id));
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        var key = AccountKeys.Normalize(email);
        lock (gate)
            return Task.FromResult(accounts.Any(a => AccountKeys.Normalize(a.Email) == key));
    }

    public Task<bool> NameExistsAsync(string name)
    {
        var key = AccountKeys.Normalize(name);
        lock (gate)
            return Task.FromResult(accounts.Any(a => AccountKeys.Normalize(a.Name) == key));
    }

    public Task AddAsync(UserAccount account)
    {
        lock (gate)
            accounts.Add(account);
        return Task.CompletedTask;
    }
}