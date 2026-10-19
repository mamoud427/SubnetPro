using Android.Content;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubnetPro.Core.History;
using SubnetPro.Core.Models;
using SubnetPro.Helpers;

namespace SubnetPro.ViewModels;

public partial class HistoryViewModel : BaseViewModel
{
    private const string AllTools = "Tous les outils";

    private readonly IHistoryRepository repository;
    private IReadOnlyList<HistoryEntry> allEntries = Array.Empty<HistoryEntry>();

    public HistoryViewModel(IHistoryRepository repository)
    {
        this.repository = repository;
    }

    public IReadOnlyList<string> Filters { get; } = new[] { AllTools, "Sous-réseau", "VLSM" };

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedFilter = AllTools;

    [ObservableProperty]
    private IReadOnlyList<HistoryGroupViewModel> groups = Array.Empty<HistoryGroupViewModel>();

    [ObservableProperty]
    private string countText = string.Empty;

    [ObservableProperty]
    private bool isEmpty = true;

    [ObservableProperty]
    private string emptyMessage = string.Empty;

    partial void OnSearchTextChanged(string value) => Refresh();

    partial void OnSelectedFilterChanged(string value) => Refresh();

    /// <summary>Appelé à chaque affichage de la page (l'historique peut avoir changé depuis un autre onglet).</summary>
    public async Task LoadAsync()
    {
        allEntries = await repository.GetAllAsync();
        Refresh();
    }

    private HistoryKind? SelectedKind() => SelectedFilter switch
    {
        "Sous-réseau" => HistoryKind.Subnet,
        "VLSM" => HistoryKind.Vlsm,
        _ => null
    };

    private void Refresh()
    {
        var filtered = HistoryGrouper.Filter(allEntries, SearchText, SelectedKind());

        Groups = HistoryGrouper
            .Group(filtered, DateTime.Now)
            .Select(g => new HistoryGroupViewModel(
                g.Title,
                g.Items.Select(e => new HistoryItemViewModel(e, OpenAsync, DeleteAsync)).ToList()))
            .ToList();

        CountText = filtered.Count switch
        {
            0 => string.Empty,
            1 => "1 calcul",
            var n => $"{n} calculs"
        };

        IsEmpty = filtered.Count == 0;
        EmptyMessage = allEntries.Count == 0
            ? "Aucun calcul enregistré. Lancez un calcul, puis touchez « Enregistrer » dans l'écran de détail."
            : "Aucun résultat pour cette recherche.";
    }

    private Task OpenAsync(HistoryItemViewModel item) =>
        Shell.Current.GoToAsync(
            Routes.Result,
            new ShellNavigationQueryParameters { { "entry", item.Entry } });

    private async Task DeleteAsync(HistoryItemViewModel item)
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Supprimer ce calcul ?",
            $"{item.Title} sera retiré de l'historique.",
            "Supprimer",
            "Annuler");

        if (!confirmed)
            return;

        await repository.DeleteAsync(item.Entry.Id);
        await LoadAsync();
    }
}