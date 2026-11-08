# Ads, IAP và Analytics

**Trạng thái:** Accepted  
**Phạm vi phát hành:** Ads là bắt buộc; IAP là mở rộng sau phát hành; analytics tối thiểu là bắt buộc.

## 1. Quyết định đã khóa

- Target thương mại đầu tiên là Android.
- Nhà cung cấp quảng cáo đầu tiên là Google AdMob qua Google Mobile Ads Unity Plugin.
- Consent dùng Google User Messaging Platform (UMP).
- Release đầu tiên dùng Banner adaptive, Rewarded và Interstitial.
- Không dùng App Open, Native hoặc Rewarded Interstitial.
- Không chép các DLL quảng cáo trích từ APK vào V2; cài package chính thức với version được pin.
- Gameplay hoạt động đầy đủ khi mất mạng, người dùng không consent hoặc Ads SDK lỗi.
- Không quảng cáo nào thay đổi luật puzzle, mở level chính, cấp Hint hay cấp Undo.

Việc chọn AdMob là quyết định adapter, không làm Application/UI phụ thuộc API Google. Có thể
thay nhà cung cấp sau này bằng adapter khác mà không đổi Core Simulation.

## 2. Placement chính thức

### 2.0 Banner Gameplay

- Chỉ đặt Banner ở đáy màn Gameplay portrait, nằm trong safe area và có khoảng cách 16 dp với vùng điều khiển.
- Dùng anchored adaptive theo chiều rộng an toàn và chiều cao SDK trả về. Layout dành vùng riêng ngay từ lúc tải quảng cáo.
- Khi mở modal Pause/Settings/Confirm/Win/Lose hoặc rời Gameplay, ẩn hoặc hủy banner. Giữ vùng bố cục khi đang ở Gameplay để tránh nhảy board và nút.
- Banner lỗi không chặn gameplay; thử lại sau 30 giây. Thay đổi kích thước hoặc safe area tạo lại banner phù hợp.
- Hiện tại tích hợp Google Mobile Ads Unity 11.5.0 qua OpenUPM, sử dụng sample App ID và adaptive Banner Ad Unit ID chính thức cho Android/iOS. Đây là cấu hình thử nghiệm, chưa tạo doanh thu.
- Editor hiển thị khung `TEST BANNER` để duyệt layout; quảng cáo SDK cần kiểm tra trên thiết bị Android/iOS.
- `GameplayBanner` sở hữu vòng đời banner trong scene, `SafeAreaPanel` nhận số pixel cần dành ở đáy. Domain không phụ thuộc SDK.

### 2.1 Rewarded

Rewarded là hành động hoàn toàn tự nguyện:

- Placement: nút `Xem quảng cáo nhận 50 coin` tại Shop/Cosmetics và Win screen.
- Reward cố định: `50 softCurrency` cho một reward callback hợp lệ.
- Tối đa 5 reward được cấp trong một cửa sổ 24 giờ; UI hiển thị số lượt còn lại.
- Không hiện nút nếu đã đạt cap; nếu ad chưa sẵn sàng, nút disabled kèm trạng thái ngắn.
- Không dùng Rewarded cho Hint, Undo, Replay, Continue, Skip hoặc mở level chính.
- Người chơi đóng ad sớm hoặc ad lỗi thì không nhận coin và quay lại đúng màn hình trước đó.
- No-ads entitlement trong tương lai không ẩn Rewarded vì đây là lựa chọn chủ động.

Trạng thái triển khai: `RewardedAdController` (Presentation/Ads) và `RewardLedger` (Infrastructure/Save)
đã có, đang dùng test ad unit của Google. Nút `WATCH AD +50` nằm trên màn Cosmetics; Win screen chưa có.
Editor mô phỏng một quảng cáo thành công sau 1 giây vì SDK không chạy trong Editor. Cap 24 giờ dùng
đồng hồ hệ thống UTC nên người chơi chỉnh giờ máy có thể lách; chấp nhận được cho giá trị 50 coin,
cần server time nếu sau này coin có giá trị tiền thật.

