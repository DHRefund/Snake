# Input, Undo, Replay và Hint

**Trạng thái:** Accepted

## 1. Input abstraction

Mọi nguồn input chuyển thành command semantic:

```text
KeyboardInput ─┐
TouchInput ────┼→ PlayerCommandSource → GameSession
UIButtonInput ─┘
```

Simulation không biết command đến từ bàn phím, swipe hay bot.

## 2. Mapping mặc định

| Action | Keyboard | Touch/UI |
|---|---|---|
| Move up | W / Up Arrow | Swipe up / nút Up |
| Move down | S / Down Arrow | Swipe down / nút Down |
| Move left | A / Left Arrow | Swipe left / nút Left |
| Move right | D / Right Arrow | Swipe right / nút Right |
| Undo | Z / Backspace | Nút Undo |
| Restart | R | Nút Replay |
| Pause | Escape | Nút Pause |

## 3. Swipe recognition

- Ghi pointer down position và thời điểm.
- Chỉ nhận swipe khi displacement vượt threshold theo DPI hoặc screen fraction.
- Chọn trục có độ lớn tuyệt đối lớn hơn.
- Tap ngắn không tạo move.
- Swipe bắt đầu trên UI control không được truyền xuống gameplay.
- Không dùng velocity làm luật gameplay.

## 4. Input state

Input gameplay được bật khi GameSession ở `ReadyForCommand`. Trong `Resolving`:

- Có thể buffer tối đa một move direction.
- Undo/Restart là priority command, cancel presentation timeline.
- Pause luôn được nhận.
- UI navigation không bị khóa theo simulation.

## 5. Undo semantics

- Chỉ accepted turn được ghi history.
- Undo phục hồi state ngay trước accepted turn gần nhất.
- Rejected input không tiêu hao Undo.
- Undo sau Won được phép trước khi chuyển level, nếu result screen chưa commit
  progression hoặc progression completion có thể idempotent.
- Undo sau Lost phục hồi state trước turn gây chết.
- Undo không đảo ngược transaction meta như purchase/reward.

## 6. Replay

Hai nghĩa cần phân biệt:

- **Restart:** quay về initial BoardState và xóa undo history.
- **Recorded replay:** phát lại command sequence để debug/hint/QA.

Replay player có chế độ:

- Step-by-step.
- Real-time presentation.
- Headless fast-forward để validate.

## 7. Hint

### Hint source

Ưu tiên solution command sequence đã validate cùng level. Hint engine so sánh
state hiện tại với state trên solution path.

### Khi người chơi chưa lệch lời giải

Trả command tiếp theo và highlight hướng/cell liên quan.

### Khi người chơi đã lệch lời giải

Các mức triển khai:

1. MVP: thông báo Undo tới state khớp solution hoặc Restart.
2. Nâng cao: solver tìm đường từ current state.
3. Fallback: trình bày mục tiêu cục bộ thay vì command sai.

Không giả vờ hint hợp lệ nếu state không nằm trên path đã biết.

## 8. Hint presentation

- Highlight head và hướng đi.
- Có arrow animation nhẹ.
- Không tự thực hiện move trừ khi người chơi xác nhận.
- Reduced Motion dùng pulse màu/tĩnh thay animation.
- Hint lock chỉ phục vụ tutorial bắt buộc, không dùng cho level thông thường.

## 9. Recording solution

Tool nội bộ ghi accepted commands khi designer chơi:

1. Load canonical level.
2. Bắt đầu record từ initial state.
3. Ghi command accepted.
4. Khi Won, replay headless để xác minh.
5. Lưu command sequence + rulesVersion + contentHash.
