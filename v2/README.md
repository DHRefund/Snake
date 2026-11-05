# Snake & Apple: Puzzle Adventure (V2)

Unity project sạch được xây lại từ specification trong [`Doc/`](Doc/README.md). Bản AssetRipper
chỉ là nguồn tham khảo asset/level và không phải runtime dependency của project này.

## Toolchain

- Unity Editor `6000.3.21f1`.
- EditMode tests qua Unity Test Framework.
- Target thương mại đầu tiên: Android.

## Trạng thái hiện tại

Milestone nền đầu tiên đã có:

- `Game.Domain` không tham chiếu UnityEngine.
- Grid, wall, snake có thứ tự head → tail, apple/VIP apple và exit.
- Move deterministic; rejected move không tạo turn.
- Ăn apple, tăng chiều dài, mở exit và commit Win.
- `GameSession` với Undo và Restart bằng full snapshot.
- JSON level parser/validator và level `normal-001` có lời giải mẫu.
- Sáu scene UI thật: Boot, Home, Level Select, Gameplay, Settings và Cosmetics.
- Gameplay HUD, D-pad, swipe/keyboard, Pause, Win và Lose modal.
- Hai prefab UI dùng chung: PrimaryButton và ModalCard.
- EditMode tests cho movement, growth, exit, rejection, undo, restart và parsing.

Chưa triển khai trong milestone này: gravity, rock/hazard, teleport, trigger/door, laser/bomb,
asset hình gốc và SDK quảng cáo. UI hiện là production shell bằng shape/màu code-native;
sprite/animation gốc sẽ thay phần minh họa mà không đổi screen flow.

## Chạy game

1. Cài Unity `6000.3.21f1` bằng Unity Hub và mở thư mục `v2`.
2. Chờ Unity resolve package và compile.
3. Mở `Assets/Game/Scenes/Boot.unity` và nhấn Play.

Toàn bộ scene đã là file thật trong project và đã có trong Build Settings; không cần chạy menu
setup. `Boot.unity` là entry point của game.

Điều khiển:

- Swipe, D-pad, Arrow keys hoặc WASD: di chuyển.
- `Z`: Undo.
- `R`: Restart.

Lời giải level mẫu: `Up → Right → Down → Down → Left`.

## Chạy test

Trong Unity: `Window > General > Test Runner > EditMode > Run All`.

Command line sau khi có Unity Editor:

```powershell
& "<Unity.exe>" -batchmode -nographics -projectPath "G:\hackgame\game  con rắn\v2" `
  -runTests -testPlatform EditMode -testResults "G:\hackgame\game  con rắn\v2\TestResults.xml" -quit
```
