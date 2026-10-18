using SubnetPro.ViewModels;

namespace SubnetPro.Views;

public partial class VlsmPage : ContentPage
{
    public VlsmPage(VlsmViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}