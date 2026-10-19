using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using SubnetPro.Core.Formatting;
using SubnetPro.Core.History;
using SubnetPro.Core.Models;

namespace SubnetPro.ViewModels;

/// <summary>
/// Écran « Détail résultat ». Reçoit l'entrée par la navigation (clé « entry ») :
/// soit un calcul pas encore enregistré (depuis Sous-réseau ou VLSM), soit une entrée de l'historique.
/// </summary>
[QueryProperty(nameof(Entry), "entry")]
public partial class ResultDetailViewModel : BaseViewModel
{
    private readonly IHistoryRepository repository;
    private string exportText = string.Empty;

    public ResultDetailViewModel(IHistoryRepository repository)
    {
        this.repository = repository;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private HistoryEntry? entry;

    // ----- En-tête -----

    [ObservableProperty]
    private string resultTitle = string.Empty;

    [ObservableProperty]
    private string detailSubtitle = string.Empty;

    // ----- Sous-réseau -----

    [ObservableProperty]
    private bool isSubnet;

    [ObservableProperty]
    private string classLabel = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<ResultItem> resultItems = Array.Empty<ResultItem>();

    // ----- VLSM -----

    [ObservableProperty]
    private bool isVlsm;

    [ObservableProperty]
    private IReadOnlyList<VlsmRow> vlsmRows = Array.Empty<VlsmRow>();

    [ObservableProperty]
    private string vlsmSummary = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasWarning))]
    private string? warning;

    public bool HasWarning => !string.IsNullOrEmpty(Warning);

    // ----- Enregistrement -----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isSaved;

    public string SaveButtonText => IsSaved ? "Enregistré dans l'historique" : "Enregistrer";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    private string? statusMessage;

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

    partial void OnEntryChanged(HistoryEntry? value)
    {
        if (value is not null)
            _ = LoadAsync(value);
    }

    private async Task LoadAsync(HistoryEntry value)
    {
        ResultTitle = value.Title;
        DetailSubtitle = HistoryGrouper.DetailLabel(value, DateTime.Now);

        try
        {
            if (value.Kind == HistoryKind.Vlsm)
                ShowVlsm(value);
            else
                ShowSubnet(value);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or System.Text.Json.JsonException)
        {
            StatusMessage = "Ce calcul ne peut pas être affiché (données invalides).";
        }

        IsSaved = await repository.GetAsync(value.Id) is not null;
    }

    private void ShowSubnet(HistoryEntry value)
    {
        var result = HistoryReader.ToSubnetResult(value);

        IsSubnet = true;
        IsVlsm = false;
        ClassLabel = ResultFormatter.ClassLabel(result.Class);
        ResultItems = ResultFormatter.ToItems(result);
        exportText = ResultFormatter.ToClipboardText(result);
    }

    private void ShowVlsm(HistoryEntry value)
    {
        var result = HistoryReader.ToVlsmResult(value);

        IsSubnet = false;
        IsVlsm = true;
        VlsmRows = VlsmFormatter.ToRows(result);
        VlsmSummary = VlsmFormatter.Summary(result);
        Warning = VlsmFormatter.UnallocatedWarning(result);
        exportText = VlsmFormatter.ToClipboardText(result);
    }

    // ----- Commandes -----

    [RelayCommand]
    private Task GoBack() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task CopyAsync()
    {
        if (string.IsNullOrEmpty(exportText))
            return;

        await Clipboard.Default.SetTextAsync(exportText);
        await ShowStatusAsync("Résultat copié dans le presse-papiers.");
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (Entry is null || string.IsNullOrEmpty(exportText))
            return;

        var safeName = new string(Entry.Title.Select(c => char.IsLetterOrDigit(c) || c == '.' ? c : '_').ToArray());
        var path = Path.Combine(FileSystem.CacheDirectory, $"subnetpro_{safeName}.txt");

        await File.WriteAllTextAsync(path, exportText);
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Exporter le résultat",
            File = new ShareFile(path)
        });
    }

    private bool CanSave() => Entry is not null && !IsSaved;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (Entry is null)
            return;

        await repository.AddAsync(Entry);
        IsSaved = true;
        await ShowStatusAsync("Calcul enregistré dans l'historique.");
    }

    private async Task ShowStatusAsync(string message)
    {
        StatusMessage = message;
        await Task.Delay(2000);
        StatusMessage = null;
    }
}