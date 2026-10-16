namespace SubnetPro;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // La charte graphique n'a qu'un thème clair (maquettes Figma).
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}