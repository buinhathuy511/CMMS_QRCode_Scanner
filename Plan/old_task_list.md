<!-- PHASE 0 - CHỐT KIẾN TRÚC
1. QR / BARCODE IDENTITY
- Mỗi đối tượng có một BarcodeID duy nhất.
- BarcodeID là identity dùng để tìm trực tiếp record.
- Không sử dụng Part Code làm QR identity.
- Không sử dụng Serial Code làm QR identity.
- Đối với Coded Spare Part:
    + Mỗi item/serial là một record riêng.
    + Mỗi item có một BarcodeID riêng.
    + Hai item có cùng Part Code vẫn có BarcodeID khác nhau.
- Đối với Equipment:
    + Mỗi Equipment có một BarcodeID riêng.
- Đối với Non-Coded Spare Part:
    + BarcodeID gắn với Spare Part record/inventory record theo thiết kế hiện tại.
    + Không tạo QR riêng cho từng đơn vị quantity vì Non-Coded Part được quản lý theo quantity.
2. QR FORMAT
Không còn format:
SP{BarcodeID}
EQ{BarcodeID}
Thay vào đó sử dụng BarcodeID duy nhất.
Ví dụ:
BC000001
BC000002
BC000003
Entity Type được xác định từ database/API sau khi scan.
Không cần encode Spare Part / Equipment vào QR.
3. QR = RECORD
- Một BarcodeID chỉ trỏ tới một record cụ thể.
- Không dùng một QR để đại diện cho nhiều Coded Part items.
- Không dùng Part Code làm QR identity.
4. DATABASE
BarcodeID:
- UNIQUE
- Không reuse
- Không thay đổi trong suốt lifecycle của record.
Part Code:
- Chỉ là thông tin business.
- Có thể có nhiều item cùng Part Code.
Serial Code:
- Dùng để nhận diện item trong business data.
- Không dùng làm QR identity.
5. ARCHITECTUR
.NET MAUI
        ↓
ASP.NET Core API
        ↓
SQL Server

Scanner gửi:
BarcodeID
API tìm trực tiếp record tương ứng.

Phase 1 - Chuẩn bị Database CMMS
1.1. Spare Part
- Kiểm tra bảng Spare Part hiện tại
- Xác định Coded Part / Non-coded Part
- Xác định bảng chứa Serial Code (SP hoặc EQ)
- Thêm BarcodeID và set là UNIQUE
- Cho phép NULL trong giai đoạn migration
- Tạo index cho BarcodeID để tăng tốc độ truy vấn
- Kiểm tra relationship giữa Part Code và Serial Code
1.2. Equipment
- Kiểm tra bảng Equipment hiện tại
- Thêm BarcodeID và set là UNIQUE
- Cho phép NULL trong giai đoạn migration
- Tạo index cho BarcodeID để tăng tốc độ truy vấn
1.3. Backend
- Tạo service generate BarcodeID
- Đảm bảo BarcodeID không bao giờ bị reuse
- Tự tạo BarcodeID khi tạo Spare Part và Equipment mới
- Không thay đổi BarcodeID khi Part Code / Equipment Code thay đổi
- Tạo BarcodeID cho những Spare Part / Equipment cũ 

Phase 2 - Generate QR Image / Label
- Tạo QR Generator trên CMMS Backend
- QR chứa SP+BarcodeID / EQ+BarcodeID
- Test scan bằng điện thoại
- Tạo template label SP / EQ
┌─────────────────┐
│    QR CODE      │             
│                 │
│   SP-000123     │
│   Bearing 6204  │
└─────────────────┘
┌─────────────────┐
│    QR CODE      │
│                 │
│   EQ-000123     │
│   CNC Machine   │
└─────────────────┘
- Export PDF hàng loạt
- Print selected
- Print single QR
- Reprint QR

Phase 3 - QR Migration thực tế
- Generate QR cho Spare Part / Equipment cũ
- Kiểm tra duplicate BarcodeID
- Kiểm tra record nào còn NULL
- Export danh sách QR
- In QR hàng loạt

Phase 4 - API cho Scanner
4.1. Authentication
- ITSM login API
- Access Token
- Refresh Token
- Token expiration
- Logout
- Permission
4.2. QR Scan
- Scan
    POST /api/qr/scan
- Request:
    {
        "barcodeId": "SP-000123"
    }
