# Export Outbound – QR Scan App Implementation Plan

## Mục tiêu

Xây dựng chức năng Export Outbound (xuất kho) trên ứng dụng MAUI QR Scan, cho phép người dùng quét QR của Spare Part và tạo phiếu xuất kho trực tiếp từ app di động. Tái sử dụng backend API hiện có (`POST api/SparePart/export-order`).

---

## Kiến trúc hiện tại

- **App**: .NET MAUI, pattern MVVM (CommunityToolkit.Mvvm), DI qua `MauiProgram.cs`
- **Backend API đã có sẵn**:
  - `GET api/Scanner/{barcodeId}` – Lookup item theo barcode
  - `GET api/SparePart/movement-types` – Lấy danh sách Movement Type
  - `POST api/SparePart/export-order` – Tạo phiếu xuất kho (nhận `ExportOrderDto` với `Details[]`)
- **Cấu trúc thư mục**: `Features/{FeatureName}/` chứa Page + ViewModel

---

## Open Questions

> [!IMPORTANT]
> **Đính kèm tài liệu (Attachment):** Trong plan có đề cập chức năng "Đính kèm tài liệu/hình ảnh". Tuy nhiên, backend API `POST export-order` hiện tại nhận `ExportOrderDto` dạng JSON thuần, trường `AttachmentUrls` là `List<string>` (chỉ lưu URL). Backend chưa có endpoint upload file riêng cho Export. Bạn muốn:
> 1. **Bỏ qua Attachment** ở phiên bản App đầu tiên, chỉ tập trung vào chức năng scan + xuất kho?
> 2. Hay cần xây dựng thêm API upload file trên backend?

---

## Proposed Changes

### 1. Models (DTOs cho App)

#### [NEW] [ExportModels.cs](file:///d:/Projects/CMMS_QRCode_Scanner/Models/ExportModels.cs)

Tạo các model dùng cho chức năng Export trên App:

```csharp
// DTO gửi lên Backend
public class ExportOrderRequest
{
    public int? MovementTypeID { get; set; }
    public DateTime ExportDate { get; set; } = DateTime.Now;
    public List<ExportOrderDetailRequest> Details { get; set; } = new();
}

public class ExportOrderDetailRequest
{
    public int SPID { get; set; }
    public string? PartCode { get; set; }
    public string? PartName { get; set; }
    public bool HasCode { get; set; }
    public string? SerialCode { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
}

// DTO nhận từ Backend
public class MovementTypeDto
{
    public int MovementTypeID { get; set; }
    public string MovementTypeName { get; set; } = string.Empty;
}

// Model nội bộ cho UI (danh sách item trên màn hình)
public class ExportLineItem : ObservableObject
{
    public int SPID { get; set; }
    public string PartCode { get; set; }
    public string PartName { get; set; }
    public string Unit { get; set; }
    public bool IsCoded { get; set; }
    public string? SerialCode { get; set; }    // Serial thực (Coded) hoặc null (Non-Coded)
    public string DisplaySerial { get; set; }  // "VNFITPDA0001" hoặc "FIFO Tự Động"
    public string BarcodeId { get; set; }
    public int Quantity { get; set; }          // Có thể chỉnh sửa (Non-Coded)
    public int AvailableQuantity { get; set; } // Tồn kho khả dụng (để validate)
    public int? ItemID { get; set; }           // ItemID cụ thể (Coded)
}
```

---

### 2. Service Layer

#### [MODIFY] [IMockDatabaseService / ApiDatabaseService](file:///d:/Projects/CMMS_QRCode_Scanner/Services/ApiDatabaseService.cs)

Thêm các method mới vào interface `IMockDatabaseService` và implement trong `ApiDatabaseService`:

```csharp
// Thêm vào IMockDatabaseService (MockDatabaseService.cs)
Task<List<MovementTypeDto>> GetMovementTypesAsync();
Task<ExportOrderResult> CreateExportOrderAsync(ExportOrderRequest request);

// Implement trong ApiDatabaseService
// GET api/SparePart/movement-types
// POST api/SparePart/export-order
```

