# Gameplay Objects & Skins Architecture

## 1. Mục tiêu và Nguyên tắc Thiết kế
Presentation layer của game áp dụng mô hình Skin-driven (tương tự `AutotileBlockSkin` và `SnakeSpriteSkin`):
1. **Zero Domain coupling**: Domain/Logic không phụ thuộc bất kỳ class UnityEngine hay Sprite nào. Logic quản lý các cell bằng mã ký tự (`'2'`, `'8'`, `'X'`, `'g'`, `'h'`, `'i'`, `'j'`, `'6'`, `'A'`, v.v.) và state dictionary (`Movables`, `Doors`, `SlidePlatforms`).
2. **Skin ScriptableObject (`GameplayItemSkin`)**: Tập trung quản lý toàn bộ sprite tài nguyên của các object trên board (thức ăn, exit portal, chướng ngại vật, movables, cơ chế).
3. **Asset Resource Lazy Loading**: Nếu scene chưa serialize reference trong editor, `GameplayScreenController.EnsureItemSkin()` tự động nạp từ `Resources/ItemSkins/DefaultItemSkin`.
4. **Fallback an toàn**: Nếu một sprite chưa được gán, hệ thống tự động fallback về màu solid và label text chuẩn, không gây crash hoặc lỗi null ref.

---

## 2. Bảng Phân bố Ký tự & Sprites

| Ký tự / Loại | Tên Object | Tệp Sprite V2 | Xoay / Visual State |
| :--- | :--- | :--- | :--- |
| `'2'` | Táo thường | `Items/apple_normal.png` | Centered, preserve aspect |
| `'8'` | Táo VIP / Vàng | `Items/apple_vip.png` | Centered, preserve aspect |
| `'X'` (Mở) | Cổng Exit mở | `Environment/exit_portal.png` | `Color.white` |
| `'X'` (Khóa) | Cổng Exit khóa | `Environment/exit_portal.png` | Tint xám mờ (`0.5, 0.5, 0.55, 0.9`) |
| `'g'` | Gai nhọn (Lên) | `Mechanics/spike.png` | Xoay 0° |
| `'h'` | Gai nhọn (Phải) | `Mechanics/spike.png` | Xoay 270° (-90°) |
| `'i'` | Gai nhọn (Xuống) | `Mechanics/spike.png` | Xoay 180° |
| `'j'` | Gai nhọn (Trái) | `Mechanics/spike.png` | Xoay 90° |
| `'6'` | Vũng axit độc | `Mechanics/toxic_acid.png` | Centered |
| `'A'` | Mạng nhện | `Mechanics/spider_net.png` | Centered |
| `MovableKind.Rock` | Tảng đá đẩy | `Movables/rock.png` | Centered (che hazard bên dưới) |
| `MovableKind.Bomb` | Quả bom TNT | `Movables/bomb.png` | Centered |
| `SlidePlatform` | Thanh trượt | `Mechanics/slide_platform.png` | Centered theo vị trí hiện tại |
| `LMNOQRST` | Nút sàn | `button_floor_released.png` / `button_floor_pressed.png` | Đổi sprite theo trạng thái `IsButtonPressed` |
| `lmno` (Đóng) | Cửa chặn | `door_bar_blue.png` / `door_bar_green.png` / `door_lock.png` | Hiển thị thanh chắn khi đóng |
| `ceCE` / `dfDF` | Cổng dịch chuyển | `teleport_gate_blue.png` / `teleport_gate_orange.png` | Xanh dương / Cam |
| `qrst` | Súng laser | `Mechanics/laser_gun.png` | Centered |
| `'k'` | Công tắc kích nổ | `Mechanics/bomb_remote.png` | Centered |

---

