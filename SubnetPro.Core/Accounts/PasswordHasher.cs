using System.Security.Cryptography;

namespace SubnetPro.Core.Accounts;

/// <summary>
/// Hachage des mots de passe avec PBKDF2-HMAC-SHA256 et un sel aléatoire par mot de passe.
/// Format stocké : <c>pbkdf2-sha256$itérations$sel$hachage</c> (sel et hachage en Base64).
/// Le nombre d'itérations est stocké avec le hachage : on pourra l'augmenter plus tard sans casser les comptes existants.
/// </summary>
public sealed class PasswordHasher
{
    private const string Prefix = "pbkdf2-sha256";
    private const int SaltSize = 16;
    private const int KeySize = 32;

    private readonly int iterations;

    public PasswordHasher(int iterations = 210_000)
    {
        if (iterations < 1)
            throw new ArgumentOutOfRangeException(nameof(iterations));

        this.iterations = iterations;
    }

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Prefix}${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string stored)
    {
        var parts = (stored ?? string.Empty).Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out var storedIterations) || storedIterations < 1)
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, storedIterations, HashAlgorithmName.SHA256, expected.Length);

            // Comparaison en temps constant.
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}