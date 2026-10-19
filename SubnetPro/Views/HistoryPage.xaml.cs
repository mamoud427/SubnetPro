using SubnetPro.ViewModels;

namespace SubnetPro.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel viewModel;

    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    // Les onglets sont créés une seule fois : on recharge la liste à chaque affichage.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadAsync();
    }
}