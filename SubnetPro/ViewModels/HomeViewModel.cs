using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubnetPro.Core.History;
using SubnetPro.Core.Models;
using SubnetPro.Helpers;

namespace SubnetPro.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IHistoryRepository repository;
    private HistoryEntry? lastEntry;

    public HomeViewModel(IHistoryRepository repository)
    {
        this.repository = repository;
    }

    // Le nom viendra de la session au jour 6.
    [ObservableProperty]
    private string userName = "Mamoud";

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

    private static Task GoToTab(string route) => Shell.Current.GoToAsync($"//{route}");
}