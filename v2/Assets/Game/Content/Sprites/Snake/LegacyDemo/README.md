# Legacy Snake Demo

Thư mục này giữ bộ rắn V1 để kiểm tra renderer modular của V2. Đây là asset tham khảo, chưa phải
skin Classic phát hành cuối cùng.

## Cấu trúc hiện tại

- `snake_legacy_head_idle.png`: đầu gốc V1, canvas `171 x 98`, cố ý tràn lên node body kế bên.
- `Assets/Game/Resources/SnakeSkins/LegacyV1SnakeSkin.asset`: skin dùng trực tiếp trong Gameplay
  và là runtime fallback nếu scene đang mở bị mất serialized reference sau khi compile.
- `Phased/body_*.png`: tám phase thân thẳng.
- `Phased/tail_*.png`: tám phase đuôi.
- Corner phase 1–4 dùng lại asset đã có trong thư mục Snake; phase 5–8 nằm trong `Phased/`.
  Cách này tránh duplicate GUID nhưng vẫn giữ đủ cả tám phase.

Renderer dựng mỗi phần bằng một `Image` trên `SnakeOverlay`, xác định straight/corner từ topology,
xoay sprite theo hướng nối và vẽ tail trước, head sau. Nếu skin thiếu asset, Gameplay tự quay về
placeholder màu để scene vẫn chạy.

Xem [Phased/README.md](Phased/README.md) để biết quy tắc chọn phase.
