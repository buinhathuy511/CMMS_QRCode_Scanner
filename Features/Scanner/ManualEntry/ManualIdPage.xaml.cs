
namespace CMMS_QRCode_Scanner.Features.Scanner.ManualEntry;

public partial class ManualIdPage : ContentPage
{
	public ManualIdPage(ManualIdViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    private async void SearchButton_Clicked(object sender, EventArgs e)
    {
        BarcodeEntry.Unfocus();
        BarcodeEntry.IsEnabled = false;
        await Task.Delay(50);
        BarcodeEntry.IsEnabled = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        BarcodeEntry.Unfocus();
    }
}


