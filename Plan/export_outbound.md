# Export Outbound – QR Scan App

## 1. Mục tiêu

Xây dựng chức năng Export Outbound trên QR Scan App, cho phép người dùng scan QR của Spare Part và thêm trực tiếp vào danh sách xuất kho.

Flow:

Export Outbound
→ Chọn Movement Type
→ Scan QR
→ Xác định Spare Part
→ Add vào danh sách
→ Kiểm tra/chỉnh Quantity
→ Confirm Export

---

## 2. Thông tin phiếu xuất

Phiếu xuất gồm:

- Movement Type:
  - Maintenance
  - Production
  - Scrap
  - Supplier Return
- Export Date: mặc định ngày hiện tại.
- Đính kèm tài liệu (Hình ảnh/Tài liệu): Cho phép chụp ảnh hoặc tải file lên làm minh chứng (tương tự chức năng trên web).
- Danh sách Spare Part xuất kho.

---

## 3. Scan Spare Part

Khi scan QR:

- Lấy BarcodeID từ QR.
- Xác định Spare Part tương ứng.
- Tự động thêm Spare Part vào danh sách Export.
- Không yêu cầu người dùng chọn lại Part sau khi scan.

Nếu QR không hợp lệ hoặc không tìm thấy Spare Part → hiển thị lỗi.

Equipment không được phép xuất kho.

---

## 4. Coded Spare Part

Đối với Coded Spare Part:

- Mỗi BarcodeID tương ứng với một item/Serial cụ thể.
- Scan QR → tự động thêm item vào danh sách.
- Serial Code được lấy từ item đã scan.
- Quantity mặc định = 1.
- Quantity luôn cố định = 1, không cho người dùng chỉnh sửa.
- Không yêu cầu chọn Serial thủ công.
- Không áp dụng FIFO.

Ví dụ:

Scan:

`BC000002`

→ PDA  
→ Serial: `VNFITPDA0001`  
→ Quantity: `1`

---

## 5. Non-Coded Spare Part

- Đối với Non-Coded Spare Part:
- Scan QR → tự động thêm Spare Part vào danh sách.
- Hiển thị Tồn kho khả dụng (Available Quantity) và Đơn vị tính (Unit) để người dùng nắm được (VD: `Tồn: 50 MÉT`).
- Quantity mặc định = 1.
- Người dùng được phép thay đổi Quantity.
- Quantity phải lớn hơn 0.
- Quantity không được vượt quá tồn kho khả dụng.
- Cột Serial Code trong danh sách hiển thị chuỗi: `FIFO Tự Động`.
- Khi Export, Backend tự động xử lý FIFO.
- App không yêu cầu người dùng chọn inventory batch.

Ví dụ:

Scan:

`BC000001`

→ DÂY MẠNG CAT6  
→ Quantity mặc định: `1`

Người dùng có thể đổi:

`1 → 20`

---

## 6. Danh sách Export

Danh sách hiển thị:

- Mã phụ tùng
- Tên phụ tùng
- Đơn vị tính (Unit) (Bổ sung thêm để rõ ràng số lượng)
- Serial Code (Hiện Serial cho Coded, và hiện `FIFO Tự Động` cho Non-Coded)
- Số lượng (Quantity)
- Xóa

Có thể chứa đồng thời:

- Coded Spare Part
- Non-Coded Spare Part

Ví dụ:

| Mã | Tên | Serial | SL |
|---|---|---|---:|
| ITTEST0001 | DÂY MẠNG CAT6 | FIFO Tự động | 20 |
| ITTEST0002 | PDA | VNFITPDA0001 | 1 |
| ITTEST0003 | Chuột | FIFO Tự động | 2 |

---

## 7. Duplicate Scan

Nếu người dùng scan cùng một BarcodeID nhiều lần:

- Không tạo duplicate item trong danh sách.
- Thông báo item đã được thêm.

Người dùng có thể xóa item khỏi danh sách trước khi Confirm.

---

## 8. Quantity

### Coded

- Quantity = 1.
- Không cho chỉnh sửa.

### Non-Coded

- Cho phép chỉnh sửa.
- Phải > 0.
- Không vượt quá Available Quantity.

Tổng Quantity của phiếu = tổng Quantity của tất cả item trong danh sách.

---

## 9. Confirm Export

Không cho Confirm nếu:

- Chưa chọn Movement Type.
- Danh sách Export rỗng.
- Quantity không hợp lệ.

Trước khi Export, hiển thị thông tin xác nhận:

- Movement Type
- Tổng số item
- Tổng số lượng
- Các tài liệu/hình ảnh đính kèm (nếu có)

Người dùng có thể:

- Cancel
- Confirm Export

---

## 10. Backend Business Rules

Backend chịu trách nhiệm:

### Coded Spare Part

- Kiểm tra BarcodeID tồn tại.
- Kiểm tra item còn Available.
- Kiểm tra item chưa được Export.
- Export đúng item/Serial được scan.
- Quantity = 1.

### Non-Coded Spare Part

- Kiểm tra BarcodeID tồn tại.
- Kiểm tra tồn kho.
- Kiểm tra Quantity hợp lệ.
- Tự động xử lý FIFO.
- Trừ tồn kho theo FIFO.

### Transaction

Một phiếu Export phải được xử lý như một transaction.

Nếu một item không thể Export → toàn bộ phiếu phải rollback.

---

## 11. Kết quả Export

Sau khi Export thành công:

- Tạo Outbound No.
- Hiển thị:
  - Outbound No
  - Movement Type
  - Tổng số item
  - Tổng số lượng
  - Trạng thái
- Xóa danh sách hiện tại.
- Cho phép quay lại màn hình chính.

---

## 12. Error Cases

Cần xử lý:

- QR không hợp lệ.
- BarcodeID không tồn tại.
- Equipment được scan.
- Item không Available.
- Item đã được Export.
- Scan trùng.
- Quantity không hợp lệ.
- Không đủ tồn kho.
- API/Network error.
- Export thất bại.

---

## 13. Acceptance Criteria

### Coded Spare Part

- Scan QR → item tự động được thêm.
- Serial đúng với item được scan.
- Quantity luôn = 1.
- Không thể chỉnh Quantity.
- Scan trùng không tạo duplicate.

### Non-Coded Spare Part

- Scan QR → item tự động được thêm.
- Hiển thị rõ Tồn kho hiện tại và Đơn vị tính cho người dùng thấy.
- Cột Serial Code hiển thị: `FIFO Tự Động`.
- Quantity mặc định = 1.
- Có thể chỉnh Quantity.
- Không thể nhập vượt tồn kho.
- Export sử dụng FIFO.

### Export

- Có thể xuất nhiều item trong cùng một phiếu.
- Có thể kết hợp Coded và Non-Coded.
- Có thể xóa item trước khi Export.
- Confirm thành công tạo Outbound.
- Nếu một item lỗi → toàn bộ phiếu không được ghi nhận.