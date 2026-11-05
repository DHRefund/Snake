# Audio, VFX và Haptics

**Trạng thái:** Accepted

## 1. Mục tiêu phản hồi

Phản hồi phải giúp đọc state trước khi trang trí. Người chơi cần phân biệt:

- Move thành công và move bị chặn.
- Ăn táo.
- Rơi và tiếp đất.
- Button được nhấn/nhả.
- Door mở/đóng.
- Hazard, chết và thắng.

## 2. Semantic cue

Domain/Application phát cue semantic:

```text
SnakeMove
MoveBlocked
AppleEaten
RockPushed
EntityLanded
ButtonPressed
DoorOpened
Teleport
Explosion
SnakeDied
LevelWon
```

Audio config ánh xạ cue sang clip, volume, pitch range và cooldown.

## 3. Audio channel

- Music: loop nền.
- Gameplay SFX: move, apple, object.
- UI SFX: click, panel.
- Ambience: tùy theme.

Mỗi channel có mixer group và setting. Master mute không được thay đổi giá trị
setting con; chỉ điều khiển mixer snapshot.

## 4. Chống spam

- Footstep/move cue có cooldown ngắn.
- Nhiều rock landing cùng tick có thể aggregate hoặc giới hạn voice.
- Button chỉ phát khi state đổi.
- Pitch variation nhỏ, deterministic không cần thiết vì audio không ảnh hưởng game.

## 5. VFX

- Apple: squash/pop và particle ngắn.
- Teleport: effect ở entry và exit, không che vị trí kết thúc.
- Hazard: chỉ rõ segment bị tác động.
- Win: celebratory nhưng có thể skip.
- Không dùng particle làm nguồn sự thật cho laser hoặc door state.

## 6. Haptics

| Sự kiện | Cường độ mặc định |
|---|---|
| Move blocked | Light |
| Apple eaten | Light |
| Rock landing | Medium |
| Explosion/death | Heavy |
| Win | Pattern ngắn |

Haptics có toggle riêng và luôn qua `IHapticsService`.

## 7. Accessibility

- Thông tin không chỉ truyền bằng âm thanh hoặc rung.
- Reduced Motion giảm shake, particle và tween overshoot.
- Flash do laser/explosion phải trong giới hạn an toàn.
- Có thể tắt music/SFX/haptics độc lập.
