
namespace CMMS_QRCode_Scanner.Features.Scanner.CameraScan;

public partial class QrScannerPage : ContentPage
{
    private readonly QrScannerViewModel _viewModel;

	public QrScannerPage(QrScannerViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
        _viewModel = viewModel;

        barcodeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
        {
            Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            await _viewModel.CheckCameraPermissionAsync();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.IsDetecting = false; // Tắt camera khi rời khỏi trang
    }

    private void CameraBarcodeReaderView_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        var first = e.Results?.FirstOrDefault();
        if (first != null)
        {
            // ZXing triggers this event on a background thread.
            // We MUST dispatch UI updates and Navigation to the Main Thread to prevent crashes.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _viewModel.ProcessBarcodeCommand.Execute(first.Value);
            });
        }
    }
}


