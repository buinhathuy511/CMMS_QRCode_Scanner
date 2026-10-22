# Phase 0 - Chốt kiến trúc
## 0.1. Barcode Identity
* BarcodeID là identity duy nhất để truy vấn record.
* Không sử dụng Part Code làm QR identity.
* Không sử dụng Serial Code làm QR identity.
* Không sử dụng Equipment Code làm QR identity.
* BarcodeID phải UNIQUE.
* BarcodeID không được reuse.
* BarcodeID không thay đổi trong suốt lifecycle của record.
## 0.2. Barcode cho từng loại dữ liệu
### Non-Coded Spare Part
* Một BarcodeID đại diện cho một Spare Part / inventory record theo cấu trúc database hiện tại.
* Quantity được quản lý theo inventory quantity.
* Không tạo một barcode riêng cho từng đơn vị quantity.
### Coded Spare Part
* Mỗi coded item / serial là một record riêng.
* Mỗi coded item có một BarcodeID riêng.
* Cùng một Part Code có thể có nhiều coded item.
* Mỗi coded item có barcode khác nhau.
Ví dụ:
| Part Code | Serial Code | BarcodeID |
| --------- | ----------- | --------- |
| TEST-004  | 1612352     | BC000001  |
| TEST-004  | 123         | BC000002  |
| TEST-004  | 456         | BC000003  |
### Equipment
* Mỗi Equipment có một BarcodeID riêng.
## 0.3. Barcode Content
QR chỉ chứa:
`BarcodeID`
Ví dụ:
`BC000001`
Không sử dụng:
* SP000001
* EQ000001
* Part Code
* Serial Code
* Equipment Code
Entity Type được xác định bởi Backend sau khi nhận BarcodeID.
## 0.4. QR / Barcode Mapping
```text
BarcodeID
    ↓
1 record / 1 item
```
Đối với Coded Spare Part:
```text
BarcodeID
    ↓
1 Coded Item
    ↓
1 Serial
```
Không sử dụng một QR để đại diện cho nhiều coded items.
## 0.5. Application Architecture
```text
.NET MAUI Mobile App
        ↓
ASP.NET Core API
        ↓
SQL Server
```
Không lưu ảnh QR trong database.

# Phase 1 - Chuẩn bị Database CMMS
## 1.1. Non-Coded Spare Part
* Kiểm tra bảng Spare Part hiện tại.
* Xác định cấu trúc Coded / Non-Coded.
* Xác định inventory quantity.
* Thêm BarcodeID.
* BarcodeID = UNIQUE.
* Cho phép NULL trong giai đoạn migration.
* Tạo index cho BarcodeID.
* BarcodeID không phụ thuộc Part Code.
* Part Code có thể thay đổi nhưng BarcodeID không thay đổi.
## 1.2. Coded Spare Part
* Kiểm tra bảng chứa Serial Code / coded item.
* Xác định mỗi Serial Code/item là một record độc lập.
* Thêm BarcodeID vào record coded item.
* BarcodeID = UNIQUE.
* Tạo index BarcodeID.
* Mỗi coded item chỉ có một BarcodeID.
* Một BarcodeID chỉ được map tới một coded item.
* Không group BarcodeID theo Part Code.
* Không dùng Part Code làm QR identity.
* Không dùng Serial Code làm QR identity.
Ví dụ:
```text
TEST-004 / 1612352 → BC000001
TEST-004 / 123     → BC000002
TEST-004 / 456     → BC000003
```
## 1.3. Equipment
* Kiểm tra bảng Equipment.
* Thêm BarcodeID.
* BarcodeID = UNIQUE.
* Tạo index BarcodeID.
* Một Equipment = một BarcodeID.
* Equipment Code có thể thay đổi nhưng BarcodeID không thay đổi.
## 1.4. Backend Barcode Generator
Tạo service generate BarcodeID.
Requirements:
* Tự động generate BarcodeID.
* Không duplicate.
* Không reuse BarcodeID.
* Không phụ thuộc Part Code.
* Không phụ thuộc Serial Code.
* Không phụ thuộc Equipment Code.
* BarcodeID không thay đổi sau khi record được tạo.
## 1.5. Migration
Đối với dữ liệu cũ:
### Non-Coded
* Generate BarcodeID cho các record cần QR.
### Coded
* Generate BarcodeID cho TỪNG coded item / Serial.
* Không generate một BarcodeID cho toàn bộ Part Code.
### Equipment
* Generate BarcodeID cho từng Equipment.
Kiểm tra:
* Duplicate BarcodeID.
* NULL BarcodeID.
* Coded item không có BarcodeID.
* Một BarcodeID map nhiều record.
* BarcodeID không có record tương ứng.

