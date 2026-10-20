using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubnetPro.Core.Accounts;
using SubnetPro.Services;
using static Java.Util.Jar.Attributes;

namespace SubnetPro.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly AuthService auth;
    private readonly ISessionService session;

    public RegisterViewModel(AuthService auth, ISessionService session)
    {
        this.auth = auth;
        this.session = session;
    }

    // ----- Saisie -----

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private bool acceptedTerms;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordToggleIcon))]
    private bool isPasswordHidden = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConfirmToggleIcon))]
    private bool isConfirmHidden = true;

    public string PasswordToggleIcon => IsPasswordHidden ? "ic_eye.png" : "ic_eye_off.png";

    public string ConfirmToggleIcon => IsConfirmHidden ? "ic_eye.png" : "ic_eye_off.png";

    // ----- Erreurs, une par champ -----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNameError))]
    private string? nameError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEmailError))]
    private string? emailError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordError))]
    private string? passwordError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConfirmError))]
    private string? confirmError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTermsError))]
    private string? termsError;

    public bool HasNameError => !string.IsNullOrEmpty(NameError);

    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);

    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);

    public bool HasConfirmError => !string.IsNullOrEmpty(ConfirmError);

    public bool HasTermsError => !string.IsNullOrEmpty(TermsError);

    // L'erreur d'un champ disparaît dès que l'utilisateur le modifie.
    partial void OnNameChanged(string value) => NameError = null;

    partial void OnEmailChanged(string value) => EmailError = null;

    partial void OnPasswordChanged(string value) => PasswordError = null;

    partial void OnConfirmPasswordChanged(string value) => ConfirmError = null;

    partial void OnAcceptedTermsChanged(bool value) => TermsError = null;

    // ----- Commandes -----

    [RelayCommand]
    private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private void ToggleConfirm() => IsConfirmHidden = !IsConfirmHidden;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        IsBusy = true;

        try
        {
            var request = new RegistrationRequest(Name, Email, Password, ConfirmPassword, AcceptedTerms);

            // Le hachage PBKDF2 est volontairement lent : on le sort du fil de l'interface.
            var result = await Task.Run(() => auth.RegisterAsync(request));

            if (!result.Success)
            {
                NameError = result.Errors.Name;
                EmailError = result.Errors.Email;
                PasswordError = result.Errors.Password;
                ConfirmError = result.Errors.Confirmation;
                TermsError = result.Errors.Terms;
                return;
            }

            // Le compte est créé : l'utilisateur est directement connecté.
            await session.SignInAsync(result.User!);
            ClearForm();
            await Shell.Current.GoToAsync("//home");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task ShowTerms() =>
        Shell.Current.DisplayAlert(
            "Conditions générales d'utilisation",
            "SubnetPro est un outil pédagogique de calcul d'adressage IPv4 qui fonctionne hors ligne. " +
            "Votre compte et votre historique restent sur cet appareil et ne sont jamais envoyés sur internet. " +
            "Les résultats sont fournis à titre indicatif.",
            "OK");

    [RelayCommand]
    private Task GoToLogin() => Shell.Current.GoToAsync("//login");

    private void ClearForm()
    {
        Name = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        AcceptedTerms = false;
        IsPasswordHidden = true;
        IsConfirmHidden = true;
    }
}