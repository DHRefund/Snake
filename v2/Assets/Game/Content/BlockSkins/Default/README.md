# Default block skin configuration

Thư mục dành cho ScriptableObject production ánh xạ `mask 0..15` tới các sprite trong
`Content/Sprites/Blocks/Skins/Default/Runtime`.

Không chuyển `LegacyDefaultBlockSkin.asset` vào đây: asset đó giữ schema ghép layer của V1 và
chỉ phục vụ migration. Cấu hình production sẽ dùng mảng 16 topology và danh sách variant cho
từng topology.

