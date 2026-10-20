namespace SubnetPro.Core.Accounts;

/// <summary>Inscription et connexion locales (aucun réseau : les comptes vivent dans la base de l'appareil).</summary>
public sealed class AuthService
{
    public const string BadCredentialsMessage = "Identifiant ou mot de passe incorrect.";

    private readonly IAccountRepository accounts;
    private readonly PasswordHasher hasher;

    public AuthService(IAccountRepository accounts, PasswordHasher hasher)
    {
        this.accounts = accounts;
        this.hasher = hasher;
    }

    public async Task<RegistrationResult> RegisterAsync(RegistrationRequest request)
    {
        var errors = AccountValidator.ValidateRegistration(request);

        var name = request.Name?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;

        if (errors.Email is null && await accounts.EmailExistsAsync(email))
            errors = errors with { Email = "Cette adresse email est déjà utilisée sur cet appareil." };

        if (errors.Name is null && await accounts.NameExistsAsync(name))
            errors = errors with { Name = "Ce nom est déjà utilisé sur cet appareil." };

        if (errors.HasErrors)
            return new RegistrationResult(null, errors);

        var account = new UserAccount(Guid.NewGuid(), name, email, hasher.Hash(request.Password), DateTime.Now);
        await accounts.AddAsync(account);

        return new RegistrationResult(account, RegistrationErrors.None);
    }

    public async Task<LoginResult> LoginAsync(string? identifier, string? password)
    {
        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrEmpty(password))
            return new LoginResult(null, "Saisissez votre identifiant et votre mot de passe.");

        var account = await accounts.FindByIdentifierAsync(identifier);

        // Même message que le compte existe ou non : on ne révèle pas quels identifiants sont enregistrés.
        if (account is null || !hasher.Verify(password, account.PasswordHash))
            return new LoginResult(null, BadCredentialsMessage);

        return new LoginResult(account, null);
    }
}