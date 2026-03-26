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

---
## 3. Phân tích nghiệp vụ & mô hình dữ liệu (liên hệ entity)

Phần backend tập trung vào luồng đặt vé -> thanh toán -> xác nhận vé điện tử (qua email) và cung cấp thống kê cho admin. Dưới đây là các nghiệp vụ chính và chúng ánh xạ tới các entity/quan hệ hiện có trong dự án.

### 3.1. Bối cảnh miền (Domain) và các entity cốt lõi

Các entity chính:
- `User`: tài khoản người dùng.
- `Movie`: thông tin phim, có cờ `IsActive` để bật/tắt hiển thị và `ReleaseDate` để phân loại “Sắp chiếu / Đang chiếu” (phần UI xử lý).
- `Cinema` và `Auditorium`: cụm rạp và phòng chiếu.
- `Seat`: ghế trong auditorium, thuộc `SeatType` (`Standard`, `VIP`, `Couple`) và có cờ `IsActive`.
- `Showtime`: suất chiếu của một `Movie` tại một `Auditorium`, chứa `StartTime`, `EndTime` và giá theo loại ghế (`StandardPrice`, `VipPrice`, `CouplePrice`).
- `Booking`: đơn đặt vé (trạng thái `Pending`, `Paid`, `Confirmed`, `Cancelled`).
- `Ticket`: từng vé/ghế nằm trong một `Booking`, lưu `SeatId` và `Price` tại thời điểm đặt.
- `Payment`: bản ghi thanh toán liên kết với `Booking` qua `BookingId`, lưu `Amount`, `PaymentDate`, `PaymentMethod`, `TransactionId`.

Quan hệ nghiệp vụ:
- `Movie` -> `Showtime` (một phim có nhiều suất).
- `Auditorium` -> `Seat` (một phòng có nhiều ghế).
- `Showtime` -> `Seat` (ghế “có thể chọn” được xác định theo `AuditoriumId` của `Showtime`).
- `Booking` -> `Ticket` (một booking có nhiều ticket tương ứng nhiều ghế).
- `Booking` -> `Payment` (một booking có thể có payment; trong hệ thống hiện tại coi như upsert 1 bản ghi theo `BookingId`).

### 3.2. Nghiệp vụ đặt vé (Booking Flow)

Luồng tổng quát:
1. Người dùng chọn `Showtime` (suất chiếu).
2. Backend cung cấp danh sách ghế còn trống cho `Showtime`:
   - Endpoint: `GET /api/Booking/available-seats/{showtimeId}`
   - Quy tắc: ghế được đánh dấu `IsAvailable = false` nếu ghế đó đã có ticket trong booking có `Status` thuộc `Paid` hoặc `Confirmed`.
   - Liên quan entity:
     - `Showtime` xác định `AuditoriumId`.
     - `Seat` cung cấp thông tin ghế.
     - `Ticket` + `Booking.Status` quyết định ghế nào đã bị bán.
3. Người dùng gửi yêu cầu tạo booking:
   - Endpoint: `POST /api/Booking/create`
   - Quy tắc:
     - Kiểm tra `Showtime.EndTime` chưa kết thúc (không cho đặt nếu suất đã qua).
     - Với mỗi `SeatId`:
       - Ghế phải thuộc đúng `AuditoriumId` của suất.
       - Ghế không được trùng với danh sách ghế đã có ticket trong booking `Paid/Confirmed`.
     - Tạo `Booking` ở trạng thái `Pending`.
     - Tạo `Ticket` cho từng ghế và tính `Booking.TotalAmount` = tổng `Ticket.Price`.
   - Liên quan entity:
     - `Booking` (lưu trạng thái & tổng tiền)
     - `Ticket` (lưu ghế + giá)
     - `SeatType` -> ánh xạ giá từ `Showtime` (`StandardPrice/VipPrice/CouplePrice`)

### 3.3. Nghiệp vụ thanh toán & webhook (Payment Confirmation)

