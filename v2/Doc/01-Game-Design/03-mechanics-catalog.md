# Danh mục cơ chế gameplay

**Trạng thái:** Accepted

Tài liệu này là catalog cấp thiết kế. Chi tiết thuật toán nằm trong Technical
Design và test case.

## 1. Snake

### Vai trò

Entity do người chơi điều khiển, vừa là nhân vật vừa là hình học puzzle.

### State

- Danh sách segment có thứ tự từ head tới tail.
- Alive, Won hoặc Dead.
- Skin ID chỉ thuộc presentation.

### Tương tác

- Ăn táo để tăng chiều dài.
- Đè button bằng bất kỳ segment nào.
- Đẩy rock/bomb bằng head.
- Chết khi bất kỳ segment chạm hazard.

## 2. Wall/Block

- Tile rắn, bất động, chặn snake và movable.
- Có thể có theme hình ảnh khác nhau nhưng cùng semantic.
- Destructible block phải là loại riêng, không dùng flag ẩn trên Wall.

## 3. Apple

- Collectible bắt buộc trước khi cổng mở.
- Apple thường tăng một segment và giảm counter bắt buộc của Normal/Hard.
- Apple VIP tăng tổng cộng hai segment (kéo dài độ dài đuôi qua 2 turn di chuyển liên tiếp), không nằm trong counter mở exit và ghi nhận một VIP
  collect; reward meta chỉ được commit khi level kết thúc Won.
- Cả hai loại apple là support cho gravity và chỉ collect khi head chủ động đi vào.
- UI hiển thị đúng mục tiêu bắt buộc theo mode, không gộp VIP vào apple counter.

## 4. Exit portal

- Một level chuẩn có đúng một exit.
- Có visual Locked/Open.
- Khi mở, phải đủ nổi bật nhưng không che board.
- Normal/Hard mở khi hết apple thường; Challenge coi exit mở từ đầu.

## 5. Rock

- Entity rắn, pushable và chịu trọng lực.
- Có thể đè button.
- Chặn snake, laser và vật khác theo cấu hình.
- Snake là support cho rock/bomb đang rơi. Vật dừng phía trên thân và không tự gây
  chết chỉ vì tiếp xúc; vật không xuyên qua hoặc cùng chiếm cell với segment.

## 6. Spike

- Hazard tĩnh có hướng.
- Cell spike giết snake từ mọi hướng tiếp cận và khi rơi lên nó.
- Orientation chỉ điều khiển visual/attachment, không thay đổi hit rule.
- Rock/bomb có thể nằm trên spike và che hazard trong lúc chiếm cell đó.
- Rock có thể che gai nếu nằm trên cùng cell theo layer rule.

## 7. Toxic

- Hazard dạng vùng, thường nguy hiểm khi snake chiếm cell.
- Không bị đẩy và không chịu trọng lực.
- Rock/bomb chiếm entity layer phía trên toxic, không bị toxic phá hủy và tạo
  một mặt đỡ an toàn cho snake khi vật vẫn còn ở đó.

## 8. Teleporter

- Luôn có pair/order và orientation.
- Hỗ trợ ít nhất hai cặp trong cùng level.
- Phải biểu diễn rõ cặp bằng màu/ký hiệu, không chỉ màu cho accessibility.
- Head đi qua pocket ẩn; thân đi theo nên có thể bắc qua hai đầu portal.
- Command sau khi vào pocket quyết định hướng ra; destination invalid làm command bị từ chối nguyên tử.
- Rock/bomb bị push qua portal giữ hướng push; slide bar không teleport.

## 9. Pressure button

- Cảm biến occupancy.
- State: Released, Pressed.
- Valid presser cố định là snake segment, rock và bomb.
- Button cửa mở latch khi được nhấn; cửa không đóng lại khi presser rời đi.
- Visual và âm thanh xảy ra ở cạnh chuyển state, không phát lặp mỗi tick.

## 10. Door

- State logic: Closed/Open/Destroyed; Opening là animation thuộc view.
- Quan hệ button–door dùng ID, không phụ thuộc vị trí trong list.
- Khi button liên kết được nhấn, cửa latch Open. Explosion có thể phá cửa và làm
  nó passable vĩnh viễn trong session.

