# Tổng kết quá trình khởi tạo Project CMMS QR Code Scanner

Ứng dụng `.NET MAUI` đã được khởi tạo thành công với cấu trúc thư mục, Global Theme (Màu Xanh Navy/Teal), giao diện Login và logic Authentication (Mock). 

## 1. Cấu trúc Project 
Project đã được tạo tại `d:\Projects\CMMS_QRCode_Scanner` với kiến trúc MVVM bao gồm các thư mục:
- `/Models`, `/Views`, `/ViewModels`, `/Services`, `/Helpers`
- Đã cài đặt thư viện **CommunityToolkit.Mvvm** phục vụ việc xây dựng ViewModel và Databinding tự động.
- Dọn dẹp các tệp mẫu mặc định (`MainPage.xaml`).

## 2. Global Theme & Styles
Các thông số giao diện đã được thiết lập tập trung tại thư mục `Resources/Styles/`:
- **Colors.xaml**: Cấu hình màu chủ đạo là xanh Navy (`#003366`) và xanh Teal (`#008080`), màu lỗi (`#D32F2F`) theo yêu cầu.
- **Styles.xaml**: Định dạng toàn bộ ứng dụng, tăng độ lớn của các `Entry`, `Button` (bo góc tròn mềm mại), thiết lập `Shell` theo màu Xanh Navy chủ đạo để đem lại vẻ ngoài công nghiệp, hiện đại.

## 3. Kiến trúc Authentication & Mock Login
- **`IAuthService`**: Interface đóng gói logic xác thực để sau này có thể dễ dàng chuyển đổi sang API thực tế.
- **`MockAuthService`**: Cung cấp logic xác thực tạm thời. Tài khoản đúng là `ITUSER` và mật khẩu là `123456`. Dịch vụ này còn giả lập độ trễ mạng (1.5s) để có thể thấy trạng thái Loading rõ ràng.

## 4. UI Login & Navigation
- **LoginPage**:
  - Có Logo, Tựa đề và phụ đề rõ ràng.
  - Box nhập tài khoản ITSM và Mật khẩu (kèm chức năng Bật/Tắt hiển thị password bằng 1 nút icon).
  - Nút "LOGIN" sẽ bị Disable và hiển thị vòng quay `ActivityIndicator` (loading) khi ấn.
  - Các thông báo lỗi (`Username is required`, `Invalid username or password`) hiển thị đầy đủ bên dưới.
- **HomePage**: 
  - Giao diện đơn giản có nút "LOGOUT" cho phép đăng xuất nhanh và điều hướng trở lại màn hình đăng nhập.
- **AppShell**: Đã được tinh chỉnh ẩn thanh Tab bar (`TabBarIsVisible="False"`) để đem lại cảm giác mượt mà trên Mobile như thiết kế gốc.

> [!NOTE]
> **Vấn đề khi Build (Android SDK)**
> Mã nguồn C# / XAML đã hoàn tất và đạt chuẩn. Tuy nhiên, khi chạy lệnh `dotnet build` trên môi trường hiện tại, hệ thống báo lỗi không tìm thấy bộ công cụ **Android SDK** (`error XA5300`). 
> Để có thể chạy trực tiếp lên điện thoại hoặc máy ảo, bạn vui lòng cài đặt Android SDK thông qua Visual Studio Installer (chọn mục `.NET Multi-platform App UI development` và tick vào `Android SDK setup`).

## 5. Hướng dẫn chạy thử
Khi bạn đã có Visual Studio 2022 (với Workload MAUI & Android SDK) trên máy:
1. Mở file `CMMS_QRCode_Scanner.csproj` bằng Visual Studio 2022.
2. Chọn target `Android Emulator` hoặc `Windows Machine` từ thanh công cụ bên trên.
3. Nhấn **F5** hoặc **Run**.
4. Test đăng nhập với tài khoản:
   - Username: **ITUSER**
   - Password: **123456**
