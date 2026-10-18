using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static Java.Util.Jar.Attributes;

namespace SubnetPro.ViewModels;

/// <summary>Une ligne de besoin de la page VLSM (nom du segment + nombre d'hôtes saisi).</summary>
public partial class RequirementRow : ObservableObject
{
    private readonly Action<RequirementRow> removeCallback;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string hostsText = string.Empty;

    public RequirementRow(string name, string hostsText, Action<RequirementRow> removeCallback)
    {
        Name = name;
        HostsText = hostsText;
        this.removeCallback = removeCallback;
    }

    [RelayCommand]
    private void Remove() => removeCallback(this);
}