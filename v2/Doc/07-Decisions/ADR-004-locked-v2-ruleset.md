# ADR-004: Khóa ruleset V2 và turn ordering

**Status:** Superseded bởi ADR-005  
**Date:** 2026-10-18

## Context

> Tài liệu này được giữ để lưu lịch sử. Không dùng các rule bên dưới làm nguồn
> triển khai hiện hành; xem `ADR-005-v1-compatible-rules-and-content.md`.

Bản AssetRipper cho biết inventory mechanic nhưng mất phần lớn method body. Chờ
khôi phục chính xác APK sẽ làm block thiết kế và implementation. Các game cùng
họ như Snakebird cung cấp nền tảng đã được kiểm chứng: head-led propagation,
fruit growth, whole-shape gravity, all-fruit exit gate và turn discrete. Sokoban
cung cấp nguyên tắc push nguyên tử và không kéo vật.

## Decision

V2 có ruleset độc lập, version bắt đầu từ `RulesVersion = 1`. Luật trong GDD là
nguồn sự thật, không phụ thuộc hành vi APK được phát hiện sau này.

### Core

- Một input trực giao di chuyển head một cell; body theo vị trí predecessor.
- Không được đi vào bất kỳ own segment nào, kể cả tail sắp rời.
- Ăn apple tăng đúng một tail segment.
- Sau active movement, unsupported snake rơi như một hình cứng.
- Ăn hết apple mở exit; head vào exit bắt đầu evacuation và dừng môi trường.

### Push và gravity

- Snake đẩy một rock/bomb theo cả bốn hướng; không đẩy chain.
- Rock/bomb chịu gravity. Rock rơi trúng snake gây `Crushed`.
- Gravity dùng simultaneous tick với deterministic conflict resolution.

### Special mechanics

- Spike gây chết từ mọi hướng; orientation chỉ là visual.
- Rock/bomb che toxic và spike trong lúc chiếm cell.
- Teleporter chỉ tác động snake head, dịch toàn thân cùng offset, không xoay.
- Door button momentary; snake/rock/bomb đều press được.
- Laser dừng ở solid blocker và snake, cập nhật sau gravity/trigger.
- Bomb nổ theo dấu cộng bán kính một; remote dùng group ID; chain theo queue.
- Spider net là support vĩnh viễn cho snake/rock/bomb, chỉ bomb phá được.
- Slide bar toggle theo press edge, di chuyển nguyên tử giữa hai endpoint.

### Turn order

```text
Validate
→ Snapshot
→ Push
→ Snake move / collect / grow
→ Static hazard + open-exit contact checkpoint
→ Teleport
→ Static hazard + open-exit contact checkpoint
→ Trigger pre-gravity
→ Gravity until stable, contact checkpoint sau mỗi tick
→ Trigger post-gravity
→ Laser
→ Remote + explosion chain
→ Dynamic hazard/death
→ Commit + ordered events
```

Tại mỗi contact checkpoint, static hazard trên bất kỳ segment nào được xét trước
open-exit contact. Sau khi exit capture bắt đầu, không có phản ứng môi trường mới.
Rejected command không tạo turn hoặc snapshot.

## Alternatives

### Chờ reverse engineering hoàn chỉnh

Tăng độ giống APK nhưng không có thời hạn chắc chắn và tiếp tục để doc mơ hồ.

### Sao chép toàn bộ Snakebird

Có nền tảng tốt nhưng không bao phủ mechanic riêng như laser, bomb và slide bar.

### Dùng Unity physics

Nhanh cho prototype nhưng khó deterministic, Undo và solver.

## Consequences

- Team có thể code và test mà không chờ APK.
- Level import phải migrate theo ruleset V2; một số level legacy có thể cần sửa.
- Replay/save gắn `RulesVersion` và content hash.
- Khác biệt với APK được ghi là deviation, không tự động đổi rule.
- Thay đổi bất kỳ luật nào ở trên cần ADR mới và tăng RulesVersion nếu ảnh hưởng
  replay, solution hoặc state transition.

## Nguồn tham khảo thiết kế

- [Snakebird mechanical profile](https://puzzlegenome.org/en/games/snakebird/)
- [Snakebird overview](https://thinkygames.com/games/snakebird/)
- [Snakebird App Store editorial](https://apps.apple.com/us/iphone/story/id1350884296)
- [Sokoban push model](https://en.wikipedia.org/wiki/Sokoban)
- [Snake Apple product description](https://apkpure.net/snake-apple/com.snakeapple.worm.puzzle)
