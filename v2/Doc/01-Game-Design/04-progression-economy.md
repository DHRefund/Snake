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

- Skin sâu, block theme và background không thay đổi collision hoặc hitbox.
- Không dùng màu cosmetic làm mất khả năng nhận biết mechanic.
- Preview cosmetic không cần tải lại simulation.
- Giá launch cho 12 slot skin lần lượt là `0, 150, 200, 250, 300, 350, 400,
  450, 500, 550, 600, 700` coin; slot không có asset thật phải bị ẩn.

## 7. Monetization guardrails

- Không hiển thị interstitial giữa một turn hoặc trước Undo.
- Không mất reward khi quảng cáo báo thành công nhưng callback presentation lỗi.
- IAP và ads không được tham chiếu từ Core Simulation.
- Có chế độ development tắt toàn bộ monetization.
- Trẻ em và quyền riêng tư phải được xử lý theo thị trường phát hành.
- Release dùng Rewarded và Interstitial; không dùng Banner hoặc App Open Ad.
- No-ads IAP nếu bổ sung chỉ tắt Interstitial, không ẩn Rewarded tự nguyện.