# Phase 2 - Generate QR Image / Label
## 2.1. QR Generator
Tạo QR Generator trên CMMS Backend.
QR chỉ chứa:
```text
BC000001
```
Không chứa:
```text
SP000001
EQ000001
```
## 2.2. Non-Coded Spare Part Label
Ví dụ:
```text
┌─────────────────┐
│     QR CODE     │
│                 │
│    BC000001     │
│                 │
│  Bearing 6204   │
└─────────────────┘
```
## 2.3. Coded Spare Part Label
Mỗi item có QR riêng.
Ví dụ:

```text
┌─────────────────┐
│     QR CODE     │
│                 │
│    BC000002     │
│                 │
│  Serial: 123    │
│  Súng keo       │
└─────────────────┘
```
Nếu có:
```text
100 coded items
```
thì generate:
```text
100 BarcodeID
100 QR Code
```
## 2.4. Equipment Label
Mỗi Equipment có QR riêng.
Ví dụ:
```text
┌─────────────────┐
│     QR CODE     │
│                 │
│    BC000100     │
│                 │
│  CNC Machine    │
└─────────────────┘
```
## 2.5. Print / Export
* Export PDF hàng loạt.
* Print selected.
* Print single QR.
* Reprint QR.
* Coded Part có thể chọn từng item để print.
* Không generate QR theo Part Code.

# Phase 3 - QR Migration thực tế
## 3.1. Non-Coded Spare Part
* Generate BarcodeID.
* Generate QR.
* Kiểm tra duplicate.
* Kiểm tra NULL.
* Export danh sách QR.
* In QR hàng loạt.
## 3.2. Coded Spare Part
* Lấy toàn bộ coded item / Serial hiện tại.
* Generate BarcodeID cho từng item.
* Generate QR cho từng item.
* Không group theo Part Code.
Ví dụ:
```text
TEST-004 / 1612352 → BC000001
TEST-004 / 123     → BC000002
TEST-004 / 456     → BC000003
```
## 3.3. Equipment
* Generate BarcodeID.
* Generate QR.
* Export danh sách.
* In QR hàng loạt.
## 3.4. Migration Validation
Kiểm tra:
* Duplicate BarcodeID.
* NULL BarcodeID.
* Một BarcodeID có nhiều record.
* Một coded item không có BarcodeID.
* Một Equipment không có BarcodeID.
* BarcodeID không tồn tại record.

