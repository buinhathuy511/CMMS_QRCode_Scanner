using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CMMS_QRCode_Scanner.Features.Scanner.ManualEntry
{
    public partial class ManualIdViewModel : BaseViewModel
    {
        public ManualIdViewModel()
        {
            Title = "Enter Barcode ID";
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
                ErrorMessage = "Please enter a Barcode ID.";
                HasError = true;
                return;
            }

            IsBusy = true;

            // Navigate to real ItemInfo page
            await Shell.Current.GoToAsync($"ItemInfo?BarcodeId={BarcodeId}");
            
            BarcodeId = string.Empty; // Reset form
            
            IsBusy = false;
        }
    }
}