Reward amount và cap nằm trong versioned local config. Remote config có thể giảm tần suất hoặc
tắt placement, nhưng không được tăng vượt giới hạn release nếu chưa qua QA.

### 2.2 Interstitial

Interstitial chỉ xuất hiện tại điểm nghỉ tự nhiên sau khi thắng level:

1. Win screen xuất hiện và kết quả/reward level đã được commit.
2. Giữ tối thiểu 1 giây không có CTA điều hướng để tránh accidental tap.
3. Nếu placement đủ điều kiện và ad đã preload ở trạng thái Ready, hiển thị ad.
4. Khi ad đóng/lỗi, bật `Next Level`, `Replay`, `Home` ngay.
5. Nếu ad chưa Ready, bỏ qua; tuyệt đối không chờ load và không hiện muộn ở màn sau.

Điều kiện đồng thời:

- Không nằm trong 5 level Normal đầu.
- Không phải tutorial và không phải sau thất bại.
- Đã có ít nhất 3 level hoàn thành kể từ interstitial gần nhất.
- Cách lần interstitial gần nhất ít nhất 120 giây theo monotonic session clock.
- App không vừa resume trong 30 giây.
- Không có purchase/consent/system modal và không có entitlement no-ads.

Không hiện interstitial sau thao tác Undo, Restart, Pause, Back hoặc ngay khi mở app.

## 3. Consent và khởi động

Mỗi lần app launch:

```text
Load profile/settings
→ UMP consent info Update
→ LoadAndShowConsentFormIfRequired
→ kiểm tra CanRequestAds
→ nếu được phép: initialize Mobile Ads một lần và preload
→ nếu không/timeout/lỗi: dùng NullAdsService cho phiên hiện tại
→ vào Home không chờ mạng quảng cáo
```

- Boot không chặn quá 3 giây vì consent/ads; form hệ thống đang hiển thị không tính là loading giả.
- Settings có `Privacy options` khi UMP báo entry point là bắt buộc.
- Không yêu cầu personalized ads nếu consent không cho phép; adapter tuân theo kết quả UMP.
- Cờ child-directed/under-age phải được cấu hình trước khi SDK có thể tải quảng cáo.
- Product owner phải khai báo chính xác độ tuổi mục tiêu và store data-safety trước release.

## 4. Vòng đời và trạng thái quảng cáo

Mỗi placement có state riêng:

```text
Disabled → Idle → Loading → Ready → Showing → Idle
                       ↘ Failed ────────────↗
```

- Chỉ giữ một ad object Ready cho mỗi format.
- Preload interstitial sau khi consent cho phép và sau mỗi lần ad đóng.
- Preload rewarded khi vào Home/Shop, không preload lặp vô hạn.
- Load timeout 10 giây; exponential backoff 5, 15, 30, 60 giây, tối đa 60 giây.
- Một show request chỉ được chấp nhận khi state là Ready.
- Callback SDK được marshal về Unity main thread trước khi đổi UI/state.
- Khi full-screen ad mở: khóa input game, pause presentation, mute/duck game audio.
- Khi đóng/lỗi: restore audio, focus, time scale và input đúng một lần.
- App pause/resume trong lúc ad mở không được tạo show thứ hai hoặc kẹt modal.

## 5. Reward transaction

Mỗi lần show Rewarded tạo một `adAttemptId` UUID phía game. SDK callback không được giả định có
transaction ID ổn định.

```text
User tap Rewarded
→ kiểm tra cap + Ready
→ tạo và lưu adAttemptId trạng thái Pending
→ Show
→ SDK reward callback
→ RewardLedger.TryCommit(adAttemptId, 50 coin)
→ atomic save
→ cập nhật UI
→ ad close callback kết thúc presentation
```

- Reward callback là nguồn duy nhất cấp thưởng; close callback không cấp thưởng.
- `TryCommit` idempotent: callback lặp không cấp hai lần.
- Nếu reward callback tới trước close callback, thưởng vẫn được save ngay.
- Nếu save lỗi, transaction giữ PendingGranted và retry; UI không tự cấp lần thứ hai.
- Ledger giữ tối đa 100 attempt gần nhất và tổng hợp timestamp grant để tính cap 24 giờ.
- Pending attempt chưa có reward callback khi app bị kill không được tự cấp thưởng lúc khởi động.

