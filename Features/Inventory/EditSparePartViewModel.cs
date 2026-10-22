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
    public partial class EditSparePartViewModel : BaseViewModel
    {
        private readonly IMockDatabaseService _dbService;

        public EditSparePartViewModel(IMockDatabaseService dbService)
        {
            _dbService = dbService;
            Title = "Edit Spare Part";
        }

        [ObservableProperty]
        private string barcodeId = string.Empty;

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

        private SparePart? _itemToEdit;

        [ObservableProperty]
        private SparePart? editableItem;

        [ObservableProperty]
        private bool isCoded;

        public string SerialCode => (_itemToEdit as CodedSparePart)?.SerialCode ?? string.Empty;

        private async Task LoadItemAsync(string id)
        {
            IsBusy = true;
            var item = await _dbService.GetItemByBarcodeIdAsync(id);
            if (item is SparePart sp)
            {
                _itemToEdit = sp;
                LoadData(sp);
            }
            IsBusy = false;
        }

        private void LoadData(SparePart item)
        {
            IsCoded = item is CodedSparePart;
            OnPropertyChanged(nameof(SerialCode));
            
            if (IsCoded)
            {
                var csp = (CodedSparePart)item;
                EditableItem = new CodedSparePart
                {
                    Id = csp.Id,
                    BarcodeId = csp.BarcodeId,
                    EntityType = csp.EntityType,
                    Type = csp.Type,
                    PartCode = csp.PartCode,
                    PartName = csp.PartName,
                    ImportDate = csp.ImportDate,
                    Quantity = csp.Quantity,
                    MinStock = csp.MinStock,
                    Unit = csp.Unit,
                    Specification = csp.Specification,
                    Note = csp.Note,
                    Supplier = csp.Supplier,
                    Price = csp.Price,
                    Department = csp.Department,
                    AgeDays = csp.AgeDays,
                    Status = csp.Status,
                    Location = csp.Location,
                    SerialCode = csp.SerialCode
                };
            }
            else
            {
                EditableItem = new SparePart
                {
                    Id = item.Id,
                    BarcodeId = item.BarcodeId,
                    EntityType = item.EntityType,
                    Type = item.Type,
                    PartCode = item.PartCode,
                    PartName = item.PartName,
                    ImportDate = item.ImportDate,
                    Quantity = item.Quantity,
                    MinStock = item.MinStock,
                    Unit = item.Unit,
                    Specification = item.Specification,
                    Note = item.Note,
                    Supplier = item.Supplier,
                    Price = item.Price,
                    Department = item.Department,
                    AgeDays = item.AgeDays,
                    Status = item.Status,
                    Location = item.Location
                };
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            if (EditableItem == null) return;
            
            if (string.IsNullOrWhiteSpace(EditableItem.PartName) || string.IsNullOrWhiteSpace(EditableItem.PartCode))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Part Name and Part Code are required.", "OK");
                return;
            }

            IsBusy = true;
            bool success = await _dbService.UpdateItemAsync(EditableItem);
            IsBusy = false;

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Item updated successfully.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to update item.", "OK");
            }
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
