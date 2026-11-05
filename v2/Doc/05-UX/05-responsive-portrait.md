# Responsive portrait và safe area

**Trạng thái:** Accepted  
**Cập nhật:** 2026-10-20

## 1. Orientation

- Portrait cố định cho Android release.
- Reference canvas: 1080 × 1920, `Scale With Screen Size`, Match Width/Height `0.45`.
- Landscape chỉ phục vụ Editor/debug; không có layout release riêng.
- Không tự xoay khi thiết bị nghiêng.

## 2. Vùng dọc

```text
┌────────────────────────┐
│ Safe area / Top HUD    │
├────────────────────────┤
│                        │
│  Flexible board/content│
│                        │
├────────────────────────┤
│ Actions / D-pad        │
│ Safe area bottom       │
└────────────────────────┘
```

HUD trên và dưới có kích thước ổn định theo canvas. Board nhận phần linh hoạt và
fit theo cạnh giới hạn nhỏ hơn. Không phóng tile vượt card và không crop board.

## 3. Aspect ratio matrix

| Tỷ lệ | Hành vi |
|---|---|
| 16:9 | Layout chuẩn, board đạt kích thước lớn nhất |
| 18:9–20:9 | Tăng khoảng thở dọc; không kéo giãn component |
| Tablet portrait | Giới hạn chiều rộng card; thêm khoảng trống hai bên |
| Notch/punch-hole | SafeAreaPanel inset toàn bộ CTA/HUD |
| Landscape Editor | Giữ toàn composition để debug, không dùng làm tiêu chí art |

## 4. Board fitting

1. Tính bounding box của mọi tile/entity/platform có ý nghĩa.
2. Thêm một cell padding nếu còn chỗ.
3. Tính cell size theo cả chiều rộng và chiều cao BoardGrid.
4. Chọn kích thước nhỏ hơn, giữ cell vuông.
5. Căn giữa board; không thay đổi tọa độ hoặc rule simulation.

Nếu cell dưới ngưỡng đọc trên thiết bị baseline, ưu tiên giảm padding/decorative
space. Pan/zoom chỉ được thêm sau device test và không được che thông tin puzzle.

## 5. Device acceptance

- 1080 × 1920.
- 1080 × 2160.
- 1080 × 2340/2400.
- Tablet portrait 1600 × 2560.
- Safe-area simulation có notch trên và home indicator dưới.

Ở mọi case: không overlap CTA, không cắt text, toàn board nhìn thấy và touch target
không nhỏ hơn ngưỡng accessibility.
