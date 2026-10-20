namespace SubnetPro.Helpers;

/// <summary>Noms des routes Shell. Les Route de AppShell.xaml doivent correspondre à ces constantes.</summary>
public static class Routes
{
    // Démarrage et authentification (hors onglets : pas de barre d'onglets)
    public const string Splash = "splash";
    public const string Login = "login";
    public const string Register = "register";

    // Onglets
    public const string Home = "home";
    public const string Subnet = "subnet";
    public const string Vlsm = "vlsm";
    public const string Classe = "classe";
    public const string History = "history";

    // Page poussée au-dessus des onglets
    public const string Result = "result";
}