## 11. Laser

- Gồm emitter, hướng và beam được tính theo ray trên grid.
- Beam dừng trước wall, frame cửa kể cả khi cửa đã mở, và slide bar.
- Với rock/bomb, beam chạm entity rồi dừng; bomb phát nổ, rock chỉ block.
- Với snake, beam chạm segment đầu tiên, đánh dấu segment đó bị hit rồi dừng.
- Beam đi qua apple, exit, teleporter, button, web và toxic.
- Beam là hazard với snake.
- Khi blocker di chuyển, laser tính lại trong cùng turn trước hazard check cuối.
- Laser crossing không tự chặn nhau.
- Emitter mặc định bật. Rock/bomb nằm trên cell emitter vô hiệu hóa beam tại nguồn.

## 12. Bomb

### Bomb thường

- Có thể push và chịu trọng lực.
- Kích nổ bởi laser, remote liên kết hoặc explosion khác; không nổ chỉ vì rơi.
- Vùng nổ là khối 3 × 3 quanh tâm, gồm cả đường chéo.
- Explosion bị wall/door chặn theo từng tia một ô, phá destructible block, phá
  spider net, kích bomb khác và giết snake trong vùng.
- Bomb bị đẩy theo bốn hướng, chịu gravity và có thể đè button.

### Remote bomb

- Remote là tile trigger tĩnh. Khi head snake đi vào, nó kích toàn bộ bomb có
  cùng `RemoteGroupId`, sau đó trở thành spent và không kích lần hai.
- Chuỗi nổ phải giải quyết deterministic bằng queue, không recursion vô hạn.
- Mỗi bomb chỉ nổ tối đa một lần trong turn; queue sắp theo stable EntityId.

## 13. Spider net

- Là support tile không rắn: entity có thể đi/đứng trong cell.
- Snake, rock và bomb tiếp xúc web được xem là có support và không rơi.
- Web tồn tại vĩnh viễn qua movement thường; chỉ explosion phá web.
- Web không chặn laser và không gây chết.

## 14. Slide bar

- Là platform rắn di chuyển giữa hai endpoint kề nhau trên một trục.
- Button slide-bar hoạt động theo cạnh Released → Pressed: mỗi lần nhấn hợp lệ
  đổi endpoint một lần; phải nhả button trước lần đổi tiếp theo.
- Khi platform dịch chuyển, occupant trên platform đi cùng nó. Entity ở cell đích
  bị đẩy một cell theo hướng platform nếu cell sau hợp lệ.
- Nếu bất kỳ occupant/push destination bị chặn, toàn bộ chuyển động platform bị
  hủy và endpoint không đổi.
- Phải ghi đầy đủ endpoint, orientation và state trong snapshot Undo.
- Slide bar là support và chặn laser tại vị trí hiện tại.

## 15. Gravity modifier

- Level có thể đảo hướng trọng lực.
- Nếu có object đổi gravity giữa level, phải là mechanic riêng và tạo turn event.
- Camera/UI không được lật input một cách ngầm định; Up vẫn là Up trên màn hình.

## 16. Ma trận tương tác cấp cao

| Entity | Chặn snake | Chịu gravity | Pushable | Đè button | Chặn laser |
|---|---:|---:|---:|---:|---:|
| Wall | Có | Không | Không | Không | Có |
| Rock | Có | Có | Có | Có | Có |
| Bomb | Có | Có | Có | Có | Có |
| Door đóng | Có | Không | Không | Không | Có |
| Door mở | Không | Không | Không | Không | Có, do frame |
| Apple/VIP | Không | Không; làm support | Không | Không | Không |
| Teleporter | Không | Không | Không | Không | Không |
| Snake | Có điều kiện | Theo toàn thân | Không | Có | Có |

Snake segment chặn laser; beam dừng trước segment và segment đầu tiên bị beam chạm
chết trong pha hazard. Ma trận chi tiết phải được mã hóa thành test, không
hard-code rải rác trong view.
