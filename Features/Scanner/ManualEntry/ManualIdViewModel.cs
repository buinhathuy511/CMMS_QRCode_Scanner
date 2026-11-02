using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CMMS_QRCode_Scanner.Features.Scanner.ManualEntry
{
    [QueryProperty(nameof(Mode), "Mode")]
    public partial class ManualIdViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string mode = string.Empty;

        public ManualIdViewModel()
        {
            Title = "Enter Barcode ID";
        }

        partial void OnModeChanged(string value)
        {
            if (string.Equals(value, "export", System.StringComparison.OrdinalIgnoreCase))
            {
                Title = "Nhập Mã Barcode (Xuất Kho)";
            }
        }

        [ObservableProperty]
        private string barcodeId;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private bool hasError;

        [RelayCommand]
        public async Task SearchAsync()
        {
            HasError = false;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(BarcodeId))
            {
                ErrorMessage = "Vui lòng nhập Barcode ID.";
                HasError = true;
                return;
            }

            IsBusy = true;

            var trimmedId = BarcodeId.Trim();
            BarcodeId = string.Empty; // Reset form

            if (string.Equals(Mode, "export", System.StringComparison.OrdinalIgnoreCase))
            {
                // Return to ExportSetupPage with the entered barcode
                await Shell.Current.GoToAsync($"..?ScannedBarcodeId={Uri.EscapeDataString(trimmedId)}");
            }
            else
            {
                // Navigate to real ItemInfo page
                await Shell.Current.GoToAsync($"ItemInfo?BarcodeId={Uri.EscapeDataString(trimmedId)}");
            }

            IsBusy = false;
        }
    }
}

