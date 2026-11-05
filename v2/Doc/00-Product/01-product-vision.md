# Tầm nhìn và phạm vi sản phẩm

**Trạng thái:** Accepted  
**Đối tượng:** Product owner, game designer, programmer, artist, QA

## 1. Tuyên bố sản phẩm

**Snake & Apple: Puzzle Adventure** (V2) là game puzzle theo lượt trên lưới. Người chơi điều khiển một
con sâu di chuyển qua môi trường có trọng lực, ăn đủ táo và đưa đầu sâu vào cổng
thoát. Độ khó đến từ hình dạng cơ thể, thứ tự thao tác và phản ứng dây chuyền
giữa đá, công tắc, cửa, laser, teleport cùng các mối nguy.

Trò chơi phải dễ hiểu trong vài giây nhưng cho phép các level về sau tạo ra bài
toán không gian nhiều bước, có thể giải bằng suy luận thay vì phản xạ nhanh.

## 2. Định vị sản phẩm

| Thuộc tính | Quyết định |
|---|---|
| Nền tảng đầu tiên | Android |
| Orientation | Portrait |
| Thể loại | Grid puzzle theo lượt |
| Mô hình chơi | Single-player, offline-first |
| Độ dài phiên mục tiêu | 2–10 phút; một level thường 30 giây–5 phút |
| Điểm khác biệt | Cơ thể dài là một phần hình học; trọng lực và chuỗi tương tác tạo bài toán |
| Mô hình kinh doanh | Free-to-play có ads có giới hạn; cosmetic là lớp phụ |

V2 không cạnh tranh bằng phản xạ, combat hoặc nội dung live service. Giá trị chính
là một catalog puzzle lớn, luật nhất quán, thử lại nhanh và có thể kiểm chứng.

## 3. Đối tượng người chơi

- Người thích puzzle ngắn từ 30 giây đến 5 phút.
- Người chơi mobile phổ thông, không yêu cầu kỹ năng điều khiển phức tạp.
- Người thích hoàn thành level, mở khóa nội dung và giải thử thách khó hơn.
- Độ tuổi mục tiêu rộng; ngôn ngữ hình ảnh phải quan trọng hơn văn bản.

Chi tiết nhu cầu và hành trình nằm tại `05-player-experience.md`.

## 4. Trụ cột sản phẩm

### 4.1 Rõ ràng

Người chơi phải biết vì sao một hành động được hoặc không được thực hiện. Mọi
vật thể cần có hình dáng, animation và phản hồi nhất quán với chức năng.

### 4.2 Công bằng

Game không phụ thuộc may rủi trong mô phỏng level. Cùng một trạng thái và cùng
một input luôn tạo ra cùng kết quả. Không có thay đổi vật lý ngoài dự đoán.

### 4.3 Hoàn tác thân thiện

Sai lầm là một phần của quá trình giải. Undo và replay phải nhanh, không gây
trừng phạt không cần thiết và không bắt người chơi xem lại animation dài.

### 4.4 Chiều sâu từ tương tác

Mỗi cơ chế riêng lẻ đơn giản. Độ sâu được tạo bởi việc kết hợp chúng, không phải
bởi luật ẩn hoặc ngoại lệ khó nhớ.

## 5. Phạm vi phiên bản V2 1.0

### Bắt buộc

- Home, chọn level, gameplay, pause, win và lose.
- Đủ catalog 284 level: 180 Normal, 50 Hard và 54 Challenge.
- Di chuyển sâu, ăn táo, tăng chiều dài và cổng thoát.
- Tường, trọng lực, đá, gai, chất độc và teleport.
- Undo, replay, hint cơ bản và lưu tiến trình.
- Input bàn phím, chuột và cảm ứng.
- Audio, animation và phản hồi UI đủ để đọc rõ mọi state gameplay.
- Test tự động cho mô phỏng và dữ liệu level.
- Quảng cáo Rewarded và Interstitial theo policy tại `04-Systems/03-ads-iap-analytics.md`.
- Consent/privacy và khả năng chơi đầy đủ khi ads không khả dụng.

### Sau release 1.0

- IAP remove-ads.
- Skin/background theme mở rộng, achievement và leaderboard.
- Daily challenge hoặc local notification nếu có nhu cầu sản phẩm được chứng minh.
- Thêm ngôn ngữ sau khi pipeline localization đã được nghiệm thu.

### Không nằm trong V2 1.0

- Multiplayer.
- Server authoritative gameplay.
- Level editor dành cho người chơi.
- Live event phức tạp.
- Đồng bộ tài khoản đa nền tảng.

## 6. Tiêu chí thành công

### Chất lượng gameplay

- Mọi level phát hành đều có ít nhất một lời giải được xác minh.
- Không có state soft-lock ngoài những state người chơi có thể Undo hoặc Replay.
- Không có kết quả khác nhau giữa hai lần replay cùng chuỗi input.
- Input được chấp nhận hoặc từ chối với phản hồi rõ ràng.

### Chất lượng kỹ thuật

- Core simulation chạy không cần Scene, GameObject hoặc MonoBehaviour.
- Có thể chạy hàng nghìn lượt mô phỏng trong edit-mode test.
- Save data có version và có chiến lược migration.
- Ads/IAP không thể làm hỏng hoặc khóa gameplay.

### Trải nghiệm

- Level đầu tiên truyền đạt mục tiêu mà không cần đoạn hướng dẫn dài.
- Replay và Undo phản hồi gần như tức thời.
- Người chơi luôn biết level, số táo còn lại và trạng thái cổng.

Release gate và KPI quan sát được khóa chi tiết tại `04-release-scope-and-success.md`.

## 7. Chính sách tương thích với V1/APK

V1 là bản game đã phát hành và catalog level của nó là nội dung chuẩn của V2.
Không sửa level V1 để che một rule V2 chưa đúng. Khi replay chính thức chứng minh
sai khác có ảnh hưởng tới khả năng giải, ưu tiên sửa simulation V2 cho tương thích,
sau đó cập nhật GDD, test và ADR trong cùng thay đổi.

AssetRipper/decompile chỉ là bằng chứng kỹ thuật không hoàn chỉnh. Thứ tự nguồn sự
thật khi có xung đột là: hành vi có thể tái hiện từ game phát hành và solution
chính thức → dữ liệu level → GDD đã cập nhật → implementation hiện tại. Một
deviation cố ý khỏi V1 cần Product owner chấp thuận, ADR mới và kế hoạch migrate
solution/save; không được phát sinh ngầm từ code.

Các quyết định bắt buộc về turn ordering, exit, teleport, rock, spike, toxic,
laser, bomb, spider net và slide bar nằm trong
`01-Game-Design/02-game-rules.md`, `03-mechanics-catalog.md` và ADR-005.
