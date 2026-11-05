# Block Autotile Rendering

Status: Accepted  
Date: 2026-10-25

## 1. Quyết định chính

Block địa hình của V2 dùng **autotile 4 hướng**. Mỗi node trên grid được render bằng đúng
**một sprite hoàn chỉnh**, đã bao gồm mặt chính, viền, độ dày và các góc cần thiết.

Không dùng một sprite chung rồi cho các node chồng lên nhau. Không dựng block production bằng
nhiều GameObject con cho cạnh trên, cạnh trái và góc như renderer V1.

## 2. Connection mask

Mỗi block kiểm tra bốn ô trực giao cùng nhóm kết nối:

```text
      UP = 1
        │
LEFT=8 ┼ RIGHT=2
        │
     DOWN = 4
```

```csharp
var mask = 0;
if (Connects(up)) mask |= 1;
if (Connects(right)) mask |= 2;
if (Connects(down)) mask |= 4;
if (Connects(left)) mask |= 8;

sprite = skin.Variants[mask].PickStable(position);
```

Mask có giá trị từ `0` đến `15`, tương ứng đủ 16 topology: block đứng riêng, đầu mút,
đoạn thẳng, góc, chữ T, và block được bao quanh cả bốn phía.

Hai block chỉ nối hình ảnh khi chúng có cùng `connectivity group`. Quy tắc này cho phép tường
thường, tường phá được hoặc block cơ chế dùng skin khác mà không vô tình nối vào nhau.

## 3. Quy chuẩn asset của một skin

Một skin production tối thiểu có:

- 16 sprite hoàn chỉnh, mỗi mask một sprite.
- Khuyến nghị 3 biến thể bề mặt cho mỗi mask: tổng cộng 48 sprite.
- Một ảnh preview dùng trong editor/cosmetics nếu skin xuất hiện trong UI.
- Một cấu hình production ánh xạ trực tiếp `mask -> variants`. `BlockSpriteSkin` hiện tại là
  schema legacy trong giai đoạn migration và sẽ không được dùng làm schema cuối.

Tên file chuẩn:

```text
block_<skin>_m00_v01.png
block_<skin>_m00_v02.png
block_<skin>_m00_v03.png
...
block_<skin>_m15_v03.png
```

Trong đó:

- `m00..m15` là connection mask, luôn ghi hai chữ số.
- `v01..v03` là biến thể hình ảnh, không thay đổi topology hoặc điểm nối.

## 4. Quy chuẩn hình ảnh và import

### 4.1. Góc nhìn thống nhất

Tất cả skin block dùng cùng một góc nhìn giả lập 2.5D:

- Camera ở **phía trên-trái**, nhìn xuống dưới-phải.
- Một block có thể lộ **mặt trước, mặt trên và mặt trái**.
- Không tạo skin nhìn từ bên phải và không mirror sprite để đổi hướng nhìn.
- Ánh sáng mặc định đi từ trên-trái; mặt trên sáng nhất, mặt trái sáng vừa và mặt trước tối hơn.

Đây là hướng nhìn của V1. Bộ asset V1 có các lớp `LeftTop`, `LeftMid`, `LeftBot`, `TopLeft`,
`TopMid` và `TopRight`; V2 giữ quy ước này để các skin, object và background không mâu thuẫn phối cảnh.

### 4.2. Logical footprint và visual overscan

Kích thước chuẩn của source art:

```text
Mặt trước/logical footprint:  256 x 256 px
Mặt trái bè sang trái:         62 px
Mặt trên nhô lên:              65 px
Vector chiều sâu:             (-62, +65) px
Visual bounds chưa tính viền: 318 x 321 px
```

`256 x 256 px` là diện tích gameplay của đúng một node. Mặt trên và mặt trái chỉ là visual và được phép
tràn sang ô phía trên/trái; chúng không tạo thêm collision và không thay đổi tọa độ grid của block.

Trong hệ tọa độ source, có thể quy ước mặt trước chiếm `x = 0..256`, `y = 0..256`; phần chiều sâu
mở rộng tới `x = -62` và `y = 321`. Canvas xuất PNG phải chứa toàn bộ visual bounds cộng thêm khoảng
an toàn cho outline. Canvas không bắt buộc vuông.

Pivot logic nằm tại **tâm mặt trước** `(128, 128)`, không nằm tại tâm của toàn bộ visual bounds. Nếu PNG
không có padding, vị trí pivot chuẩn hóa tương ứng xấp xỉ `(0.598, 0.399)`. Khi có padding, phải tính lại
pivot theo vị trí thật của tâm mặt trước; không copy cứng giá trị chuẩn hóa này.

