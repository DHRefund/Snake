# UI Design System

**Trạng thái:** Accepted  
**Cập nhật:** 2026-10-20

## 1. Design tokens placeholder

| Token | Giá trị | Vai trò |
|---|---|---|
| `color/background` | `#156769` | Nền chính |
| `color/background-deep` | `#0C4145` | Overlay/camera clear |
| `color/surface` | `#FFF9E2` | Card và modal |
| `color/primary` | `#FF842A` | CTA chính |
| `color/secondary` | `#73D3A6` | CTA phụ tích cực |
| `color/reward` | `#FFC537` | Coin/VIP/reward |
| `color/danger` | `#E5493F` | Hazard/Lose |
| `color/text-dark` | `#263436` | Text trên surface sáng |
| `color/text-muted` | `#5B6C6C` | Chú thích |
| `color/disabled` | 48% alpha | Disabled/locked |

Đây là token placeholder đã đủ contrast để phát triển. Art pass có thể đổi giá
trị nhưng không đổi semantic: primary, danger và reward không dùng lẫn nhau.

## 2. Typography

- Font placeholder: `LegacyRuntime.ttf` có sẵn trong Unity.
- Font cuối phải hỗ trợ tiếng Việt và localization target.
- Display: 48–78 px ở reference resolution.
- Screen title: 39 px, Bold.
- Button chính: 27–38 px, Bold.
- Body: 20–27 px.
- Caption: 17–21 px.
- Không bake text vào sprite; không co text nhỏ dưới ngưỡng đọc để nhét nội dung.

## 3. Spacing và hình học

- Base spacing: 8 px; khoảng cách chính dùng bội số 16/24/32.
- Lề nội dung reference: 60–75 px, sau đó cộng safe area.
- CTA chính rộng 610–720 px, cao 104–126 px.
- Icon button tối thiểu 92 × 92 px trên reference canvas.
- Card chính rộng tối đa 930–960 px.
- Modal card 790 × 760 px; overlay chặn raycast toàn màn hình.
- Canvas Scaler dùng reference 1080 × 1920 và Match Width/Height `0.45` để cân
  bằng điện thoại dài với tablet portrait.

## 4. Component contract

### Button

Trạng thái bắt buộc: Normal, Highlighted, Pressed, Disabled và Selected. Pressed
tối hơn Normal; Disabled giảm alpha nhưng label vẫn đọc được. Một button chỉ có
một hành động và không nhận callback lặp trong khi navigation đang chạy.

### Level tile

Hiển thị số level và một trong: Locked, Current, Completed. State phải có text/icon
ngoài màu. Tile locked vẫn cho phép giải thích điều kiện mở mode nếu phù hợp.

### Toggle

Toàn bộ row là vùng bấm. Có title, subtitle và checkmark/state rõ. Thay đổi được
lưu ngay; không cần nút Save.

### Modal

Chỉ một blocking modal ở lớp trên cùng. CTA chính nằm trên, destructive/back là
secondary. Back/Escape đóng lớp modal cao nhất trước.

### HUD pill

Dùng cho apple objective và coin. Gameplay Challenge hiển thị `EXIT` thay vì số
apple bắt buộc. Pill không được dùng làm button nếu không có affordance nhấn.

## 5. Motion

- UI transition mục tiêu 150–250 ms.
- Blocked feedback dưới 180 ms.
- Modal fade/scale dưới 220 ms.
- Reduced motion chuyển sang fade ngắn hoặc snap.
- Animation không trì hoãn commit state hoặc navigation bắt buộc.

## 6. Icon placeholder

Trong giai đoạn chưa có asset, icon Unicode/text đơn giản được chấp nhận nếu có
label hoặc ngữ cảnh rõ. Release art thay icon qua sprite/component mà không đổi
object name được controller tra cứu.
