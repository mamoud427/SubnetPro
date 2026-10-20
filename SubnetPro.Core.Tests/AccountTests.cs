using SubnetPro.Core.Accounts;
using Xunit;

namespace SubnetPro.Core.Tests;

public class PasswordHasherTests
{
    // Peu d'itérations pour que les tests restent rapides (l'application utilise la valeur par défaut).
    private static readonly PasswordHasher Hasher = new(1000);

    [Fact]
    public void Hash_IsNotThePlainPassword_AndVerifies()
    {
        var hash = Hasher.Hash("MotDePasse123");

        Assert.DoesNotContain("MotDePasse123", hash);
        Assert.StartsWith("pbkdf2-sha256$1000$", hash);
        Assert.True(Hasher.Verify("MotDePasse123", hash));
    }

    [Fact]
    public void Hash_UsesARandomSaltEachTime()
    {
        Assert.NotEqual(Hasher.Hash("MotDePasse123"), Hasher.Hash("MotDePasse123"));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = Hasher.Hash("MotDePasse123");

        Assert.False(Hasher.Verify("motdepasse123", hash));
        Assert.False(Hasher.Verify("", hash));
    }

    [Fact]
    public void Verify_UsesTheIterationCountStoredInTheHash()
    {
        var hash = new PasswordHasher(1000).Hash("MotDePasse123");

        Assert.True(new PasswordHasher(5000).Verify("MotDePasse123", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("pbkdf2-sha256$x$y$z")]
    [InlineData("pbkdf2-sha256$1000$!!!$!!!")]
    [InlineData("autre$1000$AAAA$AAAA")]
    public void Verify_MalformedHash_ReturnsFalse(string stored) =>
        Assert.False(Hasher.Verify("MotDePasse123", stored));
}

public class AccountValidatorTests
{
    private static RegistrationRequest Valid() =>
        new("Mamoud", "mamoud@exemple.com", "MotDePasse123", "MotDePasse123", true);

    [Fact]
    public void ValidRequest_HasNoError() =>
        Assert.False(AccountValidator.ValidateRegistration(Valid()).HasErrors);

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("M")]
    public void InvalidName_IsRejected(string name) =>
        Assert.NotNull(AccountValidator.ValidateRegistration(Valid() with { Name = name }).Name);

    [Theory]
    [InlineData("")]
    [InlineData("pas-un-email")]
    [InlineData("a@b")]
    [InlineData("a b@c.com")]
    public void InvalidEmail_IsRejected(string email) =>
        Assert.NotNull(AccountValidator.ValidateRegistration(Valid() with { Email = email }).Email);

    [Fact]
    public void ShortPassword_IsRejected()
    {
        var errors = AccountValidator.ValidateRegistration(Valid() with { Password = "1234567", ConfirmPassword = "1234567" });

        Assert.Equal("Le mot de passe doit contenir au moins 8 caractères.", errors.Password);
    }

    [Fact]
    public void PasswordsThatDiffer_AreRejected()
    {
        var errors = AccountValidator.ValidateRegistration(Valid() with { ConfirmPassword = "Autre12345" });

        Assert.Equal("Les mots de passe ne correspondent pas.", errors.Confirmation);
    }

    [Fact]
    public void RefusedTerms_AreRejected() =>
        Assert.NotNull(AccountValidator.ValidateRegistration(Valid() with { AcceptedTerms = false }).Terms);
}

public class AuthServiceTests
{
    private static AuthService NewService() => new(new InMemoryAccountRepository(), new PasswordHasher(1000));

    private static RegistrationRequest Request(string name = "Mamoud", string email = "mamoud@exemple.com") =>
        new(name, email, "MotDePasse123", "MotDePasse123", true);

    [Fact]
    public async Task Register_Succeeds_AndStoresOnlyAHash()
    {
        var result = await NewService().RegisterAsync(Request());

        Assert.True(result.Success);
        Assert.Equal("Mamoud", result.User!.Name);
        Assert.DoesNotContain("MotDePasse123", result.User.PasswordHash);
        Assert.StartsWith("pbkdf2-sha256$", result.User.PasswordHash);
    }

    [Fact]
    public async Task Register_InvalidFields_ReturnsErrorsAndCreatesNothing()
    {
        var service = NewService();

        var result = await service.RegisterAsync(Request(email: "pas-un-email"));

        Assert.False(result.Success);
        Assert.NotNull(result.Errors.Email);
        Assert.False((await service.LoginAsync("Mamoud", "MotDePasse123")).Success);
    }

    [Fact]
    public async Task Register_DuplicateEmail_IsRejected_IgnoringCase()
    {
        var service = NewService();
        await service.RegisterAsync(Request());

        var result = await service.RegisterAsync(Request(name: "Autre", email: "MAMOUD@exemple.COM"));

        Assert.False(result.Success);
        Assert.Equal("Cette adresse email est déjà utilisée sur cet appareil.", result.Errors.Email);
    }

    [Fact]
    public async Task Register_DuplicateName_IsRejected()
    {
        var service = NewService();
        await service.RegisterAsync(Request());

        var result = await service.RegisterAsync(Request(name: "mamoud", email: "autre@exemple.com"));

        Assert.False(result.Success);
        Assert.Equal("Ce nom est déjà utilisé sur cet appareil.", result.Errors.Name);
    }

    [Theory]
    [InlineData("mamoud@exemple.com")]
    [InlineData("MAMOUD@EXEMPLE.COM")]
    [InlineData("Mamoud")]
    [InlineData("  mamoud  ")]
    public async Task Login_WithEmailOrName_Succeeds(string identifier)
    {
        var service = NewService();
        await service.RegisterAsync(Request());

        var result = await service.LoginAsync(identifier, "MotDePasse123");

        Assert.True(result.Success);
        Assert.Equal("Mamoud", result.User!.Name);
    }

    [Fact]
    public async Task Login_WrongPasswordAndUnknownUser_GiveTheSameMessage()
    {
        var service = NewService();
        await service.RegisterAsync(Request());

        var wrongPassword = await service.LoginAsync("Mamoud", "mauvais-mot-de-passe");
        var unknownUser = await service.LoginAsync("inconnu", "MotDePasse123");

        Assert.False(wrongPassword.Success);
        Assert.False(unknownUser.Success);
        Assert.Equal(AuthService.BadCredentialsMessage, wrongPassword.Error);
        Assert.Equal(AuthService.BadCredentialsMessage, unknownUser.Error);
    }

    [Theory]
    [InlineData("", "MotDePasse123")]
    [InlineData("Mamoud", "")]
    [InlineData(null, null)]
    public async Task Login_WithEmptyFields_AsksForBoth(string? identifier, string? password)
    {
        var result = await NewService().LoginAsync(identifier, password);

        Assert.False(result.Success);
        Assert.Equal("Saisissez votre identifiant et votre mot de passe.", result.Error);
    }
}