using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubnetPro.Core.History;
using SubnetPro.Core.Models;
using SubnetPro.Helpers;
using SubnetPro.Services;

namespace SubnetPro.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IHistoryRepository repository;
    private readonly ISessionService session;
    private HistoryEntry? lastEntry;

    public HomeViewModel(IHistoryRepository repository, ISessionService session)
    {
        this.repository = repository;
        this.session = session;
    }

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoLastCalculation))]
    private bool hasLastCalculation;

    public bool HasNoLastCalculation => !HasLastCalculation;

    [ObservableProperty]
    private string lastCalculationTitle = string.Empty;

    [ObservableProperty]
    private string lastCalculationInfo = string.Empty;

    /// <summary>Appelé à chaque affichage de l'accueil : le dernier calcul peut venir d'être enregistré.</summary>
    public async Task LoadAsync()
    {
        UserName = session.CurrentUser?.Name ?? string.Empty;

        var all = await repository.GetAllAsync();
        lastEntry = all.FirstOrDefault();

        HasLastCalculation = lastEntry is not null;
        LastCalculationTitle = lastEntry?.Title ?? string.Empty;
        LastCalculationInfo = lastEntry is null
            ? string.Empty
            : HistoryGrouper.DetailLabel(lastEntry, DateTime.Now);
    }

    [RelayCommand]
    private Task OpenSubnet() => GoToTab(Routes.Subnet);

    [RelayCommand]
    private Task OpenVlsm() => GoToTab(Routes.Vlsm);

    [RelayCommand]
    private Task OpenClasse() => GoToTab(Routes.Classe);

    [RelayCommand]
    private Task OpenHistory() => GoToTab(Routes.History);

    [RelayCommand]
    private Task OpenLast()
    {
        if (lastEntry is null)
            return Task.CompletedTask;

        return Shell.Current.GoToAsync(
            Routes.Result,
            new ShellNavigationQueryParameters { { "entry", lastEntry } });
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Déconnexion",
            "Voulez-vous vraiment vous déconnecter ? Vos données restent sur cet appareil.",
            "Se déconnecter",
            "Annuler");

        if (!confirmed)
            return;

        await session.SignOutAsync();

        // Nouveau Shell : les onglets déjà créés (et leurs données) ne sont pas conservés
        // pour le compte suivant. Il redémarre sur Splash, qui mène à la connexion.
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = new AppShell();
    }

    private static Task GoToTab(string route) => Shell.Current.GoToAsync($"//{route}");
}