## 6. Build configuration và test ads

| Build | Ad unit | Hành vi |
|---|---|---|
| Editor/Development | Google sample test ID hoặc FakeAdsService | Không bao giờ dùng production ID |
| QA device | Test device + ad unit QA | Kiểm tra callback/focus/mediation |
| Release | Production ID từ protected build config | Tắt debug UI và test-device override |

- App ID/ad unit ID không hard-code trong prefab hay Core.
- CI/release validation fail nếu Release vẫn dùng sample ID, ID rỗng hoặc Development dùng production ID.
- Tester chỉ click quảng cáo có nhãn Test Ad; mediation phải bật test mode riêng cho từng network.
- Không commit secret, receipt hoặc dữ liệu người dùng vào repository/log.

## 7. IAP

IAP không bắt buộc cho release quảng cáo đầu tiên. Kiến trúc vẫn dành `IPurchaseService` để bổ
sung sản phẩm `remove_ads` sau này.

Khi bật `remove_ads`:

- Tắt Banner và Interstitial; Rewarded tự nguyện vẫn còn.
- Giá lấy từ store, UI không hard-code.
- Purchase pending có UI riêng.
- Restore purchase khả dụng ở platform yêu cầu.
- Entitlement được restore từ store, không chỉ dựa vào local flag.
- Receipt/log nhạy cảm không xuất ra production console.

## 8. Analytics taxonomy

Tên event dùng lowercase snake_case. Không gửi board state, replay, advertising ID hoặc PII.

### Funnel gameplay

```text
app_open
tutorial_start
tutorial_complete
level_start
level_complete
level_fail
mode_unlocked
```

### Ads

```text
ad_load_requested(format, placement)
ad_load_result(format, placement, result, error_category)
ad_show_requested(format, placement)
ad_impression(format, placement)
ad_show_result(format, placement, result, error_category)
ad_reward_granted(placement, reward_type, reward_amount)
```

Không gửi raw SDK error string nếu nó có thể chứa identifier; map sang `error_category` ổn định.
Analytics là quan sát, không phải nguồn cấp reward hay điều kiện cho flow gameplay.

## 9. Test doubles và acceptance matrix

Phải có:

- `NullAdsService`.
- `FakeAdsService`: ready, unavailable, load fail, show fail, cancel, delayed/duplicate callbacks.
- `FakeConsentService`: required, not required, denied, timeout và privacy-options-required.
- `FakePurchaseService`: pending, success, restore và duplicate callback.
- `InMemoryAnalyticsService` để assert event.

Các case bắt buộc:

- Offline từ lúc boot vẫn vào Home và chơi được.
- Interstitial không xuất hiện trong 5 level đầu, tutorial hoặc sau Lose.
- Không vượt frequency cap 3 level/120 giây.
- Ad chưa Ready bị skip, không xuất hiện muộn.
- Duplicate reward callback chỉ cộng 50 coin một lần.
- Close không có reward callback không cấp coin.
- Kill/pause/resume trong full-screen ad không kẹt input/audio/modal.
- No-ads chặn Banner và Interstitial nhưng không chặn Rewarded tự nguyện.
- Consent denied vẫn chơi đầy đủ; Privacy options mở lại được khi bắt buộc.
- Release build không chứa sample test ad unit ID.

## 10. Nguồn chính sách triển khai

- [Google Mobile Ads Unity quick start](https://developers.google.com/admob/unity/quick-start)
- [UMP consent cho Unity](https://developers.google.com/admob/unity/privacy)
- [Google test ads cho Unity](https://developers.google.com/admob/unity/test-ads)
- [AdMob interstitial implementation guidance](https://support.google.com/admob/answer/6201350)

Các version SDK/API cụ thể được pin lúc implementation và kiểm tra lại trước mỗi release; tài liệu
này khóa hành vi sản phẩm, không khóa một version package vĩnh viễn.
