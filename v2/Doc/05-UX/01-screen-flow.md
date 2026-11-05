# Luồng màn hình và hành vi UI

**Trạng thái:** Accepted

## 1. Sơ đồ điều hướng

```text
Boot/Splash
   ↓
Home ─────────→ Settings
 │  ├─────────→ Cosmetics
 │  └─────────→ Mode Select
 ↓
Level Select
 ↓
Gameplay ↔ Pause ↔ Quick Settings
 ├────────→ Win → Next Level / Home / Level Select
 └────────→ Lose → Undo / Replay / Home
```

Back navigation phải dự đoán được và không thoát app khi đang có modal chưa xử lý.
Orientation phát hành là portrait cố định; chi tiết responsive nằm trong
`05-responsive-portrait.md`.

## 2. Boot/Splash

### Trách nhiệm

- Load local profile và settings.
- Load level catalog.
- Khởi tạo service không chặn gameplay.
- Chuyển Home khi dependency bắt buộc sẵn sàng.

### Không nên

- Chờ ads/analytics network trước khi vào Home.
- Hiển thị loading giả lâu hơn cần thiết.
- Mất save mà không có fallback/backup.

## 3. Home

Thành phần tối thiểu:

- Play/Continue là CTA chính.
- Select Level.
- Mode hoặc Challenge khi đã mở.
- Settings.
- Cosmetic/Shop chỉ xuất hiện khi feature sẵn sàng.
- Version nhỏ ở khu vực debug/about.

`Play` mở level chưa hoàn thành cao nhất của mode hiện tại.

## 4. Level Select

- Chia page hoặc scroll, không tạo toàn bộ item nếu catalog lớn.
- Mỗi item hiển thị level number và state.
- Locked item giải thích điều kiện mở nếu được chọn.
- Completed/current có ký hiệu ngoài màu sắc.
- Giữ vị trí scroll/page khi quay lại.
- Có tab/mode selector khi nhiều mode được mở.

## 5. Gameplay HUD

### Luôn hiển thị

- Level/mode.
- Undo.
- Replay.
- Pause/Home.
- Hint khi feature được bật.

### Theo ngữ cảnh

- Số táo còn lại nếu nhiều hoặc cần nhắc.
- Tutorial arrow/text ngắn.
- Move counter nếu mode sử dụng.

HUD không che bounding box board trên mọi safe area. Nút điều hướng bằng swipe có
thể dùng song song với D-pad trong placeholder build; cả hai tạo cùng command.

## 6. Pause

- Đóng băng input gameplay và animation timeline.
- Continue.
- Replay.
- Quick Settings không rời scene gameplay.
- Settings nhanh: music/SFX/haptics.
- Home có confirmation nếu mất tiến trình turn hiện tại là quan trọng.

Không cần pause core simulation theo time scale nếu simulation theo command;
presentation coroutine/tween vẫn phải dừng hoặc snap an toàn.

## 7. Win

- Xác nhận rõ hoàn thành.
- Reward nếu có.
- Next Level là CTA chính.
- Replay và Home là secondary.
- Kết quả phải hiện tối thiểu 1 giây trước interstitial; trong khoảng này chưa bật CTA điều hướng.
- Nếu interstitial đủ điều kiện và đã preload, hiển thị trước khi bật CTA; nếu chưa Ready thì bỏ qua.
- Không bao giờ chờ tải hoặc hiện interstitial muộn sau khi người chơi đã bắt đầu điều hướng.
- Button phải hoạt động kể cả service reward/ads không sẵn sàng.

## 8. Lose

- Nêu nguyên nhân bằng hình ảnh/animation.
- Undo là CTA chính nếu history còn.
- Replay nếu không undo hoặc người chơi muốn làm lại.
- Không ép xem quảng cáo để tiếp tục core flow.

## 9. Modal priority

Thứ tự ưu tiên:

```text
System/consent
Purchase result
Fatal content error
Pause
Win/Lose
Tutorial
General message
```

Chỉ một blocking modal active. Modal manager phải quản lý queue và focus.

## 10. Safe area và responsive

- Hỗ trợ portrait là orientation chính, trừ khi product quyết định khác.
- Board fit trong vùng còn lại sau top/bottom HUD.
- Không đặt CTA trong notch/home indicator.
- Text không bị cắt ở aspect ratio dài.
- Test ít nhất 16:9, 18:9, 19.5:9, tablet và desktop window.