## 3. Cấu trúc Lớp Hiển thị (`ObjectVisual`)
Trong mỗi cell UI của `BoardGrid`:
- `cellImages[position]` (Image gốc của Cell): Đặt `color = Color.clear` khi có sprite object để tránh lộ background màu placeholder.
- `cellLabels[position]` (Text): Xóa rỗng text ("APPLE", "EXIT", v.v.).
- `ObjectVisual` (Child RectTransform + Image):
  - Pivot: `(0.5, 0.5)`
  - Anchor: `(0.5, 0.5)`
  - Size: Fit theo `cellSize` với `preserveAspect = true`.
  - Local Rotation: Xoay theo góc hướng (cho các loại gai `'g', 'h', 'i', 'j'`).
  - Khi cell trở thành ô trống (ăn táo, dời đá), `SetCell` tự động destroy `ObjectVisual` để không để lại bóng ma đồ họa.

---

## 4. Kiến trúc Snake Skin (`SnakeSpriteSkin`) & Tiêu chuẩn Thiết kế Asset (Clean-Room)

### 4.1. Tiêu chuẩn Kích thước & Lưới (Figma ➔ Unity)
Để đảm bảo hiển thị sắc nét trên màn hình Retina/High-DPI và tương thích tuyệt đối với hệ thống Autotile Block `256 × 256 px`:
- **Canvas / Bounding Box**: Cố định `256 × 256 px` cho toàn bộ các frame, nền trong suốt (Transparent PNG 32-bit).
- **Pivot Point**: Chính giữa `(0.5, 0.5)`. Khi xoay 0°, 90°, 180°, 270°, sprite không bao giờ bị lệch tâm khỏi ô lưới.
- **Hệ thống Viền (Border Outline)**: Độ dày chuẩn `8 px` ở mỗi mép ngoài. Viền được dựng bằng filled shape/contour hình học (thay vì vector stroke thông thường) để dễ dàng tùy biến màu viền cho các skin khác nhau.
- **Không áp đặt Highlight cố định**: Lớp ruột bên trong để mở linh hoạt, mỗi theme skin tự do phối màu (màu phẳng, gradient, vân sọc, bóng sáng).

### 4.2. Bộ 4 Sprite Nền tảng
Mỗi bộ Skin rắn hoàn chỉnh bao gồm tối thiểu 4 frame cơ bản:

1. **`Snake_[Theme]_Head.png` (Đầu rắn)**:
   - Cổ nối (bên Trái `X = 0`): Phẳng lì, cao `256 px`, không có viền trái để nối khít 100% với thân.
   - Mõm trước (bên Phải `X = 256`): Bo tròn 2 góc Top-Right & Bottom-Right với bán kính `R = 80 px`.
   - Viền `8 px` chạy dọc mép trên, mép dưới và ôm trọn cung tròn mõm trước.
   - Gương mặt Chibi: Đôi mắt to tròn, má hồng, tai hoặc sừng (nằm trong ranh giới `256 × 256`).
   - Xoay hướng & Lật gương: Khi bò sang Phải (0°), Lên (+90°), Xuống (-90°). Riêng khi bò sang Trái, renderer áp dụng Lật ngang (`scale.x = -1, rotation = 0°`) để tai và mắt luôn ngửa lên trên tự nhiên, không bị lộn ngược đỉnh đầu cắm xuống sàn.

2. **`Snake_[Theme]_Body.png` (Thân thẳng)**:
   - Kích thước `256 × 256 px`, nối thẳng từ Trái sang Phải.
   - Viền trên: `X = 0..256, Y = 0..8` (dày 8px).
   - Viền dưới: `X = 0..256, Y = 248..256` (dày 8px).
   - Hai cạnh Trái và Phải hở hoàn toàn để kết nối liền mạch với các đốt thân kế tiếp.