> [!NOTE]
> API `GET api/Scanner/{barcodeId}` đã có sẵn và trả về đầy đủ thông tin (`EntityType`, `Id`(SPID), `Code`, `Name`, `Quantity`, `Unit`, `Serial`, `Status`). Tuy nhiên, nó chỉ trả về `SPID` (ID bảng `Tbl_SparePart`), không trả về `ItemID` (bảng `Tbl_SparePartItem`). Đối với **Coded Spare Part**, ta cần biết `ItemID` cụ thể để Backend xử lý đúng item. Cần bổ sung backend ScannerController trả thêm `ItemID` cho Coded item, hoặc backend Export API tự tìm ItemID dựa trên SPID + SerialCode.

---

### 3. Feature Pages (App MAUI)

#### [NEW] `Features/ExportOutbound/` – Thư mục mới

Tạo 3 trang chính cho flow Export Outbound:

---

#### 3.1. [NEW] [ExportSetupPage.xaml + .cs](file:///d:/Projects/CMMS_QRCode_Scanner/Features/ExportOutbound/ExportSetupPage.xaml)
#### [NEW] [ExportSetupViewModel.cs](file:///d:/Projects/CMMS_QRCode_Scanner/Features/ExportOutbound/ExportSetupViewModel.cs)

**Màn hình thiết lập phiếu xuất** – Trang chính của flow Export

UI bao gồm:
- **Header**: "Tạo Đơn Xuất Kho"
- **Movement Type Picker**: Dropdown chọn loại hình xuất (Maintenance, Production, Scrap, Supplier Return). Load từ API `movement-types`.
- **Export Date**: DatePicker, mặc định ngày hiện tại.
- **Nút "Scan QR"**: Mở camera scanner. Sau khi scan, item tự động thêm vào danh sách bên dưới.
- **Danh sách Export Items** (CollectionView):
  - Mã phụ tùng | Tên phụ tùng | Serial Code | Số lượng | Nút Xóa
  - Non-Coded: Serial hiện "FIFO Tự Động", Quantity có thể chỉnh (Entry), hiện tồn kho khả dụng
  - Coded: Serial hiện mã Serial thực, Quantity cố định = 1, không cho chỉnh
- **Footer**: Nút "Hủy" + Nút "Lưu Phiếu" (disabled khi chưa đủ điều kiện)

Logic ViewModel:
- Load `MovementTypes` khi init
- Xử lý kết quả scan (nhận `BarcodeId` từ trang Scanner qua query param hoặc MessagingCenter)
- Gọi `Scanner/{barcodeId}` để lookup item
- Kiểm tra trùng lặp (không thêm nếu đã có)
- Kiểm tra Equipment (không cho phép)
- Validate trước khi submit:
  - MovementType đã chọn
  - Danh sách không rỗng
  - Tất cả Quantity > 0 và <= Available

---

#### 3.2. Export Scanner

**Tái sử dụng `QrScannerPage` hiện có** với chế độ "export mode":
- Khi scan từ Export flow, thay vì navigate đến `ItemInfo`, nó sẽ quay lại `ExportSetupPage` và truyền `BarcodeId` đã scan.
- Cách implement: Truyền query parameter `mode=export` khi mở Scanner. Khi scan xong, nếu `mode == export`, navigate back với kết quả thay vì đi tới ItemInfo.

---

#### 3.3. [NEW] [ExportConfirmPage.xaml + .cs](file:///d:/Projects/CMMS_QRCode_Scanner/Features/ExportOutbound/ExportConfirmPage.xaml)
#### [NEW] [ExportConfirmViewModel.cs](file:///d:/Projects/CMMS_QRCode_Scanner/Features/ExportOutbound/ExportConfirmViewModel.cs)

**Màn hình xác nhận xuất kho** – Hiển thị trước khi gửi lên server

