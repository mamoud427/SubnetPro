using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliJ.Lang.Annotations;
using SubnetPro.Core.Accounts;
using SubnetPro.Services;

namespace SubnetPro.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService auth;
    private readonly ISessionService session;

    public LoginViewModel(AuthService auth, ISessionService session)
    {
        this.auth = auth;
        this.session = session;
    }

    [ObservableProperty]
    private string identifier = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordToggleIcon))]
    private bool isPasswordHidden = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string PasswordToggleIcon => IsPasswordHidden ? "ic_eye.png" : "ic_eye_off.png";

    partial void OnIdentifierChanged(string value) => ErrorMessage = null;

    partial void OnPasswordChanged(string value) => ErrorMessage = null;

    [RelayCommand]
    private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;

        try
        {
            var id = Identifier;
            var pw = Password;

            // Le hachage PBKDF2 est volontairement lent : on le sort du fil de l'interface.
            var result = await Task.Run(() => auth.LoginAsync(id, pw));

            if (!result.Success)
            {
                ErrorMessage = result.Error;
                return;
            }

            await session.SignInAsync(result.User!);

            Password = string.Empty;
            IsPasswordHidden = true;

            await Shell.Current.GoToAsync("//home");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task ForgotPasswordAsync() =>
        Shell.Current.DisplayAlert(
            "Mot de passe oublié",
            "Les comptes sont enregistrés uniquement sur cet appareil et SubnetPro fonctionne hors ligne : " +
            "le mot de passe ne peut pas être réinitialisé par e-mail. " +
            "Vous pouvez créer un nouveau compte avec un autre nom et une autre adresse.",
            "OK");

    [RelayCommand]
    private Task GoToRegister() => Shell.Current.GoToAsync("//register");
}