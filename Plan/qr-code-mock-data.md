Dựa trên file `Data/MockData/database.json` hiện có trong source code, hãy triển khai chức năng QR Scanner để sử dụng trực tiếp với các dữ liệu mẫu trong file JSON.

### 1. Tạo QR Code cho Mock Data

Sử dụng toàn bộ các item mẫu trong `database.json` để tạo QR Code tương ứng.

Các BarcodeID mẫu gồm:

**Non-Coded Spare Part**

* `BC000001`
* `BC000002`
* `BC000003`

**Coded Spare Part**

* `BC000101`
* `BC000102`
* `BC000103`

**Equipment**

* `BC000201`
* `BC000202`
* `BC000203`

Yêu cầu:

* QR Code chỉ chứa đúng giá trị `BarcodeID`.
* Ví dụ QR của item `BC000001` chỉ chứa:
  `BC000001`
* Không đưa Part Code, Serial Code hoặc Equipment Code vào nội dung QR.
* Mỗi BarcodeID phải có một QR Code riêng.
* Có thể tạo QR Code động từ `BarcodeID`, không cần lưu ảnh QR vào database.
* Hiển thị các QR mẫu trong ứng dụng để thuận tiện cho việc test scanner.
* Có thể tạo một màn hình hoặc khu vực `Sample QR Codes` để hiển thị toàn bộ QR mẫu.

### 2. Cấp quyền Camera

Triển khai quyền truy cập camera cho ứng dụng .NET MAUI Android.

Yêu cầu:

* Khai báo camera permission đúng cách cho Android.
* Khi người dùng mở chức năng Scan QR lần đầu, kiểm tra quyền camera.
* Nếu chưa được cấp quyền:

  * Hiển thị yêu cầu cấp quyền camera.
  * Nếu user cho phép → mở scanner.
  * Nếu user từ chối → không mở scanner và hiển thị thông báo phù hợp.
* Xử lý trường hợp user đã cấp quyền từ trước.
* Xử lý trường hợp permission bị từ chối vĩnh viễn nếu cần.
* Không crash app khi permission bị từ chối.

### 3. QR Scanner

Tạo màn hình `QR Scanner`.

Flow:

```text
Home
  ↓
Scan QR
  ↓
Request Camera Permission
  ↓
Open Camera Scanner
  ↓
Scan QR
  ↓
Read BarcodeID
  ↓
Search database.json
  ↓
Find matching item
  ↓
Item Information Screen
```

Scanner phải đọc được các QR mẫu:

```text
BC000001
BC000002
BC000003

BC000101
BC000102
BC000103

BC000201
BC000202
BC000203
```

Sau khi scan thành công:

* Lấy nội dung QR làm `BarcodeID`.
* Tìm kiếm `BarcodeID` trong `database.json`.
* Tự xác định item thuộc loại nào:

  * Non-Coded Spare Part
  * Coded Spare Part
  * Equipment
* Không xác định entity type dựa trên prefix của BarcodeID.
* Nếu tìm thấy item → chuyển sang màn hình thông tin.
* Nếu không tìm thấy → hiển thị thông báo `Item not found`.
* Không tự tạo record mới nếu BarcodeID không tồn tại.

### 4. Màn hình Item Information

Sau khi scan thành công, hiển thị thông tin của item tương ứng.

#### Non-Coded Spare Part

Hiển thị:

* BarcodeID
* Part Code
* Part Name
* Import Date
* Import Qty
* Remaining Qty
* Age
* Status
* Location

Ví dụ:

```text
Barcode ID
BC000001

Part Code
RAR0001

Part Name
ĐẦU ĐỌT / STRAIGHT FITTING

Import Date
2026-07-15

Import Qty
35

Remaining Qty
35

Age
33 Days

Status
AVAILABLE

Location
A1
```

#### Coded Spare Part

Hiển thị:

* BarcodeID
* Part Code
* Part Name
* Serial Code
* Import Date
* Import Qty
* Remaining Qty
* Age
* Status
* Location

Ví dụ:

```text
Barcode ID
BC000102

Part Code
ITT004

Part Name
Súng keo

Serial Code
1612352

Import Date
2026-07-15

Import Qty
1

Remaining Qty
1

Age
33 Days

Status
AVAILABLE

Location
A1
```

#### Equipment

Hiển thị:

* BarcodeID
* Equipment Code
* Equipment Name
* Model
* Serial
* Asset No
* Location
* Status
* Next Maintenance

Ví dụ:

```text
Barcode ID
BC000201

Equipment Code
EQ000001

Equipment Name
THERMO FORMING MACHINE No1

Model
A1

Serial
120000000354

Asset No
120000000354

Location
A1

Status
OVERDUE

Next Maintenance
2026-07-15
```

### 5. Button trên màn hình Item Information

Màn hình thông tin chỉ có **2 button chính**:

```text
[ Edit ]        [ Quay lại ]
```

#### Edit

Khi nhấn `Edit`:

* Chuyển sang màn hình Edit tương ứng với loại item.
* Cho phép chỉnh sửa các thông tin được phép chỉnh sửa.
* `BarcodeID` phải luôn Read-only.
* Không được thay đổi BarcodeID.

Ở giai đoạn này chỉ cần triển khai navigation tới màn hình Edit và chuẩn bị flow chỉnh sửa mock data.

#### Quay lại

Khi nhấn:

```text
Quay lại
```

→ quay về màn hình trước đó, không tạo thay đổi dữ liệu.

### 6. Mock Database

Không hard-code dữ liệu item trực tiếp trong View.

Sử dụng:

```text
Data/
└── MockData/
    └── database.json
```

Tạo một service riêng, ví dụ:

```text
MockDatabaseService.cs
```

Service chịu trách nhiệm:

* Load `database.json`.
* Tìm item theo `BarcodeID`.
* Xác định loại item.
* Trả về item cho UI.
* Chuẩn bị method để update item ở bước Edit tiếp theo.

Kiến trúc mong muốn:

```text
QR Scanner
    ↓
BarcodeID
    ↓
MockDatabaseService
    ↓
database.json
    ↓
Find Item
    ↓
Item Information
    ↓
[ Edit ] [ Quay lại ]
```

### 7. Yêu cầu kiến trúc code

Không viết toàn bộ logic vào code-behind của Page.

Tách rõ:

```text
Models/
Services/
ViewModels/
Views/
Data/
└── MockData/
    └── database.json
```

Ưu tiên MVVM của .NET MAUI.

Tạo các model phù hợp với dữ liệu trong `database.json`.

Không tạo database SQL thật ở giai đoạn này.

Không gọi API thật ở giai đoạn này.

Mục tiêu của task này chỉ là hoàn thiện flow:

```text
QR mẫu
   ↓
Camera
   ↓
Scan BarcodeID
   ↓
Tìm item trong database.json
   ↓
Hiển thị thông tin item
   ↓
[ Edit ] [ Quay lại ]
```

Sau khi hoàn thành, đảm bảo có thể dùng camera của thiết bị Android thật để scan QR và mở đúng thông tin của cả **Non-Coded Spare Part, Coded Spare Part và Equipment**.
