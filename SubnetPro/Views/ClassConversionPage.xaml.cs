using SubnetPro.ViewModels;

namespace SubnetPro.Views;

public partial class ClassConversionPage : ContentPage
{
    public ClassConversionPage(ClassConversionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}