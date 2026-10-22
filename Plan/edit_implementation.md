Spare Part – Database, View & Edit

1. Database mẫu

Bổ sung các field còn thiếu:

BarcodeID, PartCode, PartName, SerialCode

EntityType / SerialTracking

Specification, MinStock, Unit, Quantity

Note, Supplier, Price, Location, Department, Image

ImportDate, Age, Status

Quy tắc:

Coded: Quantity = 1, có SerialCode.

Non-Coded: Quantity = tổng số lượng.

BarcodeID UNIQUE, không được thay đổi.

2. View Spare Part

Hiển thị:

Part Code, Part Name, BarcodeID

Entity Type / Serial Tracking, Serial Code (Coded)

Quantity, Min Stock, Unit

Specification, Supplier, Price

Location, Department

Import Date, Age, Status

Note, Image

3. Edit Spare Part

Cho phép Edit

Part Code, Part Name, Specification, Min Stock, Unit, Note, Supplier, Price, Location, Department, Image.

Không được Edit

BarcodeID, SerialCode, Quantity, ImportDate, Age, Status, EntityType / SerialTracking.

Các field protected phải được bảo vệ cả ở Backend/API, không chỉ khóa trên UI.

Sau khi Save, View phải hiển thị dữ liệu mới nhất.