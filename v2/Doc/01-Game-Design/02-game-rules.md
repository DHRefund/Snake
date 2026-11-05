# Luật mô phỏng gameplay

**Trạng thái:** Accepted  
**RulesVersion:** 1  
**Mục tiêu:** định nghĩa hành vi quan sát được, độc lập với implementation

## 1. Điều kiện bắt đầu

Một level hợp lệ phải có:

- Chính xác một đầu sâu.
- Ít nhất một segment; đầu cũng được tính là một segment.
- Chính xác một cổng thoát.
- Tất cả entity nằm trong board.
- Mọi tham chiếu cặp như teleport hoặc button–door đều hợp lệ.

Level bắt đầu đúng pose đã author trong V1. Khi tải chỉ khởi tạo trigger/door cần
thiết; không chạy gravity cho rắn hoặc movable. Phản ứng môi trường đầy đủ bắt đầu
sau command hợp lệ đầu tiên.

## 2. Input di chuyển

Người chơi gửi một trong bốn hướng trực giao. Không có di chuyển chéo.

Một input bị từ chối khi:

- Đầu đi ra ngoài board.
- Ô đích là tường hoặc cửa đang đóng.
- Ô đích là bất kỳ segment nào của chính sâu, bao gồm cả tail sẽ rời đi.
- Ô đích chứa vật đẩy được nhưng vật đó không thể dịch chuyển.
- Simulation đang phát animation của một turn trước và input buffering bị tắt.

Input bị từ chối không tạo turn, không ghi undo và không thay đổi state.

## 3. Chuyển động cơ thể

Khi input hợp lệ:

1. Đầu chuyển sang ô đích.
2. Segment thứ `i` chuyển vào vị trí cũ của segment `i - 1`.
3. Tail rời vị trí cũ, trừ khi sâu vừa ăn vật phẩm tăng chiều dài.

Các segment luôn phải tạo thành một chuỗi liên thông trực giao, ngoại trừ khoảng
thời gian presentation đang tween. State logic phải cập nhật nguyên tử.

## 4. Ăn táo và tăng chiều dài

- Táo thường bị tiêu thụ khi đầu đi vào cell của nó.
- Trong turn ăn táo thường, tail giữ nguyên vị trí, làm chiều dài tăng một.
- `RemainingApples` giảm đúng một khi ăn táo thường.
- Táo thường tăng một segment. Táo VIP tăng tổng cộng hai segment (kéo dài độ dài đuôi qua 2 turn di chuyển liên tiếp) và ghi nhận một VIP collect cho reward meta khi Win.
- Apple/VIP Apple là support đối với gravity. Chúng chỉ bị thu thập khi head chủ
  động di chuyển vào cell, không bị thu thập vì thân/rắn rơi lên trên.
- Nếu head chủ động đi vào VIP Apple cùng hướng gravity, rắn không chạy gravity
  trong chính turn collect đó; gravity tiếp tục từ turn hợp lệ kế tiếp.
- Táo không respawn trong một lần chơi level.

Trong Normal/Hard, khi số táo thường về 0, cổng chuyển sang trạng thái mở. VIP
Apple không nằm trong counter bắt buộc. Challenge dùng exit mở từ đầu theo mode.
Đây là state logic, không chỉ là animation.

## 5. Cổng thoát và chiến thắng

Luật chính thức của V2:

- Trong Normal/Hard, khi còn táo thường, cổng nhìn thấy nhưng chưa thể hoàn tất level.
- Trong Challenge, head có thể hoàn tất level qua exit mà không cần thu hết apple.
- Khi hết táo, đầu đi vào cổng bắt đầu quá trình thoát.
- Toàn bộ thân lần lượt được kéo vào cổng cho presentation.
- Simulation chuyển sang `Exiting` ngay khi lệnh vào cổng được chấp nhận.
- Trong `Exiting`, segment lần lượt rời board qua cổng; không chạy gravity,
  trigger, laser hoặc hazard mới.
- `Won` được commit khi segment cuối cùng đã được đánh dấu evacuated. Presentation
  có thể animate sau commit nhưng không được thay đổi kết quả.
- Sau `Won`, không nhận thêm move command; chỉ nhận UI command.

## 6. Trọng lực

Trọng lực là một vector trực giao, mặc định `Down`. Level có thể dùng `Up`.

Sau pha di chuyển chủ động, hệ thống giải quyết trọng lực cho tới stable state:

- Toàn bộ sâu được xem là một hình cứng khi rơi.
- Snake được support nếu ít nhất một segment có solid surface ở cell kế tiếp theo
  hướng gravity, hoặc segment đang chiếm spider-net cell.
