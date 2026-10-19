using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using SubnetPro.Core;
using SubnetPro.Core.Calculators;
using SubnetPro.Core.Formatting;
using SubnetPro.Core.History;
using SubnetPro.Core.Models;
using SubnetPro.Helpers;

namespace SubnetPro.ViewModels;

public partial class SubnetViewModel : BaseViewModel
{
    private const string DefaultCidr = "24";

    // Entrée d'historique du dernier calcul (même identifiant tant que le résultat ne change pas).
    private HistoryEntry? currentEntry;

    // ----- Saisie -----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AddressError))]
    [NotifyPropertyChangedFor(nameof(HasAddressError))]
    [NotifyCanExecuteChangedFor(nameof(CalculateCommand))]
    private string address = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CidrError))]
    [NotifyPropertyChangedFor(nameof(HasCidrError))]
    [NotifyCanExecuteChangedFor(nameof(CalculateCommand))]
    private string cidrText = DefaultCidr;

    // ----- Résultat -----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    private SubnetResult? result;

    [ObservableProperty]
    private IReadOnlyList<ResultItem> resultItems = Array.Empty<ResultItem>();

    [ObservableProperty]
    private string resultTitle = string.Empty;

    [ObservableProperty]
    private string classLabel = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    private string? statusMessage;

    // ----- Propriétés calculées pour l'interface -----

    public bool HasResult => Result is not null;

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

    // Les erreurs ne s'affichent que si le champ n'est pas vide : pas de rouge avant la saisie.
    public string? AddressError =>
        string.IsNullOrWhiteSpace(Address) || IsAddressValid
            ? null
            : "Adresse IPv4 invalide (ex. 192.168.10.34).";

    public bool HasAddressError => AddressError is not null;

    public string? CidrError =>
        string.IsNullOrWhiteSpace(CidrText) || TryGetCidr(out _)
            ? null
            : "Le préfixe doit être compris entre 0 et 32.";

    public bool HasCidrError => CidrError is not null;

    private bool IsAddressValid => IPv4.TryParse(Address, out _);

    private bool TryGetCidr(out int cidr)
    {
        var text = CidrText?.Trim().TrimStart('/') ?? string.Empty;
        return int.TryParse(text, out cidr) && cidr is >= 0 and <= 32;
    }

    // ----- Commandes -----

    private bool CanCalculate() => IsAddressValid && TryGetCidr(out _);

    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private void Calculate()
    {
        if (!IsAddressValid || !TryGetCidr(out var cidr))
            return;

        var computed = SubnetCalculator.Calculate(Address.Trim(), cidr);

        Result = computed;
        currentEntry = HistoryFactory.FromSubnet(computed, DateTime.Now);
        ResultTitle = computed.NetworkCidr;
        ClassLabel = ResultFormatter.ClassLabel(computed.Class);
        ResultItems = ResultFormatter.ToItems(computed);
        StatusMessage = null;
    }

    [RelayCommand]
    private void Clear()
    {
        Address = string.Empty;
        CidrText = DefaultCidr;
        Result = null;
        currentEntry = null;
        ResultItems = Array.Empty<ResultItem>();
        ResultTitle = string.Empty;
        ClassLabel = string.Empty;
        StatusMessage = null;
    }

    [RelayCommand]
    private Task ShowDetailAsync()
    {
        if (currentEntry is null)
            return Task.CompletedTask;

        return Shell.Current.GoToAsync(
            Routes.Result,
            new ShellNavigationQueryParameters { { "entry", currentEntry } });
    }

    [RelayCommand]
    private async Task CopyAsync()
    {
        if (Result is null)
            return;

        await Clipboard.Default.SetTextAsync(ResultFormatter.ToClipboardText(Result));
        await ShowStatusAsync("Résultat copié dans le presse-papiers.");
    }

    private async Task ShowStatusAsync(string message)
    {
        StatusMessage = message;
        await Task.Delay(2000);
        StatusMessage = null;
    }
}