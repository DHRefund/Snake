# Specification từng màn hình

**Trạng thái:** Accepted  
**Cập nhật:** 2026-10-20

## 1. Boot

- Load profile và catalog thật; không dùng loading giả dài.
- Thành công chuyển Home sau branding transition ngắn.
- Lỗi catalog hiển thị thông báo và Retry; không vào Home với content hỏng.
- Ads/analytics không được chặn Boot.

## 2. Home

- `Play`: tiếp tục level đang chọn hợp lệ hoặc Normal 1.
- `Levels`: Level Select.
- `Cosmetics`: Shop/Cosmetics.
- `Settings`: Settings.
- Hiển thị coin và version nhỏ; không có popup tự mở.
- Android Back tại Home dùng hành vi hệ thống; không tạo navigation loop.

## 3. Level Select

- Tab Normal, Hard, Challenge luôn nhìn thấy.
- Tab mode bị khóa vẫn nhận tap để giải thích điều kiện mở, nhưng không mở grid.
- 20 level mỗi trang, giữ mode/page khi quay lại trong cùng session.
- Tile phân biệt Locked/Completed bằng text ngoài màu.
- Back/Escape về Home.

## 4. Gameplay

### Top HUD

- Pause, mode/level và objective pill.
- Normal/Hard: số apple thường còn lại.
- Challenge: `EXIT`, vì apple không khóa cổng.

### Board

- Fit bounding box content vào BoardCard; không crop tile tương tác.
- Swipe trên vùng không phải UI để di chuyển.
- D-pad là phương án điều khiển thay thế luôn sẵn trong placeholder build.

### Bottom actions

- Undo, Restart và Hint.
- Hint chỉ hiển thị một hướng kế tiếp từ verified solution.
- Nếu command history không còn là prefix của solution hoặc solution rỗng, Hint
  giải thích cần Undo/Restart hoặc hiện unavailable; không tự chơi.

### Pause và terminal modal

- Pause: Resume, Settings nhanh, Restart và Home.
- Home từ một level đang chơi cần confirmation.
- Win: move count, coin reward, Next Level/All Levels, Levels và Home.
- Lose: lý do chết, Undo nếu có, Restart và Home.
- Back/Escape đóng confirmation/settings trước, rồi đóng Pause; khi Playing thì mở Pause.

## 5. Settings

- Music, SFX, Haptics và Reduced Motion; lưu ngay.
- Privacy Options chỉ hiện khi consent service yêu cầu.
- Back/Escape về màn hình gọi. Bản hiện tại mở từ Home; quick settings trong
  Pause không rời gameplay để tránh mất session.

## 6. Cosmetics

- Coin balance, 12 skin launch và status Selected/Use/Price.
- Skin chưa có asset release phải bị tắt khỏi catalog; placeholder chỉ dùng development.
- Mua idempotent, không trừ coin hai lần; chọn skin không ảnh hưởng gameplay.
- Back/Escape về Home.

## 7. Trạng thái lỗi/empty

- Catalog/level thiếu: thông báo có ID/mode/number, Retry hoặc về Level Select.
- Ads unavailable: CTA rewarded disabled hoặc báo unavailable ngay, không chờ muộn.
- Không có Undo: button disabled và feedback `Nothing to undo` nếu command khác gọi tới.
- Không có Hint: feedback rõ, không hiển thị command đoán.
