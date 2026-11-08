# UI Sprites

Sprite dùng cho màn hình UI. Chưa gắn vào scene nào ngoài logo và progress bar của Boot;
scene hiện vẫn vẽ bằng shape code-native (`UiSceneGenerator`).

## Cấu trúc

| Thư mục | Nội dung |
|---|---|
| `Backgrounds/` | Nền toàn màn hình (portrait, 1440×1920) |
| `Branding/` | Logo game, logo rắn, logo táo |
| `Buttons/` | Nút bấm (nền không chữ, một số có border 9-slice) |
| `Cosmetics/` | Khung ô skin, nút giá, dấu đã chọn |
| `Currency/` | Coin và khung hiển thị coin |
| `Icons/` | Icon nhỏ đặt trên nút hoặc thanh |
| `Panels/` | Khung nền popup |
| `Progress/` | Thanh tiến trình (track + fill) |
| `Ribbons/` | Ruy-băng Win / Lose |

## Quy ước

- Tên file `snake_case` chữ thường, tiền tố theo loại: `btn_`, `icon_`, `bg_`, `coin_`.
- Mỗi sprite đi cùng `.meta`; di chuyển file phải di chuyển cả `.meta` (giữ GUID).
- Code Editor nạp sprite theo đường dẫn đầy đủ; đổi vị trí file phải sửa `UiSceneGenerator`.
- Nút cần co giãn đặt `spriteBorder` rồi dùng `Image.Type.Sliced`.
- File cũ chưa đổi tên (`btn_SETTINGS`, `btn_CANCEL`, ...) để giữ GUID; file mới dùng chữ thường.

## Nguồn gốc (V1 AssetRipper)

Toàn bộ sprite được lấy từ `SnakeApple_ExportedProject/.../Texture2D`. Chưa qua review license
(xem roadmap M8). Cần thay bằng asset tự sản xuất hoặc có giấy phép trước khi phát hành.

| V2 | V1 |
|---|---|
| `Branding/logo_main` | `logo` |
| `Backgrounds/bg_clouds_light` | `BG1` |
| `Backgrounds/bg_desert_day` | `BG2` |
| `Buttons/btn_level` | `btn_LEVEL` |
| `Buttons/btn_skin` | `btn_SKIN` |
| `Buttons/btn_shop` | `btn_SHOP` |
| `Buttons/btn_no_ads` | `btn_NO_ADS` |
| `Icons/icon_settings` | `icon_SETTINGS` |
| `Icons/icon_skin` | `icon_skin` |
| `Icons/icon_shop` | `icon_shop_skin` |
| `Icons/icon_home` | `icon_HOME` |
| `Panels/box_popup` | `box_POPUP` (border 60 đặt thủ công) |
| `Panels/box_endgame_win` | `box_Endgame_WIN` (border 60) |
| `Panels/box_endgame_lose` | `box_endgame_LOSE` (border 60) |
| `Characters/endgame_worm_win` | `endgame_worm_WIN` |
| `Buttons/btn_green` | `btn_AD` (nút xanh lá trống, dùng cho Watch Ad) |
| `Icons/icon_return` | `icon_RETURN` |
| `Buttons/btn_toggle_on` | `btn_ON` |
| `Buttons/btn_toggle_off` | `btn_OFF` |
| `Icons/icon_music_on` | `icon_music_ON` |
| `Icons/icon_music_off` | `icon_music_OFF` |
| `Icons/icon_sound_on` | `icon_sound_ON` |
| `Icons/icon_sound_off` | `icon_sound_OFF` |
| `Icons/icon_vibration_on` | `icon_vibration_ON` |
| `Icons/icon_vibration_off` | `icon_vibration_OFF` |
| `Cosmetics/skin_card` | `box_shop_skin` |
| `Cosmetics/skin_card_selected` | `box_selected_skin` |
| `Cosmetics/btn_skin_free` | `btn_free` |
| `Cosmetics/btn_skin_price` | `btn_price` |
| `Cosmetics/icon_skin_selected` | `icon_selected_skin` |
| `Icons/icon_dpad_arrow` | `icon_MOVE` |
| `Icons/icon_restart` | `icon_RESET` |
| `Icons/icon_hint` | `icon_hint` |
| `Icons/icon_undo` | `icon_undo` |
| `Backgrounds/bg_iceberg` | `BG4` |
| `Icons/badge_count` | `box_hint_number` |
| `Currency/coin` | `coin` |
| `Currency/coin_pill` | `box_COIN_0` |

`home.png` của V1 là sprite atlas (2048×2048) chứa nhiều mảnh nhân vật, không phải ảnh màn Home,
nên không mang sang.