Runtime nên tách logical root khỏi visual để không phụ thuộc vào kích thước canvas:

```text
BlockCell       1 ô grid, pivot ở tâm logical footprint
└── BlockVisual sprite hoàn chỉnh 318 x 321 px + outline/padding, tràn lên trên-trái
```

`BoardGrid` không được dùng `Mask`/`RectMask2D` làm cắt phần overscan. Khung board phải chừa tối thiểu
62 px bên trái và 65 px phía trên theo tỷ lệ source, cộng thêm outline/padding.

### 4.3. Yêu cầu chung

Tất cả sprite trong cùng skin phải có:

- Cùng logical footprint `256 x 256 px` và cùng vector chiều sâu `(-62, +65) px`.
- Cùng quy ước canvas, padding, pivot, Pixels Per Unit và import settings.
- Điểm nối chạm chính xác bốn cạnh tile, cùng bề rộng ở mọi mask.
- Viền chỉ xuất hiện ở chu vi ngoài; cạnh có hàng xóm không được còn đường viền hoặc khe hở.
- Không chứa bóng đổ vượt sang tile khác. Nếu cần bóng chung, render bằng một layer riêng cho
  toàn board, không giải quyết bằng thứ tự đè giữa các block.
- `Sprite Mesh Type = Full Rect` để hình học và batching có hành vi ổn định.
- Các sprite runtime của một skin được đóng chung một Sprite Atlas và dùng cùng material.

Source art có thể giữ layer/vector tùy công cụ, nhưng file runtime phải là sprite đã flatten/bake.

## 5. Chọn biến thể

Biến thể không được random lại mỗi lần refresh hoặc khi rắn di chuyển. Chọn biến thể bằng hash
deterministic từ tọa độ và seed của level:

```text
variant = StableHash(levelSeed, x, y, skinId) % variantCount
```

Undo, restart, replay và load lại level phải cho ra cùng một bề mặt block.

## 6. Runtime và hiệu năng

- Một node dùng một `Image` hoặc `SpriteRenderer`; không sinh renderer con cho từng cạnh/góc.
- Cache sprite lookup trong mảng 16 phần tử; không tìm asset bằng tên trong mỗi frame.
- Chỉ tính lại mask khi board topology thay đổi. Rắn di chuyển không làm thay đổi wall topology.
- Tái sử dụng/pool view của cell; không bắt buộc rebuild toàn board sau mỗi turn.
- Ưu tiên một atlas/material cho cả skin để Unity có thể batch.
- Chỉ chuyển sang mesh batching hoặc Tilemap Renderer khi profiling chứng minh số cell hiện tại
  là bottleneck.

## 7. Validation bắt buộc

Trước khi một skin được chấp nhận:

1. Render một bảng test chứa đủ 16 mask.
2. Kiểm tra không có khe, viền nội bộ hoặc cạnh bị cắt ở nhiều kích thước cell.
3. Kiểm tra ba biến thể của cùng mask có cùng silhouette và điểm nối.
4. Kiểm tra một hình chữ U, chữ T, vòng kín và block đứng riêng.
5. Di chuyển, undo, restart và reload để xác nhận biến thể không đổi.
6. Kiểm tra Frame Debugger: các block cùng skin dùng cùng texture/material.
7. Kiểm tra mọi sprite đều nhìn từ trên-trái, không có mask hoặc variant bị mirror sai hướng.
8. Kiểm tra mặt trước vẫn bám đúng ô logic, còn phần tràn 62 px/65 px không bị board hoặc cell cắt mất.

## 8. Trạng thái migration từ V1

V1 đã dùng đúng ý tưởng topology 4 hướng và có 16 nhóm body, mỗi nhóm ba biến thể. Tuy nhiên,
mỗi node V1 còn ghép thêm các layer cạnh/góc. Bộ `LegacyDefault` hiện có trong V2 được giữ làm:

- bằng chứng hình ảnh/reference;
- renderer chuyển tiếp để đối chiếu V1;
- nguồn đầu vào cho bước bake 48 sprite production.

Nó không phải format asset cuối cùng. Khi bộ `Default/Runtime` hoàn tất và qua validation,
Gameplay phải chuyển sang lookup một sprite/node; sau đó renderer ghép layer được loại bỏ.
