# Presentation, animation và game flow

**Trạng thái:** Accepted

## 1. Nguyên tắc

Presentation biểu diễn event đã xảy ra; nó không quyết định event có hợp lệ hay
không. BoardState có thể commit ngay, trong khi view phát animation tuần tự.

## 2. BoardView

`BoardView` ánh xạ semantic state sang view:

```text
EntityId → EntityView
GridPos  → WorldPosition
TileType → TileVisualDefinition
```

Nó chịu trách nhiệm:

- Spawn/pool view khi load level.
- Cập nhật sprite/theme.
- Giữ registry view theo EntityId.
- Không chứa luật collision.

## 3. Animation Pipeline & Visual Juice (`AnimateMoveRoutine`)

Quá trình diễn hoạt turn trong V2 được chia thành 2 Phase độc lập với các thông số vật lý và juice đàn hồi cao cấp:

```text
Turn Start
  │
  ├── [Phase 1: Active Move Slide] (0.12s)
  │     ├── Nội suy SmoothStep giữa vị trí cũ và mới của từng đốt rắn.
  │     └── Push Movables (đá, bom) trượt đồng bộ cùng vận tốc.
  │
  ├── [Chomp Feedback] (0.08s - chỉ khi ăn táo)
  │     ├── Đầu rắn giật punch scale (1.25x -> 1.0x).
  │     └── Apple Visual biến mất đồng thời.
  │
  ├── [Phase 2: Gravity Fall] (0.065s / cell rơi)
  │     ├── Rơi gia tốc theo hàm bậc hai (t²), tạo cảm giác rơi tự do chân thực.
  │     └── Các đối tượng rơi tự do (đá, bom, rắn) rơi đồng thời.
  │
  └── [Landing Squash & Stretch] (0.09s - sau khi tiếp đất)
        ├── Toàn bộ thân rắn nhún xẹp xuống (Scale Y = 0.86, Scale X = 1.14).
        └── Bung nảy đàn hồi trở lại kích thước chuẩn (1.0x).
```

- **Phản hồi va chạm (`BumpHeadRoutine`)**: Khi bước đi bị từ chối do đâm tường/vật cản, đầu rắn giật nảy ngược lại trong $0.10s$ ($0.04s$ thụt vào, $0.06s$ hồi vị), mang lại cảm giác phản lực vật lý rõ ràng.
- Cuối timeline, view tự động snap chính xác về state đã commit trong Domain.
- Khi người chơi gọi Undo/Restart, coroutine animation lập tức bị hủy (`CancelActiveAnimation`), dọn sạch trạng thái tạm thời và đồng bộ ngay với snapshot mới.

## 4. Input locking và Buffering (Hàng đợi lệnh)

- Trong suốt thời gian Phase 1 và Phase 2 đang diễn ra, input tức thời được khóa để tránh xung đột vật lý.
- **Input Buffer Queue**: Hệ thống lưu trữ tối đa 1 lệnh di chuyển kế tiếp (`queuedDirection`). Khi animation của turn hiện tại hoàn tất, lệnh trong hàng đợi được thực thi ngay lập tức mà không cần người chơi phải canh đúng frame bấm phím, mang lại trải nghiệm điều khiển cực nhạy và mượt mà.
- Không buffer quá 1 lệnh để tránh tình trạng state đi trước visual quá xa.

## 5. Camera

- Fit board vào vùng gameplay sau khi trừ safe area và HUD.
- Không thay đổi zoom giữa turn trừ event đặc biệt.
- Shake nhỏ cho explosion/death, có setting giảm motion.
- Board-to-world conversion chỉ tồn tại ở presentation.

## 6. Game flow state machine

```text
Boot
  → LoadingProfile
  → Home
  → LevelSelect
  → LoadingLevel
  → Playing
       ↔ Paused
       → Won
       → Lost
  → Home / LevelSelect / NextLevel
```

Mỗi transition có owner rõ:

| Transition | Trigger |
|---|---|
| Boot → Home | Profile + catalog load thành công |
| Home → LevelSelect | Người chơi chọn Play/Select |
| LevelSelect → Loading | Chọn level hợp lệ |
| Loading → Playing | Level parse, validate và view build xong |
| Playing → Won/Lost | Domain result |
| Playing → Paused | Application pause hoặc UI command |

## 7. Scene strategy

V2 dùng đúng hai scene runtime:

- `Boot`: bootstrapper và persistent services.
- `Game`: camera, screen root, board root và reusable presentation.

Home/LevelSelect/Gameplay là UI screen, không bắt buộc mỗi screen một scene.
Level content là data asset, không phải scene hoặc prefab chứa logic.

## 8. Error handling

Nếu level load/validate thất bại:

- Không vào Playing.
- Hiển thị error screen trong development build.
- Ghi LevelId, content hash và danh sách validation errors.
- Production có thể quay về LevelSelect với thông báo chung.