- Sâu chỉ rơi một cell nếu mọi segment đều có thể dịch theo hướng trọng lực.
- Segment khác của cùng con sâu không cản chuyển động tịnh tiến của chính nó.
- Tường, cửa đóng, slide bar, apple, VIP apple, remote và movable cản rơi.
- Rắn được resolve tới stable trước; sau đó đá/bom mới được resolve tới stable.
- Đá/bom có thể nằm trên thân rắn và dùng nó như support; tiếp xúc này không tự
  gây `Crushed` trong ruleset tương thích V1.
- Rơi ra ngoài board gây thua, trừ khi level định nghĩa boundary đặc biệt.

### Thứ tự chính thức khi nhiều vật cùng rơi

1. Dịch toàn bộ rắn từng cell tới khi được support, ra ngoài board hoặc đạt terminal state.
2. Sắp movable theo hướng gravity, vật gần đáy theo hướng rơi được xét trước.
3. Dịch từng movable hợp lệ một cell.
4. Lặp pha movable tới khi không còn thay đổi.

Entity ID/vị trí tạo tie-break ổn định; kết quả không phụ thuộc thứ tự của collection runtime.

## 7. Đẩy vật

- Đầu sâu có thể đẩy một vật `Pushable` nếu ô phía sau vật có thể nhận nó.
- Chỉ đẩy đúng một vật; chuỗi hai hay nhiều vật làm move bị từ chối.
- Push xảy ra trước khi thân sâu cập nhật.
- Nếu push thất bại, toàn bộ move bị từ chối.
- Sau push và movement, trọng lực có thể làm cả sâu và vật rơi.

Được phép đẩy theo cả bốn hướng. Vật vừa bị đẩy và snake cùng chịu gravity sau
active movement.

## 8. Hazard và cái chết

Hazard được kiểm tra sau mỗi pha có thể thay đổi vị trí:

- Sau active movement.
- Sau teleport.
- Sau mỗi gravity tick nếu cần animation theo bước.
- Sau khi laser được tính lại.
- Sau vụ nổ.

Bất kỳ segment nào chạm hazard gây thua, trừ entity có miễn nhiễm được khai báo.
State `Lost` phải chứa `DeathReason` và cell gây chết để presentation phản hồi.

## 9. Teleport

- Teleporter ghép cặp theo thứ tự pair đã author; ký tự/nhóm phải tương thích.
- Head đi vào một pocket ẩn một cell phía sau mặt portal. Thân tiếp tục đi theo
  head, vì vậy rắn có thể tạm thời bắc qua hai đầu portal.
- Khi head đang ở pocket, command kế tiếp chọn hướng đi ra tại portal cặp; không
  bắt buộc lặp lại hướng đã dùng để vào.
- Nếu cell ra theo hướng command bị chặn hoặc ngoài board, command bị từ chối nguyên tử.
- Portal không tự loop trong cùng command. Sau khi ra, pipeline tiếp tục trigger,
  gravity và hazard bình thường.
- Khi push rock/bomb vào portal, vật xuất hiện ở phía ngoài portal cặp theo cùng
  hướng push. Slide bar không teleport.

## 10. Trigger và cửa

- Button có state `Pressed` khi có ít nhất một valid presser chiếm cell.
- Valid presser gồm snake segment, rock và bomb.
- Button có cạnh nhấn khi một valid presser bắt đầu chiếm cell.
- Door liên kết mở khi button tương ứng được nhấn và giữ trạng thái mở trong lần
  chơi hiện tại; rời button không đóng lại cửa đã mở.
- Door bị explosion phá được xem là mở vĩnh viễn trong session.
- Quan hệ button–door theo danh sách/link đã author; không suy ra bằng khoảng cách.
- Button của slide bar dùng cạnh Released → Pressed và phải được nhả trước lần toggle kế tiếp.

## 11. Quy tắc kết thúc turn chính thức

```text
Validate command
→ Save snapshot
→ Push
→ Move snake / consume apple
→ Open-exit contact check; nếu Win thì kết thúc pipeline
→ Static hazard check
→ Resolve triggers theo state hiện tại
→ Resolve snake gravity tới stable; sau mỗi tick kiểm tra exit rồi hazard
→ Resolve movable gravity tới stable
→ Resolve triggers again
→ Recalculate lasers
→ Resolve remote/bombs and chain explosions
→ Resolve dynamic hazards
→ Check win/loss
→ Emit TurnCompleted
```

Thứ tự này là hợp đồng gameplay. Mọi thay đổi phải cập nhật test golden và ADR.

## 12. Quy tắc ưu tiên kết quả

Mỗi checkpoint sau khi snake đổi vị trí kiểm tra open exit trước static hazard.
Nếu head chạm exit hợp lệ, `Won/Exiting` được khóa và các phản ứng môi trường còn
lại không thể đảo kết quả. Quy tắc ưu tiên Win này áp dụng cả khi hazard và exit
cùng có thể được phát hiện trong một tick, phù hợp replay Challenge chính thức.