3. **`Snake_[Theme]_Corner.png` (Khúc cua 90°)**:
   - Kích thước `256 × 256 px`, dùng cho mọi góc rẽ (Unity tự động xoay 4 góc bằng `CornerRotation`).
   - Cung bo góc ngoài (Top-Left): Bo tròn bán kính `R = 80 px` (hoặc `96 px`), viền ngoài dày `8 px`.
   - Cạnh Phải và Cạnh Dưới: Phẳng lì, màu thân ruột hở để tiếp giáp thẳng với đốt thân dọc và đốt thân ngang.
   - Quy tắc xoay góc (`CornerRotation`):
     - `0°`: Nối Phải (`1, 0`) & Dưới (`0, -1`) — cùi chỏ ngoài ở Top-Left.
     - `+90°`: Nối Trên (`0, 1`) & Phải (`1, 0`) — cùi chỏ ngoài ở Bottom-Left.
     - `180°`: Nối Trái (`-1, 0`) & Trên (`0, 1`) — cùi chỏ ngoài ở Bottom-Right.
     - `-90°`: Nối Dưới (`0, -1`) & Trái (`-1, 0`) — cùi chỏ ngoài ở Top-Right.

4. **`Snake_[Theme]_Tail.png` (Đuôi rắn)**:
   - Khớp nối thân (bên Phải `X = 256`): Phẳng lì, cao `256 px`, ăn khớp với thân thẳng.
   - Chóp đuôi (bên Trái `X = 0`): Bo tròn 2 góc Top-Left & Bottom-Left với bán kính `R = 80 px`, để lại đoạn thẳng ở giữa `96 px` tạo dáng đuôi Chibi mập mạp.
   - Viền `8 px` ôm quanh chóp đuôi và chạy thẳng ở 2 mép trên/dưới.

### 4.3. Danh mục Tài nguyên ScriptableObject
- **`Assets/Game/Resources/SnakeSkins/CatSnakeSkin.asset`** (Default Production Skin):
  - Theme: Rắn Mèo Chibi (`Snake_Cat`) màu be, sọc tím than, tai vểnh, biểu cảm mắt híp cười.
  - Cấu hình: `HeadWidthInCells = 1`, `HeadHeightInCells = 1`, 1 phase cho Body, Corner, Tail.
- **`Assets/Game/Resources/SnakeSkins/LegacyV1SnakeSkin.asset`** (Demo V1 Reference):
  - Theme: Rắn cam nguyên bản V1 (1.71 cell head, 8 crawling phases).

### 4.4. Tích hợp Runtime
`GameplayScreenController.EnsureSnakeSkin()` tự động kiểm tra `LocalProfileStore.SelectedSkin`:
- `SelectedSkin == 0`: Tự động nạp `SnakeSkins/CatSnakeSkin` (mặc định mới).
- `SelectedSkin == 1`: Tự động nạp `SnakeSkins/LegacyV1SnakeSkin` (hoặc skin kế tiếp).
- Fallback an toàn về `DefaultSnakeSkinResource` nếu thiếu asset.

---

## 5. Kiến trúc Hybrid: Spine Head + Grid Sprite Body & Tail (Chuẩn VIP ADR-007)

Nhằm đảm bảo visual đạt chuẩn xuất sắc (ngang hoặc vượt V1) mà vẫn giữ hiệu năng siêu nhẹ và dễ mở rộng, V2 chính thức áp dụng mô hình kiến trúc Hybrid:

### 5.1. Phân chia Trách nhiệm Kiến trúc
1. **Đầu rắn (Head Visual)**:
   - Sử dụng **Spine 2D Runtime** (`SkeletonGraphic` hoặc `SkeletonAnimation`) gắn trực tiếp trên ô đầu rắn.
   - Điều khiển bằng component `SnakeSpineHead`.
   - Sử dụng khung xương đa dụng 132 bones từ `skeleton_1.json` để thực hiện biến dạng lưới mềm (mesh deformation): phồng má khi nhai, há to mồm khi thèm táo, đảo mắt, chớp mắt, choáng khi va tường.
