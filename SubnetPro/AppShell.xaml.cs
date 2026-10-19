using SubnetPro.Helpers;
using SubnetPro.Views;

namespace SubnetPro;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages poussées au-dessus des onglets (les onglets sont déclarés dans AppShell.xaml).
        Routing.RegisterRoute(Routes.Result, typeof(ResultDetailPage));
    }
}