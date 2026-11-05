# Chiến lược kiểm thử

**Trạng thái:** Accepted

## 1. Test pyramid

```text
              Manual playtest
           PlayMode integration tests
       Application/use-case integration tests
    Domain unit + golden replay + level validation
```

Phần lớn coverage phải nằm ở Domain vì nhanh và deterministic.

## 2. Domain unit tests

### Movement

- Đi vào empty cell.
- Bị wall/door chặn.
- Đi vào body bị chặn.
- Đi vào tail cell khi tail rời đi.
- Ăn apple không bỏ tail.
- Push rock thành công/thất bại.

### Gravity

- Snake rơi một/nhiều cell.
- Một segment được đỡ giữ toàn thân.
- Reverse gravity.
- Rock và snake cùng rơi.
- Destination conflict deterministic.
- Rơi khỏi board.

### Trigger

- Snake/rock nhấn và nhả button.
- Door Any/All.
- Door pending close khi occupied.
- Trigger sau gravity.

### Hazard

- Head/body chạm spike, toxic, laser.
- Hazard sau teleport/gravity.
- Win và death cùng turn theo priority rule.

## 3. Golden replay tests

Mỗi level phát hành có solution replay. Test:

1. Load level.
2. Assert content hash/schema.
3. Chạy command sequence headless.
4. Assert mọi command accepted.
5. Assert final outcome Won.
6. Có thể assert move count/final state hash.

Test này chạy cho toàn catalog trong CI.

## 4. Property/invariant tests

Sau mọi accepted turn ngẫu nhiên hợp lệ:

- Không duplicate solid occupancy.
- Snake segment liên thông.
- Apple counter đúng.
- Undo rồi redo command tạo cùng state hash.
- Replay command list tạo state hash giống session gốc.
- Simulation kết thúc trong max pipeline iterations.

## 5. Parser/validator tests

- File đúng schema.
- Unknown symbol/version.
- Sai width/height.
- Thiếu head/exit.
- Snake đứt đoạn.
- Teleport thiếu pair.
- Link dangling/cycle không hợp lệ.
- Legacy char import cho mọi symbol đã biết.

## 6. Application tests

- Start/restart/next level.
- Unlock progression idempotent.
- Undo availability event.
- Save khi completion.
- Error khi level invalid.
- Ads/service fail không chặn flow.

## 7. PlayMode tests

- BoardView spawn đúng số view.
- Event journal tạo timeline đúng thứ tự.
- Skip animation kết thúc ở view state đúng.
- UI button gửi đúng command.
- Pause/resume giữa animation.
- Safe area/layout smoke test.

## 8. Save tests

- Round-trip serialization.
- Migration từng version.
- Corrupt primary dùng backup.
- Interrupted write không mất cả hai bản.
- Duplicate reward transaction không cấp hai lần.

## 9. Performance tests

- 10.000 headless turns trong budget.
- Load level và build view không spike quá ngưỡng mục tiêu.
- Không tăng memory sau replay level nhiều lần.
- Pool không giữ asset ngoài mode/theme không dùng.

## 10. Manual playtest checklist

- Người mới hiểu mục tiêu level 1 không cần giải thích ngoài game.
- Move bị chặn có lý do rõ.
- Undo nhanh và đáng tin.
- Không nhầm portal với teleport.
- Animation không che state quan trọng.
- Level khó do suy luận, không do điều khiển khó.

## 11. Bug report tối thiểu

- Build/game/rules version.
- Platform/device.
- Level ID + content hash.
- Initial seed nếu có.
- Accepted command sequence hoặc replay file.
- Expected/actual outcome.
- Screenshot/video cho presentation bug.

Replay file là artifact bắt buộc cho simulation bug có thể tái hiện.
