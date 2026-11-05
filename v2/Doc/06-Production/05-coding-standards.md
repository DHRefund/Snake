# Coding standards và quy ước dự án

**Trạng thái:** Accepted

## 1. Mục tiêu

Code phải ưu tiên tính đúng, khả năng đọc và khả năng test. Không tối ưu sớm bằng
cách làm mờ luật puzzle.

## 2. Naming

- Type/method/property/event: `PascalCase`.
- Local/parameter/private field: `camelCase`; private serialized field có thể
  dùng `_camelCase` nếu toàn dự án thống nhất.
- Interface: prefix `I` chỉ cho abstraction thật sự.
- Boolean bắt đầu bằng `Is`, `Has`, `Can`, `Should`.
- Không dùng viết tắt mơ hồ như `Mgr`, `Ctr`, `Val` trong public API.
- Domain dùng tên semantic: `GridPos`, `TurnResult`, `DoorState`.

## 3. Class responsibility

- Một class có một lý do chính để thay đổi.
- MonoBehaviour chỉ dùng khi cần lifecycle/serialization/GameObject.
- Domain class không kế thừa MonoBehaviour/ScriptableObject.
- Không tạo class `Manager` chung chung; tên phải nói rõ use case.
- File và public type chính cùng tên.

## 4. State mutation

- Mutation gameplay chỉ qua TurnCoordinator/system được ủy quyền.
- Collection public trả read-only view.
- Value object immutable khi hợp lý.
- Không expose setter công khai chỉ để Inspector chỉnh runtime state.
- Không dùng static mutable field cho session state.

## 5. Error handling

- Invalid authoring data: validator error có path/cell/entity ID.
- Rejected player command: result bình thường, không throw exception.
- Programming invariant broken: exception/assert trong development/test.
- External service failure: typed result, không swallow im lặng.
- Không dùng exception để điều khiển gameplay flow thông thường.

## 6. Logging

Log có category và context. Ví dụ:

```text
[Simulation] Invariant failed level=normal-042 turn=18 hash=...
```

Không log mỗi frame/turn trong release. Không log receipt, token hoặc PII.

## 7. Magic values

- Gameplay constant có tên hoặc config.
- Animation duration nằm trong presentation config.
- Economy/ads rule nằm trong remote/local config có version nếu cần.
- Symbol legacy chỉ tồn tại trong importer, không lan vào Domain.

## 8. Async và coroutine

- Domain synchronous và deterministic.
- Async dùng cho I/O/service/loading.
- Coroutine/Tween dùng cho presentation.
- Cancellation bắt buộc cho flow có thể rời screen/scene.
- Callback SDK phải marshal về main thread ở adapter nếu cần.

## 9. Testability

- Time qua `IClock`, random qua `IRandomSource` khi cần.
- Service qua interface và fake.
- Không `FindObjectOfType` trong logic cần test.
- Không dùng animation event để commit state.

## 10. Comment/documentation

- Comment giải thích “vì sao”, không lặp lại code.
- Public API quan trọng có XML docs nếu tên chưa đủ rõ.
- Workaround có issue/ADR reference và điều kiện xóa.
- TODO phải có owner hoặc ticket, không để TODO vô thời hạn.
