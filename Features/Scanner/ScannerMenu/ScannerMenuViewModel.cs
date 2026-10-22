using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

using CMMS_QRCode_Scanner.Models;
using CMMS_QRCode_Scanner.Services;

namespace CMMS_QRCode_Scanner.Features.Scanner.ScannerMenu
{
    public partial class ScannerMenuViewModel : BaseViewModel
    {
        public System.Collections.ObjectModel.ObservableCollection<RecentScanItem> RecentScans => _recentScanService.RecentScans;
        private readonly IRecentScanService _recentScanService;

        public ScannerMenuViewModel(IRecentScanService recentScanService)
        {
            _recentScanService = recentScanService;
            Title = "View / Edit";
        }

        [RelayCommand]
        public async Task GoToScanQrAsync()
        {
            await Shell.Current.GoToAsync("QrScanner");
        }

        [RelayCommand]
        public async Task GoToManualIdAsync()
        {
            await Shell.Current.GoToAsync("ManualId");
        }

        [RelayCommand]
        public async Task GoToScanFromImageAsync()
        {
            await Shell.Current.GoToAsync("ScanFromImage");
        }

        [RelayCommand]
        public async Task GoToItemInfoAsync(string barcodeId)
        {
            if (string.IsNullOrWhiteSpace(barcodeId)) return;
            await Shell.Current.GoToAsync($"ItemInfo?BarcodeId={barcodeId}");
        }
    }
}

