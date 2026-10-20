using SubnetPro.Core.Data;

namespace SubnetPro.Core.Accounts;

public sealed class SqliteAccountRepository : IAccountRepository
{
    private readonly AppDatabase database;

    public SqliteAccountRepository(AppDatabase database)
    {
        this.database = database;
    }

    public async Task<UserAccount?> FindByIdentifierAsync(string identifier)
    {
        var key = AccountKeys.Normalize(identifier);
        var db = await database.GetConnectionAsync();

        var row = await db.Table<UserRow>().Where(u => u.EmailKey == key).FirstOrDefaultAsync();
        if (row is null)
            row = await db.Table<UserRow>().Where(u => u.NameKey == key).FirstOrDefaultAsync();

        return row is null ? null : ToAccount(row);
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id)
    {
        var db = await database.GetConnectionAsync();
        var row = await db.FindAsync<UserRow>(id.ToString());
        return row is null ? null : ToAccount(row);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var key = AccountKeys.Normalize(email);
        var db = await database.GetConnectionAsync();
        return await db.Table<UserRow>().Where(u => u.EmailKey == key).CountAsync() > 0;
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        var key = AccountKeys.Normalize(name);
        var db = await database.GetConnectionAsync();
        return await db.Table<UserRow>().Where(u => u.NameKey == key).CountAsync() > 0;
    }

    public async Task AddAsync(UserAccount account)
    {
        var db = await database.GetConnectionAsync();
        await db.InsertAsync(new UserRow
        {
            Id = account.Id.ToString(),
            EmailKey = AccountKeys.Normalize(account.Email),
            NameKey = AccountKeys.Normalize(account.Name),
            Name = account.Name,
            Email = account.Email,
            PasswordHash = account.PasswordHash,
            CreatedAt = account.CreatedAt
        });
    }

    private static UserAccount ToAccount(UserRow row) =>
        new(Guid.Parse(row.Id), row.Name, row.Email, row.PasswordHash, row.CreatedAt);
}