using CMMS_QRCode_Scanner.Models;
using CMMS_QRCode_Scanner.Services;
using CMMS_QRCode_Scanner.Shared.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;

namespace CMMS_QRCode_Scanner.Features.ExportOutbound
{
    [QueryProperty(nameof(ScannedBarcodeId), "ScannedBarcodeId")]
    public partial class ExportSetupViewModel : BaseViewModel
    {
        private readonly IMockDatabaseService _databaseService;

        public ExportSetupViewModel(IMockDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Tạo Đơn Xuất Kho";
            ExportDate = DateTime.Now;

            ExportItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (ExportLineItem item in e.NewItems)
                    {
                        item.PropertyChanged += OnItemPropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (ExportLineItem item in e.OldItems)
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;
                    }
                }
                NotifyCanSubmitChanged();
            };
        }

        private void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExportLineItem.Quantity) || e.PropertyName == nameof(ExportLineItem.SelectedSerial))
            {
                NotifyCanSubmitChanged();
            }
        }

        // --- Observable Properties ---

        [ObservableProperty]
        private List<MovementTypeItem> movementTypes = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        private MovementTypeItem? selectedMovementType;

        [ObservableProperty]
        private DateTime exportDate;

        [ObservableProperty]
        private string scannedBarcodeId = string.Empty;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool hasStatusMessage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusBgColor))]
        [NotifyPropertyChangedFor(nameof(StatusTextColor))]
        private bool isStatusError;

        // Computed color properties to avoid DataTrigger (AOT-unsafe with bool comparisons)
        public Color StatusBgColor => IsStatusError
            ? Color.FromArgb("#FFEBEE")
            : Color.FromArgb("#E8F5E9");

        public Color StatusTextColor => IsStatusError
            ? Color.FromArgb("#C62828")
            : Color.FromArgb("#2E7D32");

        public ObservableCollection<ExportLineItem> ExportItems { get; } = new();

        public bool CanSubmit => SelectedMovementType != null
                                 && ExportItems.Count > 0
                                 && ExportItems.All(i => i.Quantity > 0 
                                     && (i.IsCoded ? !string.IsNullOrWhiteSpace(i.SelectedSerial) : i.Quantity <= i.AvailableQuantity));

        public int TotalItems => ExportItems.Count;
        public int TotalQuantity => ExportItems.Sum(i => i.Quantity);
        public string TotalSummary => $"{TotalItems} item · SL: {TotalQuantity}";
        public bool HasItems => ExportItems.Count > 0;
        public bool IsEmpty => ExportItems.Count == 0;

        // --- Lifecycle ---

        public async Task InitializeAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var types = await _databaseService.GetMovementTypesAsync();
                MovementTypes = types;
            }
            catch (Exception ex)
            {
                ShowStatus($"Lỗi tải dữ liệu: {ex.Message}", true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private ExportLineItem? _targetLineItemForSerialScan;

        // Called when ScannedBarcodeId is set via query parameter (returning from scanner)
        partial void OnScannedBarcodeIdChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (_targetLineItemForSerialScan != null)
                {
                    var target = _targetLineItemForSerialScan;
                    _targetLineItemForSerialScan = null;

                    var trimmed = value.Trim();
                    var match = target.AvailableSerials.FirstOrDefault(s => string.Equals(s, trimmed, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        target.SelectedSerial = match;
                    }
                    else
                    {
                        target.AvailableSerials.Add(trimmed);
                        target.SelectedSerial = trimmed;
                    }
                    ShowStatus($"Đã chọn Serial '{target.SelectedSerial}' cho {target.PartCode}.", false);
                    NotifyCanSubmitChanged();
                    return;
                }

                _ = ProcessScannedBarcodeAsync(value);
            }
        }

        // --- Scan Processing ---

        private async Task ProcessScannedBarcodeAsync(string barcodeId)
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearStatus();

            try
            {
                // Check if already in list
                var existingItem = ExportItems.FirstOrDefault(i => string.Equals(i.BarcodeId, barcodeId, StringComparison.OrdinalIgnoreCase));
                if (existingItem != null && !existingItem.IsCoded)
                {
                    // For non-coded, increment quantity if stock permits
                    if (existingItem.Quantity < existingItem.AvailableQuantity)
                    {
                        existingItem.Quantity++;
                        ShowStatus($"Đã tăng số lượng '{existingItem.PartCode}' lên {existingItem.Quantity} {existingItem.Unit}.", false);
                    }
                    else
                    {
                        ShowStatus($"Số lượng '{existingItem.PartCode}' đã đạt tối đa tồn kho ({existingItem.AvailableQuantity} {existingItem.Unit}).", true);
                    }
                    NotifyCanSubmitChanged();
                    return;
                }

                // Lookup item via API
                var item = await _databaseService.GetItemByBarcodeIdAsync(barcodeId);

                if (item == null)
                {
                    ShowStatus("Không tìm thấy item với mã QR này.", true);
                    return;
                }

                // Equipment is not allowed
                if (item is Equipment)
                {
                    ShowStatus("Equipment không được phép xuất kho.", true);
                    return;
                }

                // Build export line item
                if (item is CodedSparePart coded)
                {
                    // Check if available
                    if (coded.Status != "Available")
                    {
                        ShowStatus($"Item '{coded.PartCode}' không còn khả dụng (Status: {coded.Status}).", true);
                        return;
                    }

                    var hasSerial = !string.IsNullOrWhiteSpace(coded.SerialCode);
                    var lineItem = new ExportLineItem
                    {
                        SPID = coded.Id,
                        PartCode = coded.PartCode,
                        PartName = coded.PartName,
                        Unit = coded.Unit,
                        IsCoded = true,
                        SerialCode = hasSerial ? coded.SerialCode : null,
                        SelectedSerial = hasSerial ? coded.SerialCode : null,
                        DisplaySerial = hasSerial ? coded.SerialCode : "Đang tải Serial...",
                        BarcodeId = barcodeId,
                        Quantity = 1,
                        AvailableQuantity = coded.Quantity > 0 ? coded.Quantity : 1,
                        IsLoadingSerials = true
                    };

                    ExportItems.Add(lineItem);
                    ShowStatus($"Đã thêm: {coded.PartCode} - {coded.PartName}", false);

                    // Load available serials asynchronously
                    _ = LoadSerialsForLineItemAsync(lineItem, coded.SerialCode);
                }
                else if (item is SparePart sp)
                {
                    if (sp.Quantity <= 0)
                    {
                        ShowStatus($"'{sp.PartCode}' hết hàng tồn kho.", true);
                        return;
                    }

                    ExportItems.Add(new ExportLineItem
                    {
                        SPID = sp.Id,
                        PartCode = sp.PartCode,
                        PartName = sp.PartName,
                        Unit = sp.Unit,
                        IsCoded = false,
                        SerialCode = null,
                        DisplaySerial = "FIFO Tự Động",
                        BarcodeId = barcodeId,
                        Quantity = 1,
                        AvailableQuantity = sp.Quantity
                    });

                    ShowStatus($"Đã thêm: {sp.PartCode} - {sp.PartName} (Tồn: {sp.Quantity} {sp.Unit})", false);
                }

                NotifyCanSubmitChanged();
            }
            catch (Exception ex)
            {
                ShowStatus($"Lỗi: {ex.Message}", true);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- Commands ---

        [RelayCommand]
        public async Task ScanQrAsync()
        {
            // Navigate to scanner in export mode; scanner will return here with ScannedBarcodeId
            await Shell.Current.GoToAsync($"QrScanner?Mode=export");
        }

        [RelayCommand]
        public async Task ScanSerialForLineAsync(ExportLineItem item)
        {
            if (item == null) return;
            _targetLineItemForSerialScan = item;
            await Shell.Current.GoToAsync("QrScanner?Mode=export");
        }

        private async Task LoadSerialsForLineItemAsync(ExportLineItem lineItem, string? preselectedSerial)
        {
            try
            {
                lineItem.IsLoadingSerials = true;
                var serials = await _databaseService.GetAvailableSerialsAsync(lineItem.PartCode);

                // Filter out serials already selected in other lines of the same part
                var chosenInOtherLines = ExportItems
                    .Where(i => i != lineItem && i.PartCode == lineItem.PartCode && !string.IsNullOrWhiteSpace(i.SelectedSerial))
                    .Select(i => i.SelectedSerial!)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var available = serials.Where(s => !chosenInOtherLines.Contains(s)).ToList();

                lineItem.AvailableSerials.Clear();
                foreach (var s in available)
                {
                    lineItem.AvailableSerials.Add(s);
                }
                lineItem.HasLoadedSerials = true;

                if (!string.IsNullOrWhiteSpace(preselectedSerial) && lineItem.AvailableSerials.Contains(preselectedSerial))
                {
                    lineItem.SelectedSerial = preselectedSerial;
                }
                else if (lineItem.AvailableSerials.Count > 0 && string.IsNullOrWhiteSpace(lineItem.SelectedSerial))
                {
                    // Auto-select first available serial (FIFO)
                    lineItem.SelectedSerial = lineItem.AvailableSerials[0];
                }
                else if (lineItem.AvailableSerials.Count == 0)
                {
                    lineItem.DisplaySerial = "Hết Serial khả dụng";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading serials: {ex.Message}");
            }
            finally
            {
                lineItem.IsLoadingSerials = false;
                NotifyCanSubmitChanged();
            }
        }

        [RelayCommand]
        public void RemoveItem(ExportLineItem item)
        {
            ExportItems.Remove(item);
            NotifyCanSubmitChanged();
            ShowStatus("Đã xóa item khỏi danh sách.", false);
        }

        [RelayCommand]
        public async Task SubmitExportAsync()
        {
            if (!CanSubmit) return;

            // Validate quantities and serials
            foreach (var item in ExportItems)
            {
                if (item.IsCoded)
                {
                    if (string.IsNullOrWhiteSpace(item.SelectedSerial))
                    {
                        ShowStatus($"Vui lòng chọn mã Serial cho '{item.PartCode} - {item.PartName}'.", true);
                        return;
                    }
                }
                else
                {
                    if (item.Quantity <= 0)
                    {
                        ShowStatus($"Số lượng của '{item.PartCode}' phải > 0.", true);
                        return;
                    }
                    if (item.Quantity > item.AvailableQuantity)
                    {
                        ShowStatus($"Số lượng của '{item.PartCode}' vượt quá tồn kho ({item.AvailableQuantity}).", true);
                        return;
                    }
                }
            }

            // Navigate to confirm page with data
            ExportConfirmViewModel.PendingRequest = new ExportOrderRequest
            {
                MovementTypeID = SelectedMovementType!.MovementTypeID,
                ExportDate = ExportDate,
                Details = ExportItems.Select(i => new ExportDetailRequest
                {
                    SPID = i.SPID,
                    PartCode = i.PartCode,
                    PartName = i.PartName,
                    HasCode = i.IsCoded,
                    SerialCode = i.IsCoded ? (i.SelectedSerial ?? i.SerialCode) : null,
                    Quantity = i.IsCoded ? 1 : i.Quantity,
                    Unit = i.Unit
                }).ToList()
            };
            ExportConfirmViewModel.PendingMovementTypeName = SelectedMovementType!.MovementTypeName;
            ExportConfirmViewModel.PendingItems = ExportItems.ToList();

            await Shell.Current.GoToAsync("ExportConfirm");
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            if (ExportItems.Count > 0)
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Xác nhận", "Bạn có chắc muốn hủy? Danh sách hiện tại sẽ bị xóa.", "Hủy phiếu", "Tiếp tục");
                if (!confirm) return;
            }
            await Shell.Current.GoToAsync("..");
        }

        // Called when user changes quantity in the UI
        public void OnQuantityChanged(ExportLineItem item)
        {
            NotifyCanSubmitChanged();
        }

        // --- Helpers ---

        private void NotifyCanSubmitChanged()
        {
            OnPropertyChanged(nameof(CanSubmit));
            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(TotalQuantity));
            OnPropertyChanged(nameof(TotalSummary));
            OnPropertyChanged(nameof(HasItems));
            OnPropertyChanged(nameof(IsEmpty));
        }

        private void ShowStatus(string message, bool isError)
        {
            StatusMessage = message;
            HasStatusMessage = true;
            IsStatusError = isError;
        }

        private void ClearStatus()
        {
            StatusMessage = string.Empty;
            HasStatusMessage = false;
            IsStatusError = false;
        }
    }
}
