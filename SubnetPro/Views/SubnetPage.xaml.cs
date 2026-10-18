using SubnetPro.ViewModels;

namespace SubnetPro.Views;

public partial class SubnetPage : ContentPage
{
	public SubnetPage(SubnetViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}