# Phase 4 - API cho Scanner và Outbound
# 4.1. Authentication
* ITSM Login API.
* Access Token.
* Refresh Token.
* Token expiration.
* Logout.
* Permission.
# 4.2. QR Scan API
Endpoint:
```text
POST /api/qr/scan
```
Request:
```json
{
    "barcodeId": "BC000002"
}
```
Backend:
```text
BarcodeID
    ↓
Find record
    ↓
Determine Entity Type
    ↓
Return record information
```
## Coded Spare Part Response
```json
{
    "status": "success",
    "barcodeID": "BC000002",
    "entityType": "Coded Spare Part",
    "partCode": "TEST-004",
    "partName": "Súng keo",
    "serialCode": "123",
    "location": "Warehouse A",
    "status": "Available",
    "actions": [
        "VIEW",
        "EDIT"
    ]
}
```
Không trả về Serial List.
Không cần Search Serial.
Không cần Select Serial.
## Non-Coded Spare Part Response
```json
{
    "status": "success",
    "barcodeID": "BC000001",
    "entityType": "Non-Coded Spare Part",
    "partCode": "000123",
    "partName": "Bearing 6204",
    "location": "Location A",
    "availableQty": 65,
    "actions": [
        "VIEW",
        "EDIT"
    ]
}
```
## Equipment Response
```json
{
    "status": "success",
    "barcodeID": "BC000100",
    "entityType": "Equipment",
    "equipmentCode": "EQ000123",
    "equipmentName": "CNC Machine",
    "location": "Production Area A",
    "status": "Running",
    "actions": [
        "VIEW",
        "EDIT",
        "MAINTENANCE"
    ]
}
```
IMPORTANT:
`EXPORT_OUTBOUND` không còn là action của View/Edit.
Export Outbound là một module/workflow riêng.
---
# 4.3. Spare Part View API
## Non-Coded
* Get Spare Part Info.
* Get BarcodeID.
* Get Part Code.
* Get Part Name.
* Get Import Date.
* Get Import Quantity.
* Get Remaining Quantity.
* Get Age.
* Get Status.
* Get Location.
## Coded
* Get coded item bằng BarcodeID.
* Get BarcodeID.
* Get Part Code.
* Get Part Name.
* Get Serial Code.
* Get Import Date.
* Get Import Quantity.
* Get Remaining Quantity.
* Get Age.
* Get Status.
* Get Location.
Không lấy Serial List cho View.
# 4.4. Spare Part Edit API
## Non-Coded
Cho phép edit:
* Part Code.
* Part Name.
* Location.
* Basic information.
Không cho edit:
* Quantity.
* Remaining Quantity.
* Inventory Transactions.
* BarcodeID.
## Coded
Cho phép edit:
* Basic information.
Không cho edit:
* BarcodeID.
* Serial Code.
* Quantity.
* Remaining Quantity.
* Inventory Transactions.
Requirements:
* Save.
* Permission validation.
# 4.5. Export Outbound API
Export Outbound là một workflow độc lập.
Không phụ thuộc vào màn hình View/Edit.
Endpoint có thể thiết kế riêng, ví dụ:
```text
POST /api/outbound
```
Outbound có thể chứa nhiều item.
## 4.5.1. Non-Coded Spare Part Export
User chọn Spare Part và nhập Quantity.
Backend:
* Check Stock.
* Validate Quantity > 0.
* Validate Quantity <= Available Quantity.
* FIFO by AGE.
* Determine inventory batches.
* Create outbound transaction.
* Deduct remaining quantity.
* Update status nếu cần.
* Save Export Type.
* Save user.
* Save date/time.
* Generate Outbound No.
* Transaction rollback nếu failed.
## 4.5.2. Coded Spare Part Export
Coded item:
```text
1 BarcodeID = 1 item = 1 unit
```
Nhưng một Outbound có thể chứa nhiều coded items.
User có thể:
```text
Scan BC000001
Scan BC000002
Scan BC000003
```
Outbound:
```text
Items = 3
Quantity = 3
```
Backend nhận danh sách BarcodeID:
```json
{
    "barcodeIds": [
        "BC000001",
        "BC000002",
        "BC000003"
    ],
    "exportType": "Maintenance"
}
```
Backend:
* Validate tất cả BarcodeID.
* Validate item tồn tại.
* Validate item chưa được export.
* Validate item Status = Available.
* Mỗi BarcodeID = 1 unit.
* Quantity = số coded items được chọn.
* Save Export Type.
* Create outbound transaction.
* Update status từng item.
* Generate Outbound No.
* Save user.
* Save date/time.
* Rollback toàn bộ transaction nếu một item lỗi.
Không:
* Search Serial.
* Select Serial từ Serial List.
* Nhập Quantity thủ công cho coded items.
* Fix Quantity của toàn bộ Outbound = 1.
# 4.6. Export Type
Export Type chỉ là category / purpose.
Ví dụ:
* Production.
* Maintenance.
* Internal Use.
* Other.
Export Type hiện tại:
* Chỉ lưu thông tin.
* Không quyết định business logic.
* Không quyết định permission.
* Không quyết định inventory behavior.
# 4.7. Equipment API
## View
* Get Equipment Info bằng BarcodeID.
## Edit
* Edit basic information.
* Save.
* Permission.
* BarcodeID read-only.
## Maintenance
Create Maintenance Record:
* Maintenance Type.
* Description.
* Date.
* Cost.
* User / Technician.
* Save.
* Permission.
Equipment không có:
* Export.
* Export Outbound.
* Inventory transaction.

# Phase 5 - Mobile App .NET MAUI
## 5.1. Project
* Create .NET MAUI project.
* Android target.
* Configure API URL.
* Configure authentication.
* Configure secure token storage.
* Create Models.
* Create Views.
* Create ViewModels.
* Create Services.
* Create reusable UI components.
* Create global theme.
## 5.2. Login
* Login = ITSM account.
* Login UI.
* Login API.
* Token management.
* Token expiration.
* Logout.
* Permission.
Giai đoạn UI trước:
* Mock login.
* Mock user.
* Mock token.
Sau khi UI hoàn thiện mới connect API thật.

