using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace CMMS_QRCode_Scanner.Models
{
    // DTO to send to Backend (matches ExportOrderDto on server)
    public class ExportOrderRequest
    {
        public int? MovementTypeID { get; set; }
        public DateTime ExportDate { get; set; } = DateTime.Now;
        public List<ExportDetailRequest> Details { get; set; } = new();
    }

    public class ExportDetailRequest
    {
        public int SPID { get; set; }
        public string? PartCode { get; set; }
        public string? PartName { get; set; }
        public bool HasCode { get; set; }
        public string? SerialCode { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
    }

    // DTO received from Backend
    public class MovementTypeItem
    {
        public int MovementTypeID { get; set; }
        public string MovementTypeName { get; set; } = string.Empty;

        public override string ToString() => MovementTypeName;
    }

    // Response after creating export order
    public class ExportOrderResult
    {
        public int ExportID { get; set; }
        public string ExportCode { get; set; } = string.Empty;
    }

    // API Response wrapper with status code and error details
    public class ExportOrderResponse
    {
        public bool Success { get; set; }
        public ExportOrderResult? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }
    }

    // Paged result DTO for API responses
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    // Backend DTO for Tbl_SparePartItem
    public class SparePartItemDto
    {
        public int ItemID { get; set; }
        public int SPID { get; set; }
        public string? PartCode { get; set; }
        public string? PartName { get; set; }
        public string? SerialCode { get; set; }
        public int RemainingQuantity { get; set; }
        public string? Status { get; set; }
        public string? Unit { get; set; }
        public decimal? Price { get; set; }
        public string? SupplierName { get; set; }
        public int DaysInStock { get; set; }
        public DateTime ImportDate { get; set; }
    }

    // UI model for items in the export list
    public partial class ExportLineItem : ObservableObject
    {
        public int SPID { get; set; }
        public string PartCode { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public bool IsCoded { get; set; }
        public string? SerialCode { get; set; }
        public string DisplaySerial { get; set; } = string.Empty;
        public string BarcodeId { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }

        // Available serials list for Coded parts
        public System.Collections.ObjectModel.ObservableCollection<string> AvailableSerials { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplaySerial))]
        [NotifyPropertyChangedFor(nameof(HasNoSerials))]
        [NotifyPropertyChangedFor(nameof(HasSerialSelected))]
        private string? selectedSerial;

        partial void OnSelectedSerialChanged(string? value)
        {
            SerialCode = value;
            DisplaySerial = !string.IsNullOrWhiteSpace(value) ? value : "Chưa chọn Serial";
        }

        [ObservableProperty]
        private bool isLoadingSerials;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNoSerials))]
        private bool hasLoadedSerials;

        public bool HasNoSerials => IsCoded && HasLoadedSerials && AvailableSerials.Count == 0;
        public bool HasSerialSelected => !IsCoded || !string.IsNullOrWhiteSpace(SelectedSerial);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(QuantityDisplay))]
        [NotifyPropertyChangedFor(nameof(IsOverStock))]
        private int quantity = 1;

        // Computed: whether quantity can be edited (Non-Coded only)
        public bool IsQuantityEditable => !IsCoded;

        // Display available stock info for Non-Coded
        public string StockInfo => IsCoded ? "" : $"Tồn: {AvailableQuantity} {Unit}";

        // Warning flag when user inputs quantity > stock
        public bool IsOverStock => !IsCoded && Quantity > AvailableQuantity;

        // Pre-formatted strings to avoid FormattedText MultiBinding issues in AOT/Release
        public string PartSummary => $"{PartCode} - {PartName}";
        public string QuantityDisplay => $"{Quantity} {Unit}";
    }
}
