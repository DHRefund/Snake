# ADR-001: Headless deterministic core

**Status:** Accepted  
**Date:** 2026-10-18

## Context

Game cần Undo, replay, hint, solver và validation hàng trăm level. Nếu luật chơi
nằm trong MonoBehaviour, coroutine và Transform, test sẽ chậm và kết quả có thể
phụ thuộc frame/animation.

## Decision

Core simulation được viết bằng C# thuần, không tham chiếu UnityEngine. Mỗi turn
là phép chuyển từ state + command sang next state + ordered events. Time và
animation không tham gia quyết định gameplay.

## Alternatives

### MonoBehaviour cho mọi entity

Dễ dựng prototype nhưng phụ thuộc scene, khó snapshot và replay deterministic.

### Unity ECS/DOTS

Có hiệu năng cao nhưng tăng độ phức tạp không cần thiết cho board nhỏ.

## Consequences

### Tích cực

- Unit test nhanh.
- Undo/replay/hint tự nhiên.
- Có thể build solver/tool ngoài Play Mode.
- Presentation thay đổi độc lập.

### Tiêu cực

- Cần mapping Domain entity sang Unity view.
- Có duplication giữa GridPos và Vector/Transform.
- Team phải kỷ luật không đẩy logic ngược vào view.