Hệ thống nhận thanh toán từ SePay thông qua webhook và cập nhật trạng thái đơn:
1. Webhook:
   - Endpoint: `POST /api/webhooks/sepay`
   - Backend đọc payload (tolerant với kiểu dữ liệu number/string) và lấy:
     - `transferAmount` (hoặc `amount_in`)
     - `content` (chứa mã booking theo format content; hệ thống có logic tách prefix và fallback)
     - `id` (transaction id)
2. Xác định `bookingId` từ `content`:
   - Logic `ResolveBookingIdAsync`:
     - Cắt bỏ các prefix hay gặp (`slotify`, `slotifyok`, ...).
     - Chuẩn hoá chuỗi về dạng GUID (loại dấu `-`).
     - Nếu chưa parse được: thử đối chiếu từ danh sách pending gần nhất hoặc mapping theo `transactionId`.
3. Xác thực số tiền:
   - So sánh `booking.TotalAmount` với `amountIn`.
   - Nếu không khớp: từ chối cập nhật (throw `BadRequestException`).
4. Cập nhật trạng thái thanh toán:
   - Repository `ConfirmPayment`:
     - Chỉ update `Booking.Status` sang `Paid` nếu booking đang ở `Pending` (atomic update bằng điều kiện trong DB).
     - Upsert `Payment` theo `BookingId` với `TransactionId`.
5. Trường hợp webhook trùng/đến muộn:
   - Nếu `ConfirmPayment` không cập nhật (vì booking không còn Pending nữa) nhưng booking đang `Paid/Confirmed`, hệ thống coi như webhook duplicate và có thể re-send email xác nhận.
   - Liên quan entity:
     - `Booking.Status` là nguồn sự thật cho ghế/đơn đã thanh toán.
     - `Payment.TransactionId` phục vụ đối chiếu webhook.

### 3.4. Nghiệp vụ email xác nhận vé (Email Confirmation)

Vì webhook có thể gửi nhiều lần, backend cần tránh spam email:
- Luồng gửi email xác nhận:
  - Khi thanh toán thành công qua webhook: gửi qua `SendBookingConfirmationEmailAsync(bookingId)`
  - Endpoint fallback cho user:
    - `POST /api/Booking/{id}/send-my-confirmation-email` (Authorize)
    - Cho phép user yêu cầu email lại nếu chưa nhận.
- Dedup chống gửi trùng:
  - `BookingService` dùng `ConcurrentDictionary<Guid, DateTime>` và một cửa sổ thời gian (hiện tại 3 phút) theo `bookingId`.
  - Nếu booking vừa được gửi trong thời gian cửa sổ: bỏ qua.
- Email request được xây dựng từ entity:
  - `BookingConfirmationEmailRequest` lấy:
    - Thông tin `Movie`, `Cinema`, `Auditorium`, `StartTime/EndTime`
    - `BookingCode`, `TotalAmount`, `Payment.TransactionId`
    - Danh sách `Ticket` (sắp xếp theo hàng/số ghế) và ánh xạ nhãn `SeatType` sang tiếng Việt.

### 3.5. Nghiệp vụ thống kê dashboard cho admin

Dashboard admin cần dữ liệu “thật” dựa trên DB (không mock).
- Endpoint:
  - `GET /api/Booking/admin/dashboard?recentLimit=6` (Roles = `ADMIN`)
- Quy tắc dữ liệu theo thời gian:
  - Coi “hôm nay” theo `DateTime.UtcNow.Date`.
- Thống kê hiện có:
  - `ticketsSoldToday`: số `Ticket` thuộc booking `Paid/Confirmed` và `BookingDate >= startUtc`.
  - `paidBookingsTodayCount`: số lượng booking `Paid/Confirmed` trong hôm nay.
  - `revenueToday`: tổng `Booking.TotalAmount` của booking `Paid/Confirmed` hôm nay.
  - `recentPaidBookings`: danh sách booking `Paid/Confirmed` gần đây nhất trong hôm nay (giới hạn theo `recentLimit`), kèm thông tin movie/rạp/phòng + ghế.

