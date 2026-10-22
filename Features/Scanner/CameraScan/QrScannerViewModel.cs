using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace CMMS_QRCode_Scanner.Features.Scanner.CameraScan
{
    [QueryProperty(nameof(Mode), "Mode")]
    public partial class QrScannerViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string mode = string.Empty;

        public QrScannerViewModel()
        {
            Title = "Scan QR Code";
            IsDetecting = false; // Will enable after permissions
        }

        [ObservableProperty]
        private bool isDetecting;

        [ObservableProperty]
        private bool isProcessing;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public async Task CheckCameraPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status == PermissionStatus.Granted)
            {
                IsDetecting = false;
                await Task.Delay(100); // Reset camera state after permission granted
                IsDetecting = true;
                HasError = false;
            }
            else
            {
                IsDetecting = false;
                HasError = true;
                ErrorMessage = "Camera permission is required to scan QR codes.";
            }
        }

        [RelayCommand]
        public async Task ProcessBarcodeAsync(string barcodeResult)
        {
            if (IsProcessing) return; // Prevent multiple triggers
            if (string.IsNullOrWhiteSpace(barcodeResult)) return;
            
            // Disable detecting so it doesn't trigger multiple times
            IsDetecting = false;
            IsProcessing = true;

            // Optional delay to show processing UI nicely
            await Task.Delay(500);

            if (Mode?.ToLower() == "export")
            {
                // Return to ExportSetupPage with the scanned barcode
                await Shell.Current.GoToAsync($"..?ScannedBarcodeId={barcodeResult}");
            }
            else
            {
                // Normal mode: Pass the BarcodeID to the Item Info Page
                await Shell.Current.GoToAsync($"ItemInfo?BarcodeId={barcodeResult}");
            }
            
            IsProcessing = false;
        }

        [RelayCommand]
        public async Task RetryAsync()
        {
            HasError = false;
            await CheckCameraPermissionAsync();
        }

    }
}

