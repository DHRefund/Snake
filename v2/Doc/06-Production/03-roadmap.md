# Roadmap triển khai V2

**Trạng thái:** Accepted

Roadmap dùng milestone theo outcome, không khóa cứng thời gian khi chưa biết quy mô
đội ngũ. Mỗi milestone chỉ bắt đầu khi dependency trước đủ ổn định.

## M0 — Discovery và khóa luật

### Deliverables

- Duyệt GDD/TDD.
- Ghi nhận reference APK nếu hữu ích, nhưng không dùng nó làm blocker cho ruleset.
- Chốt Unity version và target platform.
- Chốt orientation, art reuse policy và phạm vi release.
- Lập symbol mapping legacy đầy đủ.

### Exit criteria

- Ruleset V2 và turn ordering đã được đánh version.
- ADR nền được accepted.

## M1 — Headless core

### Deliverables

- Domain assembly không phụ thuộc UnityEngine.
- Grid, BoardState, snake movement, apple, exit.
- TurnCoordinator, event journal, state hash.
- Undo snapshot và replay command sequence.
- Unit tests nền.

### Exit criteria

- Test movement/undo/replay xanh.
- Một level sample chạy đến Won hoàn toàn headless.

## M2 — Level pipeline

### Deliverables

- V2 JSON schema/parser/validator.
- Legacy importer cho mechanic M1.
- Level catalog.
- Solution recorder và golden replay runner.

### Exit criteria

- Một batch level Normal đơn giản import và validate thành công.
- CI chạy replay toàn batch.

## M3 — Minimal playable Unity

### Deliverables

- Boot/Game scene.
- BoardView, SnakeView và generated/basic visual.
- Keyboard, swipe và UI button input.
- Gameplay HUD, pause, win/lose.
- Save progression cơ bản.

### Exit criteria

- Có thể cài build và chơi chuỗi level đầu trên thiết bị.
- Không cần console/editor để chuyển level.

## M4 — Core mechanics

Triển khai theo thứ tự rủi ro:

1. Rock và gravity solver đầy đủ.
2. Spike/toxic.
3. Teleporter.
4. Button/door.
5. Laser.
6. Bomb/spider net.
7. Slide bar.

Mỗi mechanic cần interaction matrix, test và tutorial level trước mechanic tiếp.

## M5 — Original presentation integration

### Deliverables

- Mapping asset gốc hoặc art thay thế.
- Worm segment/corner/tail visuals.
- Animation timeline, VFX, audio và camera polish.
- Theme/skin presentation không ảnh hưởng logic.

### Exit criteria

- Visual state luôn khớp simulation sau skip/undo/pause.
- Performance đạt thiết bị mục tiêu.

## M6 — Content và UX complete

- Normal campaign đã curate.
- Hard/Challenge khi mechanic tương ứng ổn định.
- Tutorial, hint và accessibility.
- Localization pipeline.
- Full level select và progression.

## M7 — Meta và monetization phát hành

- Cosmetic/economy.
- AdMob Rewarded/Interstitial qua service adapter; IAP remove-ads là mở rộng không bắt buộc.
- UMP consent/privacy và Privacy options.
- Analytics funnel.

M7 là release gate nhưng chỉ bắt đầu sau khi core puzzle đạt Definition of Done. SDK lỗi hoặc
không có mạng không được làm giảm khả năng chơi game.

## M8 — Release candidate

- Full regression/golden replay.
- Save migration and recovery test.
- Device matrix.
- Crash/performance monitoring.
- Store compliance, legal/privacy, attribution/license review.

## Ưu tiên khi thiếu nguồn lực

```text
Correct simulation
> Valid levels
> Clear UX
> Stable save
> Presentation polish
> Meta economy
> Monetization
```
