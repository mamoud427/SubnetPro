using System.Text.RegularExpressions;

namespace SubnetPro.Core.Accounts;

public static class AccountKeys
{
    /// <summary>Forme comparable d'un nom ou d'un email : sans espaces autour, en minuscules.</summary>
    public static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
}

public static class AccountValidator
{
    public const int MinPasswordLength = 8;

    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static RegistrationErrors ValidateRegistration(RegistrationRequest request)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        string? nameError = name.Length switch
        {
            0 => "Le nom est obligatoire.",
            < 2 => "Le nom doit contenir au moins 2 caractères.",
            > 50 => "Le nom ne peut pas dépasser 50 caractères.",
            _ => null
        };

        string? emailError = email.Length == 0
            ? "L'adresse email est obligatoire."
            : EmailPattern.IsMatch(email) ? null : "Adresse email invalide (ex. nom@exemple.com).";

        string? passwordError = password.Length < MinPasswordLength
            ? $"Le mot de passe doit contenir au moins {MinPasswordLength} caractères."
            : null;

        string? confirmationError = password == (request.ConfirmPassword ?? string.Empty)
            ? null
            : "Les mots de passe ne correspondent pas.";

        string? termsError = request.AcceptedTerms
            ? null
            : "Vous devez accepter les conditions générales d'utilisation.";

        return new RegistrationErrors(nameError, emailError, passwordError, confirmationError, termsError);
    }
}