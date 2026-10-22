
namespace CMMS_QRCode_Scanner.Features.Inventory;

public partial class ItemInfoPage : ContentPage
{
	public ItemInfoPage(ItemInfoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is ItemInfoViewModel vm)
		{
			await vm.RefreshAsync();
		}
	}
}