- Response:
    + Spare Part
    {
        "status": "success",
        "barcodeID": "SP000123",
        "entityType": "Spare Part",
        "partCode": "000123",
        "partName": "Bearing 6204",
        "location": "Location A",
        "availableQty": 10
        "actions": [
            "VIEW",
            "EDIT",
            "EXPORT_OUTBOUND"
        ]
    }
    + Equipment
    {
        "status": "success",
        "barcodeID": "EQ000123",
        "entityType": "Equipment",
        "partCode": "000123",
        "partName": "Bearing 6204",
        "location": "Location A",
        "availableQty": 10
        "actions": [
            "VIEW",
            "EDIT",
            "EXPORT_OUTBOUND"
        ]
    }
4.3. Spare Part API
a. View
- Get Spare Part Info
b. Edit
- Edit basic info
- Không cho edit qty, remaing qty, inventory transactions, serial code
- Save
- Permission
c. Export Outbound
- Export Non-coded Spare Part
     + API Check Stock
     + Validate Quantity > 0
     + Validate Quantity <= Available Qty
     + FIFO by AGE
     + Determine inventory batches
     + Create outbound transaction
     + Deduct remaining quantity
     + Update status
     + Save Export Type
     + Save user
     + Save date/time
     + Generate Outbound No
     + Transaction rollback if failed
- Export Coded Spare Part
    + Get available serial list
    + Search serial
    + Validate selected Serial
    + Validate Serial status = Available
    + Qty automatically = 1
    + Save Export Type
    + Create Outbound
    + Update Serial status
    + Generate Outbound No
4.4. Equipment API
a. View
- Get Equipment Info
b. Edit
- Edit basic info
- Save
- Permission
c. Maintenance
- Create Maintenance Record
    + Maintenance Type
    + Description
    + Date
    + Cost
    + User/Technician
    + Save
- Permission

Phase 5 - Mobile App .NET MAUI
5.1. Project
- Create .NET MAUI project
- Android target
- Configure API URL
- Configure authentication
- Configure secure token storage
5.2. Login
- Login = tài khoản ITSM
- Login UI
- Login API
- Token management
- Logout

Phase 6 - Scanner & UI
6.1. Home Screen
- Home
- Scan button
- Username
- Logout/Profile nếu cần
6.2. QR Scanner
- Camera permission
- QR Scanner
- Detect QR
- Parse BarcodeID
- Call /api/qr/scan
- Loading
- Invalid QR
- QR not found
- Unauthorized
- Network error
Fallback
- Manual ID input
- Upload image -> QR recognition

Phase 7 - Mobile Non-Coded Part UI
7.1. View
- Part Code
- Part Name
- Import Date
- Import Qty
- Remaining Qty
- Age
- Status
- Edit button
- Export Outbound button
7.2. Export
- Type = Export Purpose
- Qty input
- Stock validation
- Confirm
- Success

Phase 8 - Mobile Coded Part UI
8.1. View
- Coded Part View
- Total Qty
- Available Qty
- Serial List
- Search Serial
- Scroll
- Pagination / Lazy Loading
- Serial Status
- Edit button
- Export button
8.2. Export
- Serial Search
- Serial Selection
- Qty fixed = 1
- Export Type
- Confirm
- Success

Phase 9 - Mobile Equipment UI
9.1. View
- Equipment Code
- Equipment Name
- QR Code
- Asset No
- Status
- Edit button
- Maintenance button
9.2. Edit
- Edit basic info
- Save
- No inventory field
9.3. Maintenance
- Input
- Validation
- Save
- Success

Phase 10 -  Testing
10.1. QR
- Existing Spare Part QR
- Existing Equipment QR
- New Spare Part QR
- New Equipment QR
- Part Code changed → QR still works
- Equipment Code changed → QR still works
- Duplicate BarcodeID
- Invalid BarcodeID
- Retired/deleted record
10.2. Spare Part
- Non-Coded View
- Non-Coded Edit
- Coded View
- Serial Search
- Serial Scroll
- Coded Edit
- Quantity cannot be edited
10.3. Outbound
- Non-Coded export
- Insufficient stock
- FIFO
- Multiple AGE batches
- Coded export
- Serial unavailable
- Serial already exported
- Qty always 1
- Export Type
- Cancel
- Confirm
- Duplicate/concurrent export
10.4. Equipment
- View
- Edit
- Maintenance
- Save
- No Export button
10.5. Mobile
- Login
- Scan
- Manual input
- Upload image
- Network error
- API timeout
- Permission
- Token expiration
- APK installation -->