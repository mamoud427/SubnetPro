namespace SubnetPro.Controls;

/// <summary>Une ligne « libellé / valeur » des résultats (masque, réseau, broadcast...).</summary>
public partial class ResultRow : ContentView
{
    public static readonly BindableProperty CaptionProperty =
        BindableProperty.Create(nameof(Caption), typeof(string), typeof(ResultRow), string.Empty);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(ResultRow), string.Empty);

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public ResultRow()
    {
        InitializeComponent();
    }
}