using SubnetPro.Core.Accounts;

namespace SubnetPro.Services;

/// <summary>Utilisateur actuellement connecté, mémorisé entre les lancements de l'application.</summary>
public interface ISessionService : IUserContext
{
    UserAccount? CurrentUser { get; }

    bool IsSignedIn { get; }

    /// <summary>Relit la session mémorisée. Renvoie true si un utilisateur est reconnecté.</summary>
    Task<bool> RestoreAsync();

    Task SignInAsync(UserAccount user);

    Task SignOutAsync();
}