# Thuật ngữ dự án

**Trạng thái:** Accepted

| Thuật ngữ | Định nghĩa |
|---|---|
| Board | Không gian lưới chứa tile, entity và trạng thái level. |
| Cell | Một tọa độ nguyên `(x, y)` trên board. |
| Tile | Thành phần tĩnh của cell: nền, tường, gai, độc, cổng. |
| Entity | Thành phần có state riêng: sâu, đá, bom, cửa, slide bar. |
| Turn | Một input hợp lệ cùng toàn bộ phản ứng phát sinh cho tới khi board ổn định. |
| Tick | Một bước nhỏ bên trong turn, thường dùng để giải quyết trọng lực. |
| Stable state | Trạng thái không còn vật nào phải rơi hoặc phản ứng bắt buộc. |
| Command | Ý định của người chơi, ví dụ `Move Left`, `Undo`, `Restart`. |
| Game event | Kết quả mô phỏng có ý nghĩa cho presentation, ví dụ `AppleEaten`. |
| Snapshot | Bản sao đầy đủ hoặc tối thiểu của state dùng cho Undo/Replay. |
| Deterministic | Cùng initial state và command sequence luôn cho cùng kết quả. |
| Soft-lock | Level chưa báo thua nhưng không còn cách thắng từ state hiện tại. |
| Hard-lock | Game không tiếp tục nhận input hoặc bị kẹt kỹ thuật. |
| Head | Đốt đầu tiên của sâu, nhận lệnh di chuyển. |
| Body segment | Một đốt thân, theo sau segment đứng trước. |
| Tail | Segment cuối của sâu. |
| Growth | Apple thường tăng một segment; VIP Apple tăng hai segment (kèm ghi nhận reward meta). |
| Gravity resolution | Quá trình cho entity rơi tới khi board ổn định. |
| Trigger | Vật thể tạo state khi bị sâu hoặc vật nặng đè lên. |
| Hazard | Vật thể có thể làm sâu chết: gai, độc, laser hoặc vụ nổ. |
| Exit | Đích kết thúc level; Normal/Hard có apple gate, Challenge mở từ đầu. |
| Teleporter | Cặp portal có pocket ẩn; head đi qua và thân có thể tạm bắc qua hai đầu. |
| Golden replay | Chuỗi command đã xác minh kết thúc Won với level hash và rules version cụ thể. |
| Verified level | Level qua validation và có ít nhất một golden replay hợp lệ. |
| LevelData | Dữ liệu thiết kế bất biến của một level. |
| BoardState | Trạng thái runtime có thể thay đổi của level. |
| Presentation | Sprite, animation, âm thanh, camera và UI biểu diễn simulation. |
| Service | Hệ thống ngoài gameplay như save, ads, IAP, analytics. |
| GDD | Game Design Document: mô tả game phải hoạt động thế nào. |
| TDD | Technical Design Document: mô tả phần mềm hiện thực game ra sao. |
| ADR | Bản ghi một quyết định kiến trúc và lý do của quyết định đó. |

## Quy ước tọa độ

- Gốc board là góc trên trái.
- `x` tăng từ trái sang phải.
- `y` tăng từ trên xuống dưới.
- `Up = (0, -1)`, `Down = (0, 1)`.
- Presentation chịu trách nhiệm đổi sang tọa độ world của Unity.

Quy ước này phù hợp với dữ liệu level dạng từng dòng và phải được dùng thống
nhất trong parser, simulation, test và tool level.
