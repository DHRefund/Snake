# Danh sách skin rắn: V1 → V2

Nguồn dữ liệu:
- V1: `SnakeApple_ExportedProject/ExportedProject/Assets/MonoBehaviour/Skin*.asset` (class `SkinData`).
- V2: `v2/Assets/Game/Resources/SnakeSkins/SnakeSkinCatalog.asset`, `Skin_*.asset`,
  `v2/Assets/Game/Editor/SnakeSkinLibraryBuilder.cs`, `Doc/01-Game-Design/04-progression-economy.md` (mục 6.1).
- Màu: trích từ sprite `Content/Sprites/Snake/Skins/<key>/body_0.png` và `head.png` (bỏ pixel trong suốt).

## 1. Tổng quan

- V1 có 12 skin (id 0–11), mở khóa bằng số lần xem quảng cáo (`adRequire`).
- V2 có 12 slot, mở khóa bằng coin. Thứ tự là index lưu trong save: chỉ thêm vào cuối, không đổi thứ tự.
- Mỗi skin V2 là một `SnakeSpriteSkin`: sprite thân dùng 8 phase cho body, corner và tail, còn đầu là Spine
  (`useSpineHead = true`, `spineSkinName`) trừ skin classic.

## 2. Bảng đối chiếu V1 → V2

| Slot V2 | id V2 | Tên hiển thị | Giá V2 (coin) | V1 id | V1 `skinName` | V1 `adRequire` | Spine skin (V2) | Thư mục sprite |
|---|---|---|---|---|---|---|---|---|
| 0 | cat | Cat Snake | 0 | 1 | skin_cat | 0 | `skin_cat` | (asset riêng `CatSnakeSkin`) |
| 1 | classic | Classic Worm | 150 | 0 | skin_default | 0 | không dùng Spine | (asset riêng `LegacyV1SnakeSkin`) |
| 2 | nemo | Clownfish | 200 | 2 | skin_nemo | 0 | `skin_nemo` | `nemo` |
| 3 | dog | Puppy | 250 | 3 | skin_dog | 0 | `skin_dog` | `dog` |
| 4 | bird | Birdie | 300 | 4 | skin_bird | 0 | `skin_bird` | `bird` |
| 5 | frog | Froggy | 350 | 5 | skin_frog | 0 | `skin_frog` | `frog` |
| 6 | tiger | Tiger | 400 | 6 | skin_tiger | 0 | `skin_tiger` | `tiger` |
| 7 | bear | Bear | 450 | 7 | skin_bear | 0 | `skin_bear` | `bear` |
| 8 | unicorn | Unicorn | 500 | 8 | skin_unicorn | 2 | `skin_unicorn` | `unicorn` |
| 9 | dragon | Dragon | 550 | 9 | skin_dragon | 2 | `skin_dragon` | `dragon` |
| 10 | sorcerer | Sorcerer | 600 | 10 | skin_docter_strange | 3 | `skin_docter_strange` | `doctor_strange` |
| 11 | thunder | Thunder | 700 | 11 | skin_thor | 3 | `skin_thor` | `thor` |

Ghi chú:
- Việc `classic` ↔ V1 `skin_default` là suy ra từ `LegacyV1SnakeSkin` (đầu 1.71×0.98 ô, không Spine), chưa có tài liệu xác nhận.
- Tên V1 `skin_docter_strange` viết sai chính tả "docter" và được giữ nguyên làm tên Spine skin.
- Mỗi skin V1 có đủ danh sách sprite: `headSprite`, `listBodySprite1/2`, `listTailSprite1/2`,
  `listCornerSprite1–4` (mỗi list 8 sprite). V2 gom lại thành `bodyPhases`, `cornerPhases`, `tailPhases` (8 phase mỗi loại).
- V1 `skinSprite` là icon hiển thị trong shop. V2 dùng `preview` trong catalog.

## 3. Màu chủ đạo của từng skin

Màu được lượng tử hóa 16 mức mỗi kênh nên sai số khoảng ±8. Phần trăm là tỉ lệ trên pixel không trong suốt
của sprite. Màu `#280808` gần như là viền nâu đen và có ở mọi skin (khoảng 6–7%).

