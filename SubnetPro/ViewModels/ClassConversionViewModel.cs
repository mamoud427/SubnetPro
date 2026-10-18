using Android.Locations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubnetPro.Core;
using SubnetPro.Core.Calculators;
using SubnetPro.Core.Formatting;
using System.Formats.Asn1;

namespace SubnetPro.ViewModels;

public partial class ClassConversionViewModel : BaseViewModel
{
    private enum SourceField { Decimal, Binary }

    // Le champ modifié en dernier est celui qui sert de point de départ à la conversion.
    private SourceField lastEdited = SourceField.Decimal;
    private bool isUpdating;

    // ----- Identification de la classe -----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AddressError))]
    [NotifyPropertyChangedFor(nameof(HasAddressError))]
    private string address = "192.168.10.34";

    [ObservableProperty]
    private bool hasClass;

    [ObservableProperty]
    private string classTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasClassNote))]
    private string? classNote;

    [ObservableProperty]
    private IReadOnlyList<ClassTableRow> classRows = ClassTable.Build(null);

    public bool HasClassNote => !string.IsNullOrEmpty(ClassNote);

    public string? AddressError =>
        string.IsNullOrWhiteSpace(Address) || IPv4.TryParse(Address, out _)
            ? null
            : "Adresse IPv4 invalide (ex. 192.168.10.34).";

    public bool HasAddressError => AddressError is not null;

    // ----- Conversion binaire <-> décimal -----

    [ObservableProperty]
    private string decimalText = "192";

    [ObservableProperty]
    private string binaryText = "11000000";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConversionResult))]
    private string? conversionResult;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConversionError))]
    private string? conversionError;

    public bool HasConversionResult => !string.IsNullOrEmpty(ConversionResult);

    public bool HasConversionError => !string.IsNullOrEmpty(ConversionError);

    public ClassConversionViewModel()
    {
        UpdateClass();
        ConvertValue();
    }

    partial void OnAddressChanged(string value) => UpdateClass();

    partial void OnDecimalTextChanged(string value)
    {
        if (!isUpdating)
            lastEdited = SourceField.Decimal;
    }

    partial void OnBinaryTextChanged(string value)
    {
        if (!isUpdating)
            lastEdited = SourceField.Binary;
    }

    private void UpdateClass()
    {
        if (!IPv4.TryParse(Address, out var ip))
        {
            HasClass = false;
            return;
        }

        var info = IpClassifier.Classify(ip);
        ClassTitle = ResultFormatter.ClassLabel(info);
        ClassNote = info.PrivateRange is null ? null : $"Plage privée : {info.PrivateRange}";
        ClassRows = ClassTable.Build(info.Letter);
        HasClass = true;
    }

    [RelayCommand]
    private void ConvertValue()
    {
        ConversionError = null;

        int value;
        if (lastEdited == SourceField.Decimal)
        {
            if (!int.TryParse(DecimalText?.Trim(), out value) || value is < 0 or > 255)
            {
                Fail("Saisir un entier entre 0 et 255.");
                return;
            }
        }
        else if (!BaseConverter.TryBinaryToDecimal(BinaryText, out value))
        {
            Fail("Saisir de 1 à 8 bits (0 ou 1).");
            return;
        }

        var bits = BaseConverter.DecimalToBinary(value);

        isUpdating = true;
        DecimalText = value.ToString();
        BinaryText = bits;
        isUpdating = false;

        ConversionResult = $"{value} = {bits}₂";
    }

    private void Fail(string message)
    {
        ConversionError = message;
        ConversionResult = null;
    }
}