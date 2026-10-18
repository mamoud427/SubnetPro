using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Org.W3c.Dom;
using SubnetPro.Core;
using SubnetPro.Core.Calculators;
using SubnetPro.Core.Formatting;
using SubnetPro.Core.Models;
using System.Collections.ObjectModel;
using static Android.InputMethodServices.Keyboard;

namespace SubnetPro.ViewModels;

public partial class VlsmViewModel : BaseViewModel
{
    private VlsmResult? lastResult;

    // ----- Saisie -----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NetworkError))]
    [NotifyPropertyChangedFor(nameof(HasNetworkError))]
    [NotifyCanExecuteChangedFor(nameof(CalculateCommand))]
    private string networkText = "192.168.10.0/24";

    public ObservableCollection<RequirementRow> Requirements { get; } = new();

    // ----- Résultat -----

    [ObservableProperty]
    private bool hasResult;

    [ObservableProperty]
    private IReadOnlyList<VlsmRow> rows = Array.Empty<VlsmRow>();

    [ObservableProperty]
    private string summary = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasWarning))]
    private string? warning;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFormError))]
    private string? formError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    private string? statusMessage;

    public bool HasWarning => !string.IsNullOrEmpty(Warning);

    public bool HasFormError => !string.IsNullOrEmpty(FormError);

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

    public string? NetworkError =>
        string.IsNullOrWhiteSpace(NetworkText) || TryParseNetwork(out _, out _)
            ? null
            : "Format attendu : 192.168.10.0/24 (préfixe de 0 à 30).";

    public bool HasNetworkError => NetworkError is not null;

    // Exemple de la maquette Figma : l'utilisateur peut l'éditer ou le vider.
    public VlsmViewModel()
    {
        AddRow("Admin", "50");
        AddRow("Salle TP", "25");
        AddRow("Serveurs", "10");
    }

    private void AddRow(string name, string hosts) =>
        Requirements.Add(new RequirementRow(name, hosts, row => Requirements.Remove(row)));

    private bool TryParseNetwork(out string address, out int cidr)
    {
        address = string.Empty;
        cidr = 0;

        var parts = (NetworkText ?? string.Empty).Trim().Split('/');
        if (parts.Length != 2
            || !IPv4.TryParse(parts[0], out _)
            || !int.TryParse(parts[1].Trim(), out cidr)
            || cidr is < 0 or > 30)
        {
            return false;
        }

        address = parts[0].Trim();
        return true;
    }

    // ----- Commandes -----

    [RelayCommand]
    private void AddRequirement() => AddRow(string.Empty, string.Empty);

    private bool CanCalculate() => TryParseNetwork(out _, out _);

    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private void Calculate()
    {
        FormError = null;
        Warning = null;
        StatusMessage = null;

        if (!TryParseNetwork(out var address, out var cidr))
            return;

        if (Requirements.Count == 0)
        {
            FormError = "Ajoutez au moins un besoin.";
            HasResult = false;
            return;
        }

        var requirements = new List<VlsmRequirement>();
        for (var i = 0; i < Requirements.Count; i++)
        {
            var row = Requirements[i];
            if (!int.TryParse(row.HostsText?.Trim(), out var hosts) || hosts < 1)
            {
                FormError = $"Ligne {i + 1} : le nombre d'hôtes doit être un entier supérieur ou égal à 1.";
                HasResult = false;
                return;
            }

            var name = string.IsNullOrWhiteSpace(row.Name) ? $"Segment {i + 1}" : row.Name.Trim();
            requirements.Add(new VlsmRequirement(name, hosts));
        }

        var result = VlsmCalculator.Allocate(address, cidr, requirements);
        lastResult = result;

        Rows = VlsmFormatter.ToRows(result);
        Summary = VlsmFormatter.Summary(result);
        Warning = VlsmFormatter.UnallocatedWarning(result);
        HasResult = true;
    }

    [RelayCommand]
    private async Task CopyAsync()
    {
        if (lastResult is null)
            return;

        await Clipboard.Default.SetTextAsync(VlsmFormatter.ToClipboardText(lastResult));
        StatusMessage = "Répartition copiée dans le presse-papiers.";
        await Task.Delay(2000);
        StatusMessage = null;
    }
}