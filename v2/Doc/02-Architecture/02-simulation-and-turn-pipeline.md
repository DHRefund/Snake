# Core Simulation và Turn Pipeline

**Trạng thái:** Accepted

## 1. Yêu cầu deterministic

Simulation là hàm chuyển trạng thái:

```text
NextState, Events = Execute(CurrentState, Command)
```

Nó không được đọc:

- `Time.deltaTime`.
- Transform/GameObject.
- Random không seed.
- Thứ tự phần tử từ HashSet/Dictionary để quyết định gameplay.
- Trạng thái animation.
- Callback ads/audio/network.

## 2. Kiểu dữ liệu nền

```csharp
public readonly record struct GridPos(int X, int Y);

public enum Direction { Up, Down, Left, Right }

public sealed class BoardState
{
    public BoardSize Size { get; }
    public TileMap Tiles { get; }
    public EntityStore Entities { get; }
    public SnakeState Snake { get; }
    public GravityDirection Gravity { get; set; }
    public int RemainingApples { get; set; }
}
```

Ví dụ chỉ thể hiện intent; phiên bản C# thực tế phải phù hợp Unity đã chọn.

## 3. TurnCoordinator

`TurnCoordinator` là nơi duy nhất biết thứ tự system:

```text
ValidateCommandSystem
PushSystem
SnakeMovementSystem
CollectibleSystem
ContactTerminalSystem
TriggerSystem
SnakeGravitySystem
MovableGravitySystem
TriggerSystem (post gravity)
LaserSystem
ExplosionSystem
HazardSystem
InvariantValidator
```

Các system không tự gọi nhau. Chúng nhận context và ghi event vào `TurnJournal`.

## 4. Transaction của một turn

1. Clone/capture state cho undo.
2. Validate command mà không mutation.
3. Nếu rejected: trả `RejectedResult`, không tăng turn index.
4. Nếu accepted: thực thi pipeline trên mutable working state.
5. Validate invariant.
6. Commit state.
7. Trả ordered event list cho presentation.

Nếu invariant thất bại trong development build, turn bị rollback và phát lỗi có
đủ level ID, turn index, command sequence.

## 5. Gravity solver

Gravity chạy theo hai pha có thứ tự để tương thích content V1:

```text
repeat MoveWholeSnakeOneCell until supported/terminal
repeat
  sort movables from downstream to upstream along gravity
  MoveEligibleMovablesOneCell
until no movable moved
```

`maxTicks` thường bằng chiều dài theo hướng gravity + margin. Chạm giới hạn là
lỗi dữ liệu/logic, không phải kết thúc hợp lệ.

### Thứ tự ổn định

Trong pha movable, thứ tự ổn định là:

1. Khoảng cách tới biên theo hướng gravity.
2. Stable position theo trục phụ.
3. Stable EntityId.

Không dùng thứ tự instance Unity.

## 6. Occupancy model

Một cell có thể chứa nhiều layer:

```text
Ground layer: floor, toxic
Structure layer: wall, door, teleporter, button
Entity layer: snake segment, rock, bomb
Effect layer: laser beam, explosion
```

Không nên ép tất cả vào một enum duy nhất. Query collision dùng capability:

```text
BlocksSnake
BlocksMovable
BlocksLaser
IsHazardForSnake
CanPressTrigger
```

Capability được sinh từ definition/config, không gắn vào view prefab.

## 7. Event journal

Mỗi turn tạo danh sách event có thứ tự, ví dụ:

```text
TurnStarted(42, MoveRight)
RockMoved(R1, 5,4 → 6,4, Push)
SnakeMoved([...])
AppleConsumed(A3, 7,4)
SnakeGrew(5 → 6)
EntityFell(Snake, distance=2)
DoorOpened(D1)
TurnCompleted(42, Playing)
```

Event chứa đủ dữ liệu để animate mà không cần đọc state trung gian đã mất.

## 8. Invariant bắt buộc

Sau mỗi committed turn:

- Snake có ít nhất một segment nếu chưa hoàn tất exit animation logic.
- Segment liên tiếp hợp lệ theo đường thân, bao gồm liên kết qua portal pocket.
- Không có hai solid entity chiếm cùng entity layer cell.
- `RemainingApples` bằng số apple thường còn active; VIP Apple được theo dõi riêng.
- Entity ID duy nhất.
- Door graph không tham chiếu trigger không tồn tại.
- Teleporter pair hợp lệ.
- Won và Lost không cùng true.

## 9. Replay

Replay file tối thiểu gồm:

```text
GameVersion
RulesVersion
LevelId
LevelContentHash
InitialSeed
AcceptedCommands[]
```

Không cần lưu event; event được tái tạo. Nếu hash/rules version khác, replay được
đánh dấu incompatible thay vì chạy với kết quả sai.

## 10. Hiệu năng

Board nhỏ nên ưu tiên correctness và readability. Chưa cần ECS/DOTS. Target:

- Một turn core dưới 1 ms trên thiết bị mục tiêu trung bình.
- Không cấp phát lớn trong gravity loop sau giai đoạn tối ưu.
- Có benchmark chạy 10.000 turn trong test môi trường desktop.
