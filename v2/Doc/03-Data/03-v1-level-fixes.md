# Sửa dữ liệu và luật cho level V1

Lý do: khi chơi lại lời giải (hint) V1 bằng engine V2, 8 level không thắng được. Bốn level (90, 93, 136,
175) không thể hoàn thành dù thử mọi nước đi. Kiểm tra bằng bản port Python của `PuzzleRules`
(khớp báo cáo `Library/LegacyReplayValidation.txt`: 276 thắng, 5 bị từ chối, 2 chưa xong, 1 thua).

## 1. Luật: bệ trượt mang theo vật nằm trên nó

`PuzzleRules.CarryMovablesOnPlatform`: khi bệ trượt/thang máy đổi vị trí, đá và bom đang nằm ngay trên bệ
dịch chuyển cùng bệ một khoảng đúng bằng độ dời của bệ. Nếu ô đích bị chặn (tường, vật khác, rắn) thì vật ở
lại. Rắn không bị mang theo (thử mang rắn làm hỏng level khác).

Ảnh hưởng: Normal 90, 115, 136, 175, 179 chạy đúng lời giải V1. Không level nào đang thắng bị hỏng.
Ví dụ Normal 136: bom đi lên cùng bệ, nổ bằng nút `k` và phá tường hàng 16 cho rắn đi qua.

## 2. Dữ liệu

| Level | Sửa | Lý do |
|---|---|---|
| normal-093 | `slideBarsHorizontal` false → true, thay lời giải | Cờ hướng bệ bị ngược trong dữ liệu V1, đảo cờ thì level giải được |
| normal-119 | thay lời giải (18 bước) | lời giải V1 dừng trước khi thắng |
| normal-177 | thêm lời giải (35 bước) | V1 không có dữ liệu hint |

Các sửa này nằm trong `LegacyLevelImporter.ApplyV2Fixes` nên import lại từ V1 không làm mất chúng.
Lời giải mới là đường ngắn nhất tìm được bằng BFS, đã chạy lại và thắng.

## 3. Kết quả

Cả 284 level thắng bằng lời giải trong `legacy-catalog.json` khi chạy bằng bản port Python của engine mới.
Cần chạy `Snake Apple V2/Validate All V1 Hint Replays` trong Unity để xác nhận trên engine C#
(kỳ vọng `284/284 won`).
