using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;

namespace CMMS_QRCode_Scanner.Features.Inventory
{
    [QueryProperty(nameof(BarcodeId), "BarcodeId")]
    public partial class DummyResultViewModel : BaseViewModel
    {
        public DummyResultViewModel()
        {
            Title = "Scan Result";
        }

        [ObservableProperty]
        private string barcodeId;
    }
}

