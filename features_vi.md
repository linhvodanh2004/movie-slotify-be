# Danh Sách Tính Năng Website Đặt Vé Xem Phim

Tài liệu này mô tả các tính năng chính của hệ thống dành cho hai vai trò: **Khách hàng (User)** và **Quản trị viên (Admin)**.

## 1. Khách hàng (User)

Người dùng cuối truy cập website để tìm kiếm và đặt vé xem phim.

### 🔐 Tài khoản & Cá nhân
- **Đăng ký / Đăng nhập:** Tạo tài khoản mới, đăng nhập vào hệ thống.
- **Quản lý hồ sơ:** Cập nhật thông tin cá nhân (Tên, Email, Số điện thoại).
- **Lịch sử đặt vé:** Xem lại danh sách các vé đã đặt, trạng thái thanh toán.

### 🎬 Phim & Rạp
- **Trang chủ:** Xem danh sách phim "Đang chiếu" (Now Showing) và "Sắp chiếu" (Coming Soon).
- **Tìm kiếm:** Tìm kiếm phim theo tên, thể loại.
- **Chi tiết phim:**
    - Xem thông tin chi tiết: Đạo diễn, Diễn viên, Thời lượng, Nội dung.
    - Xem Trailer phim.
    - Xem đánh giá (Rating).

### 🎫 Đặt vé (Booking Flow)
- **Chọn suất chiếu:** Lựa chọn Rạp, Ngày chiếu và Giờ chiếu mong muốn.
- **Chọn ghế:**
    - Xem sơ đồ ghế trực quan của phòng chiếu.
    - Chọn loại ghế (Thường, VIP, Couple) với mức giá tương ứng.
- **Thanh toán:**
    - Xem tóm tắt đơn hàng (Phim, Rạp, Ghế, Tổng tiền).
    - Thực hiện thanh toán (Ví dụ: Qua thẻ, ví điện tử).
    - Nhận vé điện tử (E-Ticket) sau khi thanh toán thành công.

---

## 2. Quản trị viên (Admin)

Người quản lý hệ thống, dữ liệu phim và lịch chiếu.

### 🎥 Quản lý Phim (Movies)
- **Danh sách phim:** Xem toàn bộ phim trong hệ thống.
- **Thêm phim mới:** Nhập thông tin, upload poster, link trailer.
- **Cập nhật phim:** Sửa thông tin, thay đổi trạng thái chiếu.
- **Xóa phim:** Gỡ bỏ phim khỏi hệ thống.

### 🏢 Quản lý Rạp & Cơ sở vật chất
- **Quản lý Rạp (Cinemas):** Thêm/Sửa/Xóa thông tin các cụm rạp.
- **Quản lý Phòng chiếu (Auditoriums):** Thiết lập phòng chiếu cho từng rạp.
- **Quản lý Ghế (Seats):** Cấu hình sơ đồ ghế, thiết lập loại ghế (Standard, VIP, Couple).

### 📅 Quản lý Lịch chiếu (Showtimes)
- **Lên lịch chiếu:** Tạo suất chiếu cho phim tại phòng chiếu cụ thể.
- **Thiết lập giá vé:** Cài đặt giá vé riêng cho từng suất chiếu hoặc loại ghế (Giá thường, Giá VIP, Giá Couple).

### 📊 Quản lý Đặt vé & Doanh thu
- **Quản lý Đơn hàng:** Xem danh sách các Booking, trạng thái thanh toán.
- **Báo cáo:** Xem thống kê doanh thu theo Phim, theo Rạp hoặc theo thời gian.

### 👥 Quản lý Người dùng
- **Danh sách người dùng:** Xem thông tin các tài khoản đã đăng ký.
- **Phân quyền:** Cấp quyền quản trị (nếu có).