| Skin | Thân (body_0) | Đầu (head) |
|---|---|---|
| bear | `#B85828` nâu đỏ (90%) | `#D87838` nâu cam (91%) |
| bird | `#C80808` đỏ (91%) | `#F83818` đỏ cam (74%), `#F8D878` vàng nhạt (15%) |
| dog | `#F89818` cam (91%) | `#F8C818` vàng (68%), `#F8E8B8` kem (20%) |
| dragon | `#C80808` đỏ (83%), `#A80808` đỏ sẫm (7%) | `#F83818` đỏ cam (89%) |
| frog | `#589818` xanh lá đậm (65%), `#F8E8B8` kem (23%) | `#78C828` xanh lá sáng (66%), `#F8E8B8` kem (23%) |
| nemo | `#F8E8B8` kem (73%), `#484838` xám đen (16%) | `#F86808` cam (90%) |
| tiger | `#F8F8D8` kem (48%), `#F86808` cam (31%), `#180808` đen (5%) | `#F8A818` vàng cam (57%), `#F8F8D8` kem (25%) |
| unicorn | `#F88898` hồng (72%), `#F8F8E8` trắng (18%) | `#F8E8E8` trắng hồng (91%) |
| doctor_strange (sorcerer) | `#2868A8` xanh dương (59%), `#D83858` đỏ (20%), `#B82848` đỏ đậm (9%) | `#F8E8A8` da (72%), `#384848` xám đen (11%), `#A8B8B8` bạc (5%) |
| thor (thunder) | `#485858` xám xanh (61%), `#D83858` đỏ (21%), `#B82848` đỏ đậm (8%) | `#687878` xám (43%), `#F8E8A8` da (21%), `#F8D828` vàng (15%) |
| cat | chưa trích: skin này dùng asset riêng với neck sprite và Spine | |
| classic | chưa trích: dùng sprite `LegacyV1SnakeSkin` riêng | |

## 4. Khác biệt V1 và V2 về cách mở khóa

| | V1 | V2 |
|---|---|---|
| Tiền tệ | Số lần xem quảng cáo (`adRequire`: 0, 2 hoặc 3) | Coin, giá cố định trong catalog |
| Skin miễn phí | 8 skin (id 0–7) | 1 skin (slot 0) |
| Skin đắt nhất | 3 quảng cáo (Doctor Strange, Thor) | 700 coin (Thunder) |
| Cấu trúc | `SkinData` ScriptableObject, toàn sprite 2D | `SnakeSpriteSkin` (sprite thân) kèm Spine skin cho đầu |

Giá V2 có thể chỉnh trong `SnakeSkinCatalog` mà không cần sửa code.

## 5. Bảng màu mới đã áp dụng (thay cho màu V1)

Đã đổi màu sprite thân/góc/đuôi/preview trong `Content/Sprites/Snake/Skins/<key>/` và texture atlas đầu Spine
(`Resources/Spine/skeleton*.png`, `Content/Spine/SnakeHead/skeleton*.png`). Hoa văn, viền và animation giữ nguyên.
Catalog chỉ còn 10 skin V1 bên dưới, không còn cat và classic.

| Slot | Skin | Giá | Tối → mới | Sáng → mới |
|---|---|---|---|---|
| 0 | nemo | 0 | `#FF6D02` → `#E84C7D` | `#FCA418` → `#F27A9E` |
| 1 | dog | 150 | `#FF9C18` → `#7B4A2E` | `#FFC518` → `#A9693F` |
| 2 | bird | 200 | `#C60000` → `#0E98B0` | `#F23D13` → `#3FD0E0` |
| 3 | frog | 250 | `#589A1F` → `#7B3FC0` | `#75C82B` → `#A56BE8` |
| 4 | tiger | 300 | `#FF6D02` → `#E8A200` | `#FFA418` → `#FFD84A` |
| 5 | bear | 350 | `#B35827` → `#4F6D8F` | `#D67331` → `#7C9BBD` |
| 6 | unicorn | 450 | `#FF8394` → `#7FE0C4` | `#FFE0E9` → `#DFFBF3` |
| 7 | dragon | 550 | `#C60000` → `#0B7A4B` | `#F23D13` → `#1FBF7A` |
| 8 | sorcerer | 600 | `#2466A8` → `#1A8A8A`, `#D43750` → `#E08A1E` | `#4A94E6` → `#3FC0B8`, `#B02C45` → `#B86E10` |
| 9 | thunder | 700 | `#4C545C` → `#2B3A67`, `#D43750` → `#F0B72A` | `#687078` → `#46598F`, `#B02C45` → `#C48A12` |

Slot trong save là index trong catalog. Vì cat và classic bị bỏ nên index cũ đã đổi: reset
`profile.cosmetics.selectedSkin` và danh sách skin đã mở khi test.