UI bao gồm:
- Movement Type đã chọn
- Export Date
- Tổng số item
- Tổng số lượng
- Danh sách chi tiết (read-only)
- Nút "Hủy" + Nút "Xác nhận xuất kho"

Logic:
- Gọi `POST api/SparePart/export-order` 
- Hiển thị kết quả (thành công / thất bại)
- Thành công → hiển thị `ExportCode` (Outbound No), cho phép quay về Home

---

### 4. Navigation & DI Registration

#### [MODIFY] [AppShell.xaml.cs](file:///d:/Projects/CMMS_QRCode_Scanner/AppShell.xaml.cs)

Đăng ký routes mới:
```csharp
Routing.RegisterRoute("ExportSetup", typeof(Features.ExportOutbound.ExportSetupPage));
Routing.RegisterRoute("ExportConfirm", typeof(Features.ExportOutbound.ExportConfirmPage));
```

#### [MODIFY] [MauiProgram.cs](file:///d:/Projects/CMMS_QRCode_Scanner/MauiProgram.cs)

Đăng ký DI:
```csharp
builder.Services.AddTransient<Features.ExportOutbound.ExportSetupViewModel>();
builder.Services.AddTransient<Features.ExportOutbound.ExportSetupPage>();
builder.Services.AddTransient<Features.ExportOutbound.ExportConfirmViewModel>();
builder.Services.AddTransient<Features.ExportOutbound.ExportConfirmPage>();
```

#### [MODIFY] [HomeViewModel.cs](file:///d:/Projects/CMMS_QRCode_Scanner/Features/Dashboard/HomeViewModel.cs)

Thay popup placeholder bằng navigation thực:
```csharp
[RelayCommand]
public async Task GoToExportOutboundAsync()
{
    await Shell.Current.GoToAsync("ExportSetup");
}
```

---

### 5. Backend Adjustments (nếu cần)

#### [MODIFY] [ScannerController.cs](file:///d:/Projects/CMMS/CMMS.Server/Controllers/Scanner/ScannerController.cs)

Bổ sung thêm trường vào response cho Coded Spare Part:
- Khi tìm thấy Coded Spare Part (trong `Tbl_SparePartItem`), trả thêm `ItemID` để App biết chính xác item nào cần xuất.
- Hiện tại query chỉ tìm ở `Tbl_SparePart` (bảng master), cần thêm logic tìm ở `Tbl_SparePartItem` (cho Coded item có barcode riêng).

---

## Flow tổng quan

```
Home → [Export Outbound]
     → ExportSetupPage (chọn Movement Type, ngày xuất)
     → [Scan QR] → QrScannerPage (mode=export)
                  → Scan → Lookup API → Thêm vào danh sách
                  → Quay lại ExportSetupPage
     → Chỉnh sửa Quantity (Non-Coded)
     → [Lưu Phiếu] → ExportConfirmPage (xem trước)
                     → [Xác nhận] → POST API → Kết quả
                     → Quay về Home
```

---

## Verification Plan

### Build & Compile
- `dotnet build` project MAUI thành công không lỗi

### Manual Testing
1. Từ Home, bấm "Export Outbound" → ExportSetupPage hiện ra
2. Chọn Movement Type từ dropdown
3. Bấm "Scan QR" → Camera mở → Scan QR của Non-Coded part → Item xuất hiện trong danh sách với Serial = "FIFO Tự Động"
4. Scan QR của Coded part → Item xuất hiện với Serial thực, Quantity cố định = 1
5. Scan QR của Equipment → Hiện thông báo lỗi "Equipment không được phép xuất kho"
6. Scan QR trùng → Hiện thông báo "Item đã được thêm"
7. Chỉnh Quantity Non-Coded: nhập số > tồn kho → hiện cảnh báo
8. Bấm "Lưu Phiếu" → Màn hình xác nhận hiện đầy đủ thông tin
9. Bấm "Xác nhận" → API trả về thành công → Hiện Outbound No
10. Quay về Home
