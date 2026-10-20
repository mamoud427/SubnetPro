namespace SubnetPro.Core.Accounts;

/// <summary>Compte local. Le mot de passe n'est jamais stocké : seul son hachage PBKDF2 l'est.</summary>
public sealed record UserAccount(Guid Id, string Name, string Email, string PasswordHash, DateTime CreatedAt);

/// <summary>Donne l'utilisateur connecté (null si personne). Sert à séparer l'historique de chaque compte.</summary>
public interface IUserContext
{
    string? CurrentUserId { get; }
}

public sealed record RegistrationRequest(
    string Name, string Email, string Password, string ConfirmPassword, bool AcceptedTerms);

/// <summary>Un message d'erreur (ou null) par champ du formulaire d'inscription.</summary>
public sealed record RegistrationErrors(
    string? Name, string? Email, string? Password, string? Confirmation, string? Terms)
{
    public static RegistrationErrors None { get; } = new(null, null, null, null, null);

    public bool HasErrors =>
        Name is not null || Email is not null || Password is not null
        || Confirmation is not null || Terms is not null;
}

public sealed record RegistrationResult(UserAccount? User, RegistrationErrors Errors)
{
    public bool Success => User is not null;
}

public sealed record LoginResult(UserAccount? User, string? Error)
{
    public bool Success => User is not null;
}