# Trụ cột thiết kế và core loop

**Trạng thái:** Accepted

## 1. Fantasy của người chơi

Người chơi cảm thấy mình đang điều khiển một sinh vật mềm, dài và hơi vụng về
trong một mê cung cơ học. Mỗi bước đi vừa giải quyết vấn đề trước mắt vừa thay
đổi hình dạng cơ thể và trạng thái môi trường.

Tone là vui, sáng và cơ khí nhẹ; thất bại không mang cảm giác trừng phạt. V2 1.0
không có cốt truyện, hội thoại hay nhân vật phụ bắt buộc. Tên mode/level và visual
mechanic phải đủ để dẫn dắt trải nghiệm mà không phụ thuộc narrative.

## 2. Core loop trong một level

```text
Quan sát board
    ↓
Dự đoán kết quả của một hướng di chuyển
    ↓
Thực hiện input
    ↓
Xem sâu + môi trường phản ứng
    ↓
Đánh giá state mới
    ├── Sai kế hoạch → Undo hoặc Replay
    ├── Chưa xong → tiếp tục quan sát
    └── Đủ táo + vào cổng → Win
```

Chu kỳ phải ngắn. Từ lúc input tới lúc người chơi có thể suy nghĩ bước tiếp theo
không nên bị kéo dài bởi animation trang trí.

## 3. Meta loop

```text
Chọn level → Chơi → Thắng → Mở level tiếp → Nhận thưởng tùy chọn
       ↑                                         ↓
       └──────── Chọn lại level / mode ──────────┘
```

Meta loop không được làm yếu trải nghiệm puzzle. Coin, skin hoặc quảng cáo là
lớp phụ, không quyết định kết quả mô phỏng.

## 4. Đường cong học cơ chế

Mỗi cơ chế mới nên đi qua bốn giai đoạn:

1. **Giới thiệu:** cơ chế xuất hiện một mình trong tình huống an toàn.
2. **Xác nhận:** người chơi dùng lại với một biến thể nhỏ.
3. **Kết hợp:** cơ chế được ghép với một cơ chế đã biết.
4. **Thành thạo:** level yêu cầu lập kế hoạch nhiều bước hoặc khai thác ngoại lệ.

Không giới thiệu đồng thời hai cơ chế chưa biết trong cùng level, trừ một level
được thiết kế rõ ràng như tutorial tương tác.

## 5. Nhịp độ level

- Early game: 1–4 quyết định có ý nghĩa, ít hình phạt.
- Mid game: 5–15 quyết định, bắt đầu có chuỗi trọng lực/trigger.
- Late game: nhiều bước chuẩn bị, yêu cầu nhìn trước và dùng Undo.
- Challenge: có thể dài hơn nhưng không dựa vào thao tác nhanh.

## 6. Triết lý thất bại

Thất bại phải cung cấp thông tin:

- Đầu/thân chạm hazard: animation chỉ rõ vị trí gây chết.
- Rơi khỏi board: camera và âm thanh thể hiện nguyên nhân.
- Không thể di chuyển: rung nhẹ hoặc animation blocked, không đổi state.
- Cổng khóa: phản hồi số táo còn lại.

Game không trừ mạng trong bản V2 core. Nếu sau này dùng heart/energy, nó thuộc
meta economy và cần quyết định sản phẩm mới; V2 1.0 đã khóa là không có heart/energy.

State chưa Won/Lost nhưng người chơi tự đưa puzzle vào thế không giải được không
được hệ thống tự tuyên bố Lose nếu không có bằng chứng chắc chắn. Người chơi dùng
Undo hoặc Restart; Hint phải hướng dẫn quay về checkpoint hợp lệ thay vì đoán.
