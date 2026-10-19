using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubnetPro.Core.History;
using SubnetPro.Core.Models;

namespace SubnetPro.ViewModels;

/// <summary>Une carte de l'écran Historique : titre, résumé, ouverture du détail et suppression.</summary>
public partial class HistoryItemViewModel : ObservableObject
{
    private readonly Func<HistoryItemViewModel, Task> open;
    private readonly Func<HistoryItemViewModel, Task> delete;

    public HistoryItemViewModel(
        HistoryEntry entry,
        Func<HistoryItemViewModel, Task> open,
        Func<HistoryItemViewModel, Task> delete)
    {
        Entry = entry;
        this.open = open;
        this.delete = delete;
    }

    public HistoryEntry Entry { get; }

    public string Title => Entry.Title;

    public string Subtitle => HistoryGrouper.ShortLabel(Entry);

    public string Summary => Entry.Summary;

    [RelayCommand]
    private Task Open() => open(this);

    [RelayCommand]
    private Task Delete() => delete(this);
}

/// <summary>Un groupe de l'écran Historique (Aujourd'hui, Hier, date).</summary>
public sealed class HistoryGroupViewModel
{
    public HistoryGroupViewModel(string title, IReadOnlyList<HistoryItemViewModel> items)
    {
        Title = title;
        Items = items;
    }

    public string Title { get; }

    public IReadOnlyList<HistoryItemViewModel> Items { get; }
}