2. **Thân và Đuôi (Body & Tail Visual)**:
   - Sử dụng **2D UI Image / SpriteRenderer** sắp xếp trực tiếp theo grid cell của `boardGrid`.
   - Mỗi đốt là 1 GameObject đơn giản có component `Image` hoặc `SpriteRenderer`.
   - Định hướng và xoay góc ($0^\circ, 90^\circ, 180^\circ, 270^\circ$) bằng code thông qua vector giữa đốt trước và đốt sau (`previous` và `next`).

### 5.2. Danh sách 12 Animation Spine & Trigger Máy Trạng thái (FSM)
| Animation | Mục đích biểu cảm | Điều kiện kích hoạt trong V2 |
| :--- | :--- | :--- |
| `idle_1` / `idle_2` | Đứng thở, thỉnh thoảng chớp mắt | Trạng thái nghỉ mặc định (loop = true) |
| `open_mouth` | Há to mồm chuẩn bị đớp mồi | Khi ô lân cận theo hướng di chuyển là Quả Táo (`'2'` hoặc `'8'`) |
| `eating` | Phồng má nhai nhóp nhép | Ngay sau khi nuốt táo thành công |
| `hit_wall` | Choáng váng, mắt nheo, má giật | Khi người chơi bấm đi vào tường/chướng ngại vật bị từ chối (`BumpHead`) |
| `die` / `end_lose` | Nhắm mắt, thè lưỡi ngoẹo đầu | Khi rơi vào bẫy gai (`'g'..'j'`), axit (`'6'`) hoặc rơi ra khỏi map |
| `win` / `end_win` | Cười híp mắt, nhe răng vui sướng | Khi toàn bộ thân rắn đã chui vào cổng Exit Portal (`'X'`) |
| `start` | Thức dậy, vươn vai | Khi vừa nạp xong level mới |

### 5.3. Bảng Ánh xạ Slot & Xương cho Skin mới (Ví dụ: Husky / Dog)
Dựa trên bộ khung xương `skeleton_1.json` và atlas `skeleton.png`, skin chó/mèo sử dụng các slot sau:
- **Đầu (`head`)**: Bounding box `312 × 296 px` (gắn xương `head_1` / `head_5`).
- **Tai trước (`f_ear`) & Tai sau (`b_ear`)**: Bounding box `136 × 168 px` (dao động lắc lư theo hàm chuyển động).
- **Mõm & Hàm (`mouth` / `mouth_eat`)**: Bounding box `176 × 136 px` (kéo giãn khi mở mồm).
- **Mũi (`nose`)**: Bounding box `72 × 56 px` (dành cho skin chó/husky).
- **Mắt (`L_eye`, `R_eye`) & Mí mắt**: Bounding box `88 × 88 px` (chớp mí trên/dưới và đảo đồng tử `pupil`).
- **Má (`cheek`)**: Bounding box `80 × 64 px` (phồng to khi nhai).

### 5.4. Quy trình Nghệ sĩ (Artist Workflow) Tạo Skin mới trong 3 Bước:
1. **Bước 1 (Vẽ Đầu trên Figma)**: Vẽ các bộ phận đầu (mặt, 2 tai, 2 mắt, mõm) theo đúng kích thước bounding box chuẩn của `skin_dog` hoặc `skin_cat`.
2. **Bước 2 (Vẽ Thân & Đuôi)**: Vẽ 3 hình phẳng $256 \times 256\text{ px}$:
   - `Snake_[Theme]_Body.png`: Thân thẳng hở 2 đầu trái phải.
   - `Snake_[Theme]_Corner.png`: Thân cua bo tròn góc $90^\circ$.
   - `Snake_[Theme]_Tail.png`: Đuôi bo nhọn tròn về phía sau.
3. **Bước 3 (Gán vào Unity)**: Tạo 1 `SnakeSpriteSkin` ScriptableObject:
   - Điền `spineSkinName = "skin_dog"` (hoặc tên skin mới).
   - Kéo 3 ảnh thân vào Inspector.
   - Hoàn tất! Skin mới hoạt động ngay lập tức với đầy đủ 12 animation VIP.
