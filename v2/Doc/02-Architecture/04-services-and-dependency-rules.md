# Services và quy tắc phụ thuộc

**Trạng thái:** Accepted

## 1. Service ports

Application phụ thuộc interface nhỏ:

```text
ILevelRepository
ISaveService
IAudioService
IHapticsService
IAdsService
IPurchaseService
IAnalyticsService
IClock
```

SDK cụ thể được bọc trong adapter. Không truyền object SDK vào GameSession/UI.

## 2. Save service

- API bất đồng bộ dù local save nhanh, để hỗ trợ platform khác sau này.
- Atomic write: ghi file tạm, validate, rồi replace.
- Có backup gần nhất.
- Migration theo `schemaVersion`.
- Không lưu BoardState giữa turn trong milestone đầu; lưu progression/settings.

## 3. Audio service

- Nhận semantic cue như `AudioCue.AppleEaten`, không nhận AudioClip từ Domain.
- Presentation mapping cue → clip/theme.
- Music, SFX và haptics có setting riêng.
- Ads có thể yêu cầu duck/pause audio qua Application, không tự chỉnh AudioSource.

## 4. Ads/IAP

- Chỉ khởi tạo sau consent phù hợp.
- Failure hoặc không có mạng phải trả kết quả hữu hạn, không treo UI.
- Reward dùng transaction ID/idempotency để tránh cấp hai lần hoặc mất thưởng.
- Core gameplay hoàn toàn hoạt động khi service là `NullAdsService`.

## 5. Analytics

Event analytics chính thức:

```text
level_start(level_id, mode, attempt)
turn_committed(level_id, turn_index)        // chỉ development nếu quá nhiều
level_fail(level_id, reason, move_count)
level_complete(level_id, move_count, time)
hint_used(level_id, hint_index)
undo_used(level_id, count)
```

Không gửi command sequence hoặc dữ liệu nhận dạng nếu chưa có mục đích và consent.

## 6. Quy tắc cấm

- Domain không gọi `PlayerPrefs`, `Resources.Load` hoặc `Debug.Log`.
- View không gọi ads, purchase hoặc save trực tiếp.
- Service không chuyển GameFlow tùy ý.
- Không dùng static mutable state cho current level, currency hoặc settings.
- Không tìm dependency bằng `FindObjectOfType` trong runtime production.
- Không đặt gameplay rule trong animation event.

## 7. Logging

Sử dụng `IGameLogger` ở Application/Infrastructure với category:

```text
Simulation
LevelLoading
Save
Presentation
Ads
Purchase
Analytics
```

Production log không chứa thông tin cá nhân, receipt đầy đủ hoặc secret.

## 8. Time và random

- Domain không đọc system clock.
- Daily reward nhận thời gian qua `IClock` và cần chính sách chống clock rollback.
- Random reward dùng `IRandomSource` có seed, nhưng puzzle simulation không random.