# Phase 6 - Scanner & UI
## 6.1. Home Screen
Home gồm:
* Username.
* Profile.
* Logout.
* Scan & View.
* Export Outbound.
* Manual Barcode ID.
* Scan from Image.
## 6.2. Scan & View
Flow:
```text
Scan QR
    ↓
BarcodeID
    ↓
API
    ↓
Entity Type
    ↓
View
```
Scanner không quyết định Spare Part hay Equipment bằng prefix.
## 6.3. QR Scanner
* Camera permission.
* QR Scanner.
* Detect QR.
* Read BarcodeID.
* Call `/api/qr/scan`.
* Loading.
* Invalid QR.
* QR not found.
* Unauthorized.
* Network error.
## 6.4. Manual Barcode ID
Input:
```text
BC000001
```
Không dùng:
```text
SP000001
EQ000001
```
## 6.5. Upload Image
* Upload image.
* QR recognition.
* Get BarcodeID.
* Call API.

# Phase 7 - Mobile Non-Coded Spare Part UI
## 7.1. View
Hiển thị:
* BarcodeID.
* Part Code.
* Part Name.
* Import Date.
* Import Qty.
* Remaining Qty.
* Age.
* Status.
* Location.
Actions:
* Edit.
Không đặt Export Outbound trực tiếp trên màn hình View.
## 7.2. Edit
* Edit basic information.
* Quantity read-only.
* Remaining Quantity read-only.
* BarcodeID read-only.
* Save.
* Success.
## 7.3. Export Outbound
Export nằm trong menu/module:
```text
EXPORT OUTBOUND
```
Không bắt buộc phải đi từ View.
Flow:
```text
Export Outbound
    ↓
Select / Scan Non-Coded Part
    ↓
Enter Quantity
    ↓
Select Export Type
    ↓
Validation
    ↓
Confirmation
    ↓
Success
```
## 7.4. Validation
* Quantity > 0.
* Quantity <= Available Quantity.
* Insufficient stock.
* FIFO.
* Multiple AGE batches.

# Phase 8 - Mobile Coded Spare Part UI
## 8.1. Coded Item View
Khi scan một BarcodeID:
```text
BarcodeID
    ↓
1 Coded Item
```
Hiển thị:
* BarcodeID.
* Part Code.
* Part Name.
* Serial Code.
* Import Date.
* Import Qty.
* Remaining Qty.
* Age.
* Location.
* Status.
Actions:
* Edit.
Không có:
* Serial List.
* Search Serial.
* Select Serial.
* Pagination.
* Lazy Loading.
* Export button trực tiếp trong View.
## 8.2. Edit
Cho phép:
* Edit basic information.
* Save.
Không cho edit:
* BarcodeID.
* Serial Code.
* Quantity.
* Remaining Quantity.
* Inventory transaction.
## 8.3. Export Outbound
Export là module riêng.
User có thể scan nhiều coded items:
```text
BC000001
BC000002
BC000003
```
Mỗi item:
```text
1 item = 1 unit
```
Nhưng:
```text
1 Outbound = nhiều items
```
UI:
```text
Export Outbound
Selected Items:
- BC000001 / Serial 1612352
- BC000002 / Serial 123
- BC000003 / Serial 456
Total Items:
3
Total Quantity:
3
Export Type:
Production
```
Quantity:
* Tự động tính.
* Không cho user nhập.
## 8.4. Remove Item
User có thể remove một item trước khi Confirm.
Ví dụ:
```text
3 items
    ↓
Remove BC000002
    ↓
2 items
    ↓
Quantity = 2
```
## 8.5. Duplicate Scan
Nếu scan cùng một BarcodeID nhiều lần:
* Không add duplicate.
* Hiển thị thông báo:
  "Item already added."
## 8.6. Coded Item Validation
* BarcodeID tồn tại.
* Item tồn tại.
* Status = Available.
* Item chưa được export.
* Item không được add duplicate.
## 8.7. Confirmation
Hiển thị:
* Selected Items.
* Total Quantity.
* Export Type.
Button:
* Cancel.
* Confirm Export.
## 8.8. Success
Hiển thị:
* Outbound No.
* Total Items.
* Total Quantity.
* Export Type.
* Status.

