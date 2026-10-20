using SubnetPro.ViewModels;

namespace SubnetPro.Views;

public partial class SplashPage : ContentPage
{
    private readonly SplashViewModel viewModel;

    public SplashPage(SplashViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.StartAsync();
    }
}