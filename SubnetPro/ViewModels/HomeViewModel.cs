using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubnetPro.Helpers;

namespace SubnetPro.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    // Valeurs d'aperçu : le nom viendra de la session (jour 6),
    // le dernier calcul viendra de l'historique (jour 4).
    [ObservableProperty]
    private string userName = "Mamoud";

    [ObservableProperty]
    private string lastCalculationTitle = "192.168.10.0/26";

    [ObservableProperty]
    private string lastCalculationInfo = "Sous-réseau · Aujourd'hui, 09:32";

    [RelayCommand]
    private Task OpenSubnet() => GoToTab(Routes.Subnet);

    [RelayCommand]
    private Task OpenVlsm() => GoToTab(Routes.Vlsm);

    [RelayCommand]
    private Task OpenClasse() => GoToTab(Routes.Classe);

    [RelayCommand]
    private Task OpenHistory() => GoToTab(Routes.History);

    private static Task GoToTab(string route) => Shell.Current.GoToAsync($"//{route}");
}