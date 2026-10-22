# Kế hoạch triển khai QR Scanner & Mock Database

Dựa trên yêu cầu trong `qr-code-mock-data.md` và `database.json`, đây là kế hoạch chi tiết để tích hợp máy quét QR thật bằng Camera và luồng hiển thị Item Information.

## Open Questions
> [!IMPORTANT]
> **Thư viện Scanner**
> Để quyét QR Code bằng camera thật trong .NET MAUI, thư viện tiêu chuẩn hiện tại là `ZXing.Net.Maui.Controls`. Tôi sẽ thêm package NuGet này vào dự án (`dotnet add package ZXing.Net.MAUI.Controls`). Bạn có đồng ý sử dụng thư viện này không?

> [!NOTE]
> **Trang tạo QR Code mẫu (Sample QR Codes)**
> Thay vì tạo một màn hình mới hoàn toàn rườm rà, tôi đề xuất thêm một nút nhỏ "View Sample QR Codes" tại `ScannerMenuPage` (trang menu quét mã). Nút này sẽ mở ra màn hình hiển thị 9 mã QR code (giả lập bằng cách generate image từ text) để bạn cầm điện thoại khác quét. Bạn thấy sao?

## Proposed Changes

### 1. Khởi tạo Models & MockDatabaseService
- **Models**: Tạo các class model dựa trên cấu trúc `database.json`.
  - `BaseItem` (Chứa `BarcodeId`, `Type`).
  - `SparePart` (Non-Coded).
  - `CodedSparePart`.
  - `Equipment`.
- **Service**: 
  - Đọc và parse file `database.json` nằm trong thư mục `Data/MockData/`. (Cần cấu hình file json này là `MauiAsset` hoặc đọc text tuỳ platform).
  - Cung cấp method: `GetItemByBarcodeIdAsync(string barcodeId)` trả về đúng loại đối tượng.

### 2. Cài đặt thư viện ZXing & Quyền Camera
- **NuGet Package**: Cài đặt `ZXing.Net.MAUI.Controls`.
- **AndroidManifest.xml**: Thêm quyền `<uses-permission android:name="android.permission.CAMERA" />`.
- **MauiProgram.cs**: Đăng ký `UseBarcodeReader()`.
- **Permission Check**: Xử lý logic xin quyền Camera (`Permissions.CheckStatusAsync<Permissions.Camera>()` và `Permissions.RequestAsync<Permissions.Camera>()`) trước khi mở trang quét.

### 3. Cập nhật màn hình QR Scanner (`QrScannerPage`)
- Thay thế màn hình giả lập trước đây bằng Control `CameraBarcodeReaderView` của ZXing.
- **Flow**:
  1. Quét thành công -> Lấy được text (BarcodeID).
  2. Vô hiệu hóa máy quét để tránh quét nhiều lần.
  3. Gọi `MockDatabaseService.GetItemByBarcodeIdAsync()`.
  4. Nếu tìm thấy: Điều hướng sang màn hình `ItemInfoPage` cùng dữ liệu.
  5. Nếu không tìm thấy: Báo lỗi "Item not found" và cho phép quét lại.

### 4. Màn hình Item Information (`ItemInfoPage`)
- **Giao diện động**: Sử dụng DataTemplateSelector hoặc các StackLayout ẩn/hiện (`IsVisible`) dựa trên loại Item (Non-Coded / Coded / Equipment) để render trường dữ liệu tương ứng.
- **Buttons**:
  - `[ EDIT ]`: Tạm thời hiển thị popup hoặc điều hướng sang trang trống "Edit Page Placeholder".
  - `[ QUAY LẠI ]`: Back về trang trước (trang Menu hoặc Scanner tuỳ luồng).

### 5. Màn hình Sample QR Codes (`SampleQRCodesPage`)
- Một danh sách đơn giản liệt kê 9 QR Code (BarcodeID) tương ứng với 9 item trong mock data. Có thể sử dụng ZXing BarcodeGeneratorView để vẽ trực tiếp hình vuông mã QR lên màn hình.

## Verification Plan
1. **Build project**: Đảm bảo cài package ZXing không xung đột.
2. **Quyền Camera**: Test trên máy ảo hoặc thiết bị thật (Android), đảm bảo hộp thoại xin quyền hiện ra.
3. **Quét thật**: Dùng camera soi vào mã `BC000001` trên màn hình máy tính (từ Sample QR), app sẽ nhận diện đúng và hiện trang thông tin Non-Coded Spare Part.
4. Kiểm tra các nút Edit/Back hoạt động đúng flow.
