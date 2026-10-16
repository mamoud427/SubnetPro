namespace SubnetPro.Controls;

/// <summary>En-tête commun des écrans : titre à gauche, badge « Hors ligne » à droite.</summary>
public partial class AppHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(AppHeader), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public AppHeader()
    {
        InitializeComponent();
    }
}