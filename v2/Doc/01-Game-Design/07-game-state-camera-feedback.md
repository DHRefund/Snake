# Game state, camera và feedback

**Trạng thái:** Accepted  
**Cập nhật:** 2026-10-19

## 1. State của một gameplay session

```text
Loading → Ready → Playing ↔ Paused
                    ├→ Won
                    └→ Lost
Won/Lost → Restart | Next Level | Level Select | Home
```

- `Loading`: chưa nhận gameplay input.
- `Ready`: board đã hiển thị đúng initial state; chuyển Playing khi transition kết thúc.
- `Playing`: nhận tối đa một semantic move tại một thời điểm.
- `Paused`: simulation không đổi; presentation dừng/snap an toàn.
- `Won/Lost`: khóa move input, chỉ nhận UI command.
- Quảng cáo full-screen là presentation overlay, không phải gameplay state mới.

## 2. Input và animation

- Keyboard, swipe và D-pad đều tạo cùng `MoveCommand` bốn hướng.
- Không buffer input trong V2 1.0; input mới trong lúc turn đang animate bị bỏ qua
  có phản hồi nhẹ, không được commit muộn.
- Logic commit nguyên tử trước; view phát event theo đúng thứ tự đã commit.
- Reduced motion rút ngắn/snap tween nhưng không đổi lúc command tiếp theo được nhận.

## 3. Camera

- Camera trực giao, không xoay theo gravity và không đổi ý nghĩa Up/Down.
- Khi vào level, camera fit toàn bộ board trong vùng còn lại sau HUD và safe area.
- Board lớn được scale đồng đều; không crop tile có thể tương tác.
- Camera shake chỉ là feedback tùy chọn, biên độ nhỏ và bị tắt bởi reduced motion.
- Win/Lose có thể focus ngắn vào nguồn sự kiện nhưng phải trả lại quyền điều hướng nhanh.

## 4. Feedback bắt buộc

| Sự kiện | Visual | Audio/Haptic |
|---|---|---|
| Move hợp lệ | Tween một cell và phản ứng môi trường theo thứ tự | SFX bước nhẹ |
| Move bị chặn | Nudge/squash, không đổi cell | Tick/rung nhẹ tùy setting |
| Ăn apple | Collect + growth đọc được | SFX collect |
| Exit mở | Đổi silhouette/state rõ | SFX mở cổng |
| Teleport | Entry → exit ngắn, đọc được hướng ra | SFX riêng |
| Trigger/door/slide | Highlight quan hệ và state mới | Click/cơ khí |
| Hazard/Lose | Chỉ rõ segment và nguồn chết | SFX/haptic có thể tắt |
| Win | Board xác nhận trước celebration | Jingle ngắn |
| Undo | Snap/reverse nhanh tới snapshot | SFX trung tính |

Không có event quan trọng chỉ được truyền bằng âm thanh, màu hoặc haptic.

## 5. HUD contract

- Hiển thị mode/level, Undo, Restart, Pause và Hint khi khả dụng.
- Hiển thị mục tiêu còn lại theo semantics của mode; Challenge không được hiển thị
  apple như điều kiện bắt buộc nếu exit của mode không yêu cầu hết apple.
- HUD không che board và không đổi layout giữa turn.
- Win/Lose modal chỉ xuất hiện sau khi feedback terminal đủ đọc, không chờ network.
