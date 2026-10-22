
namespace CMMS_QRCode_Scanner.Features.Scanner.ImageScan;

public partial class ScanFromImagePage : ContentPage
{
	public ScanFromImagePage(ScanFromImageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}


