# Phạm vi release và thước đo thành công

**Trạng thái:** Accepted  
**Release mục tiêu:** Snake & Apple: Puzzle Adventure 1.0  
**Cập nhật:** 2026-10-19

## 1. Mục tiêu release

Phát hành một bản Android hoàn chỉnh có thể chơi từ lúc mở ứng dụng đến khi hoàn
thành toàn bộ catalog mà không cần Editor, debug menu, mạng hoặc thanh toán.
V2 tái tạo trải nghiệm và content đã phát hành của V1 trên một codebase có thể
kiểm thử, bảo trì và thay presentation độc lập.

## 2. Phạm vi P0 bắt buộc

### Gameplay và content

- Đủ 284 level: 180 Normal, 50 Hard, 54 Challenge.
- Mọi level tải được, có thể thắng và không phụ thuộc timing animation.
- Đủ mechanic được sử dụng trong catalog V1.
- Undo, Restart, Hint, Pause, Win, Lose và Next Level hoạt động xuyên suốt.
- Progress được lưu riêng cho từng mode và giữ nguyên sau khi đóng ứng dụng.

### Luồng sản phẩm

- Boot, Home, Mode Select, Level Select, Gameplay, Pause, Settings, Win và Lose.
- Cosmetics có 12 skin rắn launch (1 mặc định, 11 mua bằng coin), cùng block theme và
  background mua bằng coin (xem `01-Game-Design/04-progression-economy.md` mục 6). Release
  phải có asset thật cho mọi slot đang bật; slot chưa có asset bị ẩn.
- Không có nút placeholder, màn hình cụt hoặc chức năng chỉ chạy từ Editor.

### Presentation tối thiểu để phát hành

- Mọi mechanic phân biệt được bằng hình dạng/ký hiệu, không chỉ bằng màu.
- UI tương thích safe area và các tỷ lệ màn hình mục tiêu.
- Có phản hồi nhìn thấy được cho input hợp lệ, input bị chặn, collect, hazard, Win và Lose.
- Music/SFX/haptics có thể tắt riêng; reduced motion được lưu.

### Thương mại và tuân thủ

- Rewarded và Interstitial tuân theo policy đã khóa; gameplay vẫn đầy đủ khi ads lỗi hoặc bị từ chối.
- Rewarded coin chỉ bật khi Cosmetics/Shop có nội dung thật để tiêu; không phát hành
  một reward không có công dụng.
- Consent/privacy, store metadata và data-safety được hoàn tất trước release.
- Release build không chứa test ad ID, debug control hoặc log nhạy cảm.

## 3. P1 sau release

- IAP `remove_ads`.
- Thêm skin, block theme hoặc background ngoài danh sách launch.
- Cloud save, achievement hoặc leaderboard.
- Daily challenge/live content.
- Thêm ngôn ngữ sau khi pipeline localization đã ổn định.

P1 không được để lại UI chết trong bản 1.0. Feature chưa hoàn thiện phải bị tắt bằng config.

## 4. Ngoài phạm vi

- Multiplayer và server-authoritative gameplay.
- Level editor cho người chơi.
- PvP, energy/heart, loot box hoặc gacha.
- Pay-to-win, bán Undo, bán lời giải hoặc khóa campaign bằng quảng cáo.
- Thay đổi 284 level chỉ để làm validator V2 vượt qua.

## 5. Thước đo chất lượng bắt buộc

Đây là release gate, không phải mục tiêu marketing tùy chọn:

| Chỉ số | Ngưỡng chấp nhận |
|---|---:|
| Catalog có thể tải | 284/284 |
| Level có lời giải được xác minh | 284/284 |
| Golden replay qua engine hiện hành | 284/284 |
| Exception trong batch replay | 0 |
| Core flow cần Editor/debug | 0 bước |
| Critical/blocker bug mở | 0 |
| Mất progression trong save recovery test | 0 case |
| Gameplay bị khóa khi offline/ads lỗi | 0 case |

## 6. KPI quan sát sau phát hành

KPI dùng để học và điều chỉnh onboarding/pacing, không được dùng để âm thầm đổi rule:

- Tutorial completion rate.
- Tỷ lệ bắt đầu/hoàn thành theo level và mode.
- Số lần Undo, Restart và Hint trên mỗi level.
- Thời gian hoàn thành và điểm rơi bỏ cuộc theo level.
- Crash-free sessions và ANR-free sessions.
- Rewarded opt-in và Interstitial impressions trên mỗi phiên.
- Retention D1/D7 khi có đủ dữ liệu hợp lệ và consent phù hợp.

Không đặt ngưỡng KPI kinh doanh giả khi chưa có baseline người dùng. Sau soft launch,
Product owner tạo một phụ lục target dựa trên dữ liệu thật, không sửa tài liệu gameplay.

## 7. Trạng thái hiện tại của content validation

Tại ngày 2026-10-19, replay validator đạt **276/284**:

- Normal: 172/180.
- Hard: 50/50.
- Challenge: 54/54.
- 0 exception.

Tám Normal level còn lại được ghi chi tiết tại
`../01-Game-Design/05-level-design-and-content.md`. Đây là known implementation
backlog và là release blocker; không phải lý do giảm phạm vi 284 level.
