using Microsoft.Maui.Storage;
using SubnetPro.Core.Accounts;

namespace SubnetPro.Services;

/// <summary>
/// Mémorise l'identifiant de l'utilisateur connecté dans <see cref="SecureStorage"/> (stockage chiffré d'Android).
/// Seul l'identifiant est conservé : ni le mot de passe, ni son hachage.
/// </summary>
public sealed class SessionService : ISessionService
{
    private const string UserIdKey = "subnetpro.session.userid";

    private readonly IAccountRepository accounts;

    public SessionService(IAccountRepository accounts)
    {
        this.accounts = accounts;
    }

    public UserAccount? CurrentUser { get; private set; }

    public bool IsSignedIn => CurrentUser is not null;

    public string? CurrentUserId => CurrentUser?.Id.ToString();

    public async Task<bool> RestoreAsync()
    {
        try
        {
            var stored = await SecureStorage.Default.GetAsync(UserIdKey);
            if (Guid.TryParse(stored, out var id))
                CurrentUser = await accounts.GetByIdAsync(id);
        }
        catch (Exception)
        {
            // Le stockage sécurisé peut être illisible (clé Android invalidée, par exemple) :
            // on repart d'une session vide, l'utilisateur se reconnectera.
            CurrentUser = null;
            try { SecureStorage.Default.RemoveAll(); } catch (Exception) { }
        }

        return CurrentUser is not null;
    }

    public async Task SignInAsync(UserAccount user)
    {
        CurrentUser = user;

        try
        {
            await SecureStorage.Default.SetAsync(UserIdKey, user.Id.ToString());
        }
        catch (Exception)
        {
            // Session non mémorisée : l'utilisateur reste connecté jusqu'à la fermeture de l'application.
        }
    }

    public Task SignOutAsync()
    {
        CurrentUser = null;

        try { SecureStorage.Default.Remove(UserIdKey); }
        catch (Exception) { }

        return Task.CompletedTask;
    }
}