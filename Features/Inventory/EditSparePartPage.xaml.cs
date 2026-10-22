namespace CMMS_QRCode_Scanner.Features.Inventory;

public partial class EditSparePartPage : ContentPage
{
	public EditSparePartPage(EditSparePartViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
