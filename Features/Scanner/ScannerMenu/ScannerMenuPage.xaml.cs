
namespace CMMS_QRCode_Scanner.Features.Scanner.ScannerMenu;

public partial class ScannerMenuPage : ContentPage
{
	public ScannerMenuPage(ScannerMenuViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}