# Phase 9 - Mobile Equipment UI
## 9.1. Equipment View
Hiển thị:
* BarcodeID.
* Equipment Code.
* Equipment Name.
* QR / BarcodeID.
* Asset No.
* Location.
* Status.
Actions:
* Edit.
* Maintenance.
KHÔNG có Export.
## 9.2. Edit
* Edit basic information.
* Save.
* BarcodeID read-only.
* No inventory fields.
Không có:
* Quantity.
* Remaining Quantity.
* Inventory.
* Export.
* Outbound.
## 9.3. Maintenance
Input:
* Maintenance Type.
* Description.
* Date.
* Cost.
* User / Technician.
Validation:
* Required fields.
* Cost validation.
Save:
* Loading.
* Success.
* Maintenance record created.
## 9.4. Maintenance History
Hiển thị:
* Date.
* Maintenance Type.
* Description.
* Cost.
* Technician.
* Status.

# Phase 10 - Testing
## 10.1. Barcode / QR
* Existing Non-Coded Spare Part QR.
* Existing Coded Item QR.
* Existing Equipment QR.
* New Non-Coded Spare Part QR.
* New Coded Item QR.
* New Equipment QR.
* Part Code changed → Barcode vẫn hoạt động.
* Serial Code changed → Barcode vẫn hoạt động.
* Equipment Code changed → Barcode vẫn hoạt động.
* Duplicate BarcodeID.
* Invalid BarcodeID.
* NULL BarcodeID.
* Retired record.
* Deleted record.
* Barcode không map đúng record.
## 10.2. Coded Spare Part
* Scan đúng coded item.
* Hai item cùng Part Code nhưng Barcode khác nhau.
* Barcode A không trả về item B.
* View.
* Edit.
* BarcodeID không editable.
* Serial Code không editable.
* Quantity của item = 1.
* Item Available.
* Item unavailable.
* Item already exported.
* Duplicate scan.
* Remove item khỏi Export List.
## 10.3. Non-Coded Spare Part
* View.
* Edit.
* Quantity validation.
* Insufficient stock.
* FIFO.
* Multiple AGE batches.
* Export.
* Export Type.
* Cancel.
* Confirm.
## 10.4. Export Outbound
### Non-Coded
* Export một loại Spare Part.
* Nhập Quantity.
* Quantity > 0.
* Quantity <= Available.
* Insufficient stock.
* FIFO.
* Multiple inventory batches.
### Coded
* Export một coded item.
* Export nhiều coded items.
* Scan nhiều BarcodeID.
* Quantity tự động = số item.
* Không nhập Quantity.
* Duplicate Barcode.
* Remove item.
* Item unavailable.
* Item already exported.
* Một item lỗi → rollback toàn bộ transaction.
* Export Type.
* Cancel.
* Confirm.
* Generate Outbound No.
* Duplicate / concurrent export.
## 10.5. Equipment
* View.
* Edit.
* Maintenance.
* Save.
* Maintenance validation.
* Maintenance history.
* No Export button.
* No Outbound.
* No Inventory field.
## 10.6. Mobile
* Login.
* Logout.
* Scan QR.
* Manual BarcodeID.
* Upload image.
* Network error.
* API timeout.
* Camera permission.
* Storage permission nếu cần.
* Unauthorized.
* Token expiration.
* APK installation.
* Different Android screen sizes.
## 10.7. Navigation
Kiểm tra:
```text
Login
  ↓
Home
  ├── Scan & View
  │     ├── Non-Coded Spare Part
  │     ├── Coded Spare Part
  │     └── Equipment
  │
  ├── Export Outbound
  │     ├── Non-Coded
  │     └── Coded
  │
  ├── Manual Barcode
  │
  └── Scan from Image
```
## 10.8. Business Separation
Đảm bảo:
```text
VIEW / EDIT
```
và
```text
EXPORT OUTBOUND
```
là hai workflow độc lập.
Không thiết kế:
```text
Scan Item
   ↓
View
   ↓
Export
```
như một flow bắt buộc.
Có thể:
```text
Scan & View
```
để xem/chỉnh sửa một item.
Hoặc:
```text
Export Outbound
```
để tạo một outbound chứa một hoặc nhiều item.
