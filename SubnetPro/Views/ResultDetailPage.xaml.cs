using SubnetPro.ViewModels;

namespace SubnetPro.Views;

public partial class ResultDetailPage : ContentPage
{
    public ResultDetailPage(ResultDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}