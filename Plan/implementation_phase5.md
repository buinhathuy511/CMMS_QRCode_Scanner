# Khởi tạo dự án CMMS QR Code Scanner (.NET MAUI)

Ứng dụng .NET MAUI sẽ được khởi tạo với cấu trúc chuẩn MVVM, bao gồm thiết lập Theme, màn hình Login hiện đại và Mock Authentication theo yêu cầu của bạn.

## Open Questions

> [!IMPORTANT]
> **Vị trí tạo Project:**
> Hiện tại thư mục `d:\Projects\CMMS_QRCode_Scanner` đang có thư mục `Documents` và file `Tasklist.md`.
> Khuyến nghị là chạy lệnh tạo project MAUI tại thư mục này nhưng không sinh ra folder `CMMS_QRCode_Scanner` lồng nhau (tức là tạo thẳng các file source vào `d:\Projects\CMMS_QRCode_Scanner\CMMS_QRCode_Scanner`). Bạn có muốn source code nằm trong một thư mục tên `Source` hay `App` để tách biệt với `Documents` không? Mặc định tôi sẽ tạo trong `d:\Projects\CMMS_QRCode_Scanner\CMMS_QRCode_Scanner`.

## Proposed Changes

### 1. Khởi tạo Project & Cấu trúc thư mục
- Tạo project .NET MAUI mới bằng command line.
- Thiết lập cấu trúc thư mục chuẩn:
  - `/Models`: Chứa các model data.
  - `/Views`: Các giao diện (LoginPage, HomePage).
  - `/ViewModels`: Logic điều khiển giao diện. Sẽ add package `CommunityToolkit.Mvvm`.
  - `/Services`: Chứa `IAuthService` và `MockAuthService`.
  - `/Helpers`: Helper classes.

### 2. Global Theme (Resources/Styles)
- **Colors.xaml**: Cấu hình bảng màu chuẩn.
  - Primary: Màu xanh Navy đậm (hiện đại, công nghiệp).
  - Secondary/Accent: Teal hoặc Cyan.
  - Backgrounds: Trắng/Xám nhạt.
  - Text & States: Success, Error, Warning colors.
- **Styles.xaml**: Style chuẩn cho Button (bo góc nhẹ, chữ lớn), Entry (có padding, viền rõ ràng), Label.

### 3. Kiến trúc Authentication & Mock Login
- **IAuthService.cs**: Định nghĩa các hàm `Task<bool> LoginAsync(string username, string password)` và `void Logout()`.
- **MockAuthService.cs**: Triển khai mock với logic:
  - Delay giả lập mạng (Loading).
  - Trả về `true` nếu `ITUSER` / `123456`.
  - Trả về `false` và lỗi tương ứng với các case khác.
- Cấu hình DI vào `MauiProgram.cs` (`AddSingleton<IAuthService, MockAuthService>()`).

### 4. Màn hình Login & Navigation (AppShell)
- **LoginPage.xaml & LoginViewModel**:
  - Tiêu đề: "CMMS QR Code Scanner" / "Scan. Manage. Maintain."
  - Hai ô nhập liệu: "ITSM Account" và "Password" (hỗ trợ bật tắt hiển thị password bằng 1 nút ImageButton).
  - Nút "LOGIN" với ActivityIndicator hiển thị khi đang xử lý (Disable nút).
  - Bắt lỗi: Bỏ trống "Username is required." / "Password is required.".
- **HomePage.xaml & HomeViewModel**:
  - Giao diện tạm để kiểm chứng Login thành công.
  - Nút Logout để xóa mock session và quay về trang Login.
- **AppShell.xaml**: 
  - Đăng ký Routing.
  - Logic tự động chuyển trang khi Login/Logout.

## Verification Plan

### Automated Tests
- Chạy `dotnet build` tại thư mục project để đảm bảo source code biên dịch không lỗi trên môi trường local.

### Manual Verification
- Mở project để kiểm chứng:
  1. Giao diện Login hiển thị đúng màu sắc (Xanh Navy/Teal), phù hợp trên điện thoại (Responsive, không bị che bởi bàn phím).
  2. Nút Show/Hide password hoạt động bình thường.
  3. Validate logic: Nhập sai hiển thị "Invalid username or password.".
  4. Flow Login -> Home -> Logout hoạt động trơn tru với Loading state.
