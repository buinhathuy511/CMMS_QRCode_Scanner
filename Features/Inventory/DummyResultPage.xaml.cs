
namespace CMMS_QRCode_Scanner.Features.Inventory;

public partial class DummyResultPage : ContentPage
{
	public DummyResultPage(DummyResultViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}


