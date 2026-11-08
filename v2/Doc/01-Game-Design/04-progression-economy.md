# Progression, mode và economy

**Trạng thái:** Accepted

## 1. Progression chính

- Hoàn thành level `N` mở level `N + 1` trong cùng mode.
- Người chơi luôn có thể chơi lại level đã mở.
- Không khóa progression chính bằng coin, quảng cáo hoặc IAP.
- Save lưu `highestUnlockedLevel`, không chỉ `currentLevel`.
- Level select hiển thị Locked, Unlocked, Current và Completed.
- Normal mở từ đầu. Hard mở sau khi hoàn thành Normal 180.
- Challenge mở sau khi hoàn thành Hard 50.
- Mỗi mode lưu `highestUnlockedLevel` và completed set riêng.

## 2. Các mode

### Normal

- Đường học cơ chế chính.
- Difficulty tăng dần.
- Tutorial được tích hợp vào level đầu của mỗi mechanic.

### Hard

- Dành cho người đã hiểu toàn bộ mechanic cốt lõi.
- Ít hint hơn, nhiều tương tác kết hợp hơn.
- Mở khóa khi hoàn thành toàn bộ Normal, không trả tiền.

### Challenge

- Các bài toán độc lập hoặc checkpoint.
- Exit mở từ đầu; apple trong Challenge không phải điều kiện bắt buộc để Win.
- Không giới hạn số lượt; chỉ ghi personal best.
- Reward coin theo cùng công thức first-completion của các mode khác.

## 3. Chấm điểm

V2 core không có hệ thống sao. Game vẫn ghi `bestMoves` phục vụ thống kê và có
thể hiển thị personal best, nhưng không dùng nó để khóa level hoặc reward.

## 4. Currency

Một currency mềm duy nhất là đủ cho V2:

- Nhận `10 coin` khi hoàn thành một level lần đầu.
- Mỗi Apple VIP đã thu thập cộng thêm `5 coin` vào lần hoàn thành đầu tiên của level;
  replay level đã nhận reward không farm lại coin.
- Nhận thêm `50 coin` từ mỗi Rewarded Ad hợp lệ, tối đa 5 grant trong cửa sổ 24 giờ.
- Chỉ dùng mua cosmetic.
- Không mua quyền vượt qua luật puzzle bắt buộc.

Mọi thay đổi balance phải nằm trong config, không dùng magic number trong code.

## 5. Booster

### Undo

Miễn phí và không giới hạn. Undo là công cụ suy luận, không phải monetization.

### Hint

- Miễn phí và không giới hạn trong V2.
- Mỗi lần chỉ chỉ ra một bước tiếp theo, không tự động chơi toàn bộ lời giải.
- Hint không dùng coin, reward ad hoặc cooldown.

### Skip

- V2 không có Skip Level. Mọi level progression chính phải được hoàn thành.

## 6. Cosmetic

Ba nhóm cosmetic đều mua bằng coin, không đổi collision hoặc hitbox, không dùng màu làm mất
khả năng nhận biết mechanic. Preview không cần tải lại simulation. Mục đầu tiên của mỗi nhóm
(index 0) miễn phí và luôn được mở. Index là giá trị lưu trong save nên chỉ được thêm vào cuối,
không đổi thứ tự. Mục chưa có asset thật bị ẩn khỏi shop nhưng vẫn giữ chỗ trong catalog.

### 6.1 Skin rắn (10 slot, skin V1 đã phối màu lại)

| Slot | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |
|---|---|---|---|---|---|---|---|---|---|---|
| Skin | nemo | dog | bird | frog | tiger | bear | unicorn | dragon | sorcerer | thunder |
| Giá (coin) | 0 | 150 | 200 | 250 | 300 | 350 | 450 | 550 | 600 | 700 |

Bảng màu mới và cách đổi nằm ở `Doc/08-Appendix/02-snake-skins-v1-to-v2.md`. Skin cat và classic do V2 tự làm
không còn trong catalog (asset vẫn còn trong `Resources/SnakeSkins` cho test).

### 6.2 Block theme

Mỗi theme là một bộ 48 sprite autotile (16 mask x 3 variant). Theme chỉ xuất hiện trong shop khi
đủ 16 mask. Block theme áp dụng cho mọi level, không gắn với chapter.

| Index | Theme | Giá (coin) |
|---|---|---|
| 0 | Frosty Ice (`ice`) | 0 |
| 1 | Sandstone (`desert`) | 200 |
| 2 | Mossy Wood (`forest`) | 300 |
| 3 | Volcano (`lava`) | 400 |
| 4 | Candy Land (`candy`) | 500 |
| 5 | Midnight (`night`) | 600 |

### 6.3 Background

Background thay ảnh nền của màn Gameplay; các màn khác giữ nền riêng.

| Index | Background | Giá (coin) |
|---|---|---|
| 0 | Iceberg (`iceberg`) | 0 |
| 1 | Desert Day (`desert`) | 150 |
| 2 | Sky Clouds (`clouds`) | 250 |

### 6.4 Cơ sở định giá

- Block theme định giá 200-600, background 150-250: một block theme tương đương một skin rắn
  tầm trung, background rẻ hơn vì chỉ đổi nền. Cả hai đều thấp hơn skin đắt nhất (700).
- Nguồn coin: 10 coin mỗi level lần đầu, cộng 5 mỗi Apple VIP, cộng Rewarded tối đa
  `5 x 50 = 250 coin` mỗi 24 giờ.
- Ví dụ: người chơi xong 20 level đầu (khoảng 200-300 coin) mua được một background hoặc một block
  theme rẻ. Mua toàn bộ block theme (2000) và background (400) cần khoảng 2400 coin, tương đương
  10 ngày xem đủ ad hoặc vài trăm level.
- Toàn bộ giá nằm trong catalog (`ThemeCatalog`, `SnakeSkinCatalog`), chỉnh giá không cần sửa code.
  Giá là giá trị khởi điểm, cần điều chỉnh theo dữ liệu thực tế (tỷ lệ mua, tốc độ kiếm coin).

## 7. Monetization guardrails

- Không hiển thị interstitial giữa một turn hoặc trước Undo.
- Không mất reward khi quảng cáo báo thành công nhưng callback presentation lỗi.
- IAP và ads không được tham chiếu từ Core Simulation.
- Có chế độ development tắt toàn bộ monetization.
- Trẻ em và quyền riêng tư phải được xử lý theo thị trường phát hành.
- Release dùng Banner adaptive ở đáy Gameplay, Rewarded và Interstitial; không dùng App Open Ad.
- Banner có vùng riêng trong safe area, cách nút điều khiển tối thiểu 16 dp; không đè lên board hoặc CTA.
- Banner ẩn khi mở Pause, Settings trong Pause, xác nhận rời màn, Win hoặc Lose; vùng bố cục vẫn được giữ để tránh nhảy UI.
- No-ads IAP nếu bổ sung tắt Banner và Interstitial, không ẩn Rewarded tự nguyện.
