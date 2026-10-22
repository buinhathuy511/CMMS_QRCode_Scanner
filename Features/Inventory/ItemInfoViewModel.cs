using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CMMS_QRCode_Scanner.Models;
using CMMS_QRCode_Scanner.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CMMS_QRCode_Scanner.Features.Inventory
{
    [QueryProperty(nameof(BarcodeId), "BarcodeId")]
    public partial class ItemInfoViewModel : BaseViewModel
    {
        private readonly IMockDatabaseService _dbService;
        private readonly IRecentScanService _recentScanService;

        public ItemInfoViewModel(IMockDatabaseService dbService, IRecentScanService recentScanService)
        {
            _dbService = dbService;
            _recentScanService = recentScanService;
            Title = "Item Information";
        }

        [ObservableProperty]
        private string barcodeId = string.Empty;

        [ObservableProperty]
        private BaseItem? currentItem;

        [ObservableProperty]
        private SparePart? nonCodedPart;

        [ObservableProperty]
        private CodedSparePart? codedPart;

        [ObservableProperty]
        private Equipment? equipmentItem;

        [ObservableProperty]
        private bool isNonCoded;

        [ObservableProperty]
        private bool isCoded;

        [ObservableProperty]
        private bool isEquipment;

        [ObservableProperty]
        private bool hasItem;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        partial void OnBarcodeIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    await LoadItemAsync(value);
                });
            }
        }

        public async Task RefreshAsync()
        {
            if (!string.IsNullOrEmpty(BarcodeId))
            {
                await LoadItemAsync(BarcodeId);
            }
        }

        private async Task LoadItemAsync(string id)
        {
            IsBusy = true;
            HasError = false;

            CurrentItem = await _dbService.GetItemByBarcodeIdAsync(id);

            if (CurrentItem == null)
            {
                HasError = true;
                HasItem = false;
                ErrorMessage = $"Item not found for Barcode ID: {id}";
                IsNonCoded = false;
                IsCoded = false;
                IsEquipment = false;
            }
            else
            {
                HasItem = true;
                string description = "";
                if (CurrentItem is CodedSparePart csp)
                {
                    CodedPart = csp;
                    IsCoded = true;
                    description = $"Coded Part - {csp.PartName}";
                }
                else if (CurrentItem is SparePart sp)
                {
                    NonCodedPart = sp;
                    IsNonCoded = true;
                    description = $"Spare Part - {sp.PartName}";
                }
                else if (CurrentItem is Equipment eq)
                {
                    EquipmentItem = eq;
                    IsEquipment = true;
                    description = $"Equipment - {eq.EquipmentName}";
                }

                _recentScanService.AddScan(CurrentItem.BarcodeId, description);
            }

            IsBusy = false;
        }

        [RelayCommand]
        public async Task EditAsync()
        {
            if (CurrentItem is SparePart sp)
            {
                var navParam = new Dictionary<string, object>
                {
                    { "BarcodeId", sp.BarcodeId }
                };
                await Shell.Current.GoToAsync("EditSparePart", navParam);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Info", "Editing is only supported for Spare Parts.", "OK");
            }
        }
    }
}

