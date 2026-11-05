# Kiến trúc tổng thể

**Trạng thái:** Accepted

## 1. Mục tiêu kiến trúc

- Core gameplay deterministic và test được ngoài Play Mode.
- Unity đóng vai trò presentation/composition, không phải nơi giữ luật chơi.
- Dữ liệu level có version, validate được và không phụ thuộc prefab hierarchy.
- Có thể thay UI, sprite hoặc animation mà không đổi simulation.
- Service thương mại không làm gameplay lỗi.
- Tránh God Object và singleton toàn cục như bản decompile.

## 2. Kiến trúc phân lớp

```text
┌─────────────────────────────────────────────┐
│ UI / Input / Unity Presentation             │
│ Screens, BoardView, Animation, Camera       │
└──────────────────────┬──────────────────────┘
                       │ Commands / Events
┌──────────────────────▼──────────────────────┐
│ Application                                 │
│ GameSession, GameFlow, TurnCoordinator      │
└──────────────────────┬──────────────────────┘
                       │ use cases
┌──────────────────────▼──────────────────────┐
│ Domain / Core Simulation                    │
│ BoardState, Rules, Systems, Results         │
└──────────────────────┬──────────────────────┘
                       │ ports
┌──────────────────────▼──────────────────────┐
│ Infrastructure                              │
│ LevelRepository, Save, Audio, Ads, IAP      │
└─────────────────────────────────────────────┘
```

Dependency chỉ được hướng vào trong. Domain không được tham chiếu UnityEngine.

## 3. Module chính thức

```text
Assets/Game/
├── Runtime/
│   ├── Domain/
│   │   ├── Board/
│   │   ├── Entities/
│   │   ├── Commands/
│   │   ├── Events/
│   │   ├── Rules/
│   │   └── Systems/
│   ├── Application/
│   │   ├── GameSession/
│   │   ├── GameFlow/
│   │   └── UseCases/
│   ├── Presentation/
│   │   ├── Board/
│   │   ├── UI/
│   │   ├── Animation/
│   │   └── Audio/
│   └── Infrastructure/
│       ├── Levels/
│       ├── Save/
│       ├── Monetization/
│       └── Analytics/
├── Editor/
│   ├── LevelEditor/
│   └── Validators/
├── Tests/
│   ├── EditMode/
│   └── PlayMode/
└── Content/
    ├── Levels/
    ├── Visuals/
    └── Audio/
```

Mỗi module lớn nên có Assembly Definition để enforce dependency.

## 4. Các aggregate chính

### GameSession

Giữ vòng đời một lần chơi level:

- Level ID và mode.
- `BoardState` hiện tại.
- Turn index.
- Undo history.
- Session status: Loading, Ready, Resolving, Won, Lost.

### BoardState

Nguồn sự thật duy nhất của simulation:

- Kích thước và tile layer.
- Entity registry.
- Snake state.
- Trigger graph.
- Gravity.
- Remaining apples.

### GameFlow

Quản lý màn hình và use case cấp ứng dụng:

- Boot → Home → LevelSelect → Loading → Playing.
- Playing → Paused/Won/Lost.
- Không chứa collision hoặc quy tắc board.

## 5. Command và Event

Presentation gửi command vào application:

```text
MoveCommand(Direction.Left)
UndoCommand
RestartCommand
PauseCommand
```

Simulation trả về event:

```text
SnakeMoved
AppleConsumed
EntityFell
DoorStateChanged
SnakeDied
LevelWon
TurnCompleted
```

Event gameplay là immutable value object. Không dùng C# event toàn cục làm nơi
lưu state; subscriber đến muộn phải lấy state hiện tại từ GameSession.

## 6. Composition root

Một `GameBootstrapper` trong boot scene tạo dependency:

```text
LevelRepository
SaveService
SimulationFacade
GameSession
GameFlowController
PresentationCoordinator
```

Dependency được truyền qua constructor cho class C# thuần và `[SerializeField]`
cho MonoBehaviour view. Không dùng `Class.Instance` ngoại trừ adapter platform
thật sự bắt buộc, và adapter đó phải được bọc bởi interface.

## 7. Assembly boundaries

| Assembly | Được phụ thuộc vào |
|---|---|
| Game.Domain | .NET/BCL tối thiểu |
| Game.Application | Game.Domain |
| Game.Infrastructure | Domain/Application ports, Unity khi cần |
| Game.Presentation | Application, Domain read models, Unity |
| Game.Editor | Runtime assemblies, UnityEditor |
| Game.Tests | Module đang test, test framework |

`Game.Domain` tuyệt đối không tham chiếu `UnityEngine.Vector2Int`; dùng `GridPos`
riêng để có thể chạy trong môi trường .NET thuần.

## 8. Quy tắc ownership

- Domain sở hữu sự thật gameplay.
- Application sở hữu lifecycle và use case.
- Presentation sở hữu GameObject và animation đang chạy.
- Infrastructure sở hữu I/O và SDK ngoài.
- Không module nào ngoài SaveService tự ghi PlayerPrefs/file.
- Không view tự sửa BoardState.
