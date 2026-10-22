using CMMS_QRCode_Scanner.Models;
using CMMS_QRCode_Scanner.Services;
using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CMMS_QRCode_Scanner.Features.ExportOutbound
{
    public partial class ExportConfirmViewModel : BaseViewModel
    {
        private readonly IMockDatabaseService _databaseService;

        // Static properties to receive data from ExportSetupPage (MAUI limitation for complex objects)
        public static ExportOrderRequest? PendingRequest { get; set; }
        public static string? PendingMovementTypeName { get; set; }
        public static List<ExportLineItem>? PendingItems { get; set; }

        public ExportConfirmViewModel(IMockDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Xác Nhận Xuất Kho";
        }

        // --- Observable Properties ---

        [ObservableProperty]
        private string movementTypeName = string.Empty;

        [ObservableProperty]
        private string exportDateDisplay = string.Empty;

        [ObservableProperty]
        private int totalItems;

        [ObservableProperty]
        private int totalQuantity;

        [ObservableProperty]
        private bool isSubmitting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotSuccess))]
        private bool isSuccess;

        public bool IsNotSuccess => !IsSuccess;

        [ObservableProperty]
        private string resultExportCode = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasError;

        public ObservableCollection<ExportLineItem> ConfirmItems { get; } = new();

        // --- Lifecycle ---

        public void Initialize()
        {
            if (PendingRequest == null || PendingItems == null) return;

            MovementTypeName = PendingMovementTypeName ?? "";
            ExportDateDisplay = PendingRequest.ExportDate.ToString("yyyy-MM-dd");
            TotalItems = PendingItems.Count;
            TotalQuantity = PendingItems.Sum(i => i.Quantity);

            ConfirmItems.Clear();
            foreach (var item in PendingItems)
            {
                ConfirmItems.Add(item);
            }
        }

        // --- Commands ---

        [RelayCommand]
        public async Task ConfirmExportAsync()
        {
            if (PendingRequest == null) return;
            if (IsSubmitting) return;

            IsSubmitting = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _databaseService.CreateExportOrderAsync(PendingRequest);

                if (response.Success && response.Data != null)
                {
                    IsSuccess = true;
                    ResultExportCode = response.Data.ExportCode;

                    // Clear static data
                    PendingRequest = null;
                    PendingMovementTypeName = null;
                    PendingItems = null;
                }
                else
                {
                    HasError = true;
                    ErrorMessage = !string.IsNullOrWhiteSpace(response.ErrorMessage)
                        ? response.ErrorMessage
                        : $"Không thể tạo phiếu xuất kho (HTTP {response.StatusCode}). Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi không xác định: {ex.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        [RelayCommand]
        public async Task GoHomeAsync()
        {
            // Navigate back to root (Home)
            await Shell.Current.GoToAsync("//Home");
        }

        [RelayCommand]
        public async Task GoBackToEditAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
