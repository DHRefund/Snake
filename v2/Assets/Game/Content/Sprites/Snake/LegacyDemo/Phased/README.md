# V1 phased snake demo

`Assets/Game/Resources/SnakeSkins/LegacyV1SnakeSkin.asset` là cấu hình duy nhất mà renderer cần
tham chiếu. Skin gồm:

- một sprite đầu V1;
- tám phase thân thẳng (`body_0` đến `body_7`);
- tám phase góc cua (`1_24` đến `8_29`, dùng lại asset đã có trong thư mục Snake);
- tám phase đuôi (`tail_0` đến `tail_7`).

Mỗi node sau đầu dùng phase `(segmentIndex - 1) % 8`. Renderer chọn straight hoặc corner từ
topology của hai node lân cận, sau đó xoay sprite chuẩn đến đúng hướng. Tail cũng dùng phase của
vị trí nó trong chuỗi nên màu không nhảy khi chiều dài thay đổi.

Đầu V1 rộng 1.71 ô và được vẽ sau cùng. Phần tràn che lên mối nối body là chủ ý của asset gốc,
không phải body đè lên đầu. Khi thay bằng bộ Classic một-node, chỉ cần đặt `headWidthInCells = 1`.

Các phase đang dùng thuộc cùng một family của `SkinDefault`. Những family đảo chiều còn lại chưa
được đưa vào demo này vì code lựa chọn family của APK đã mất khi decompile; không nên đoán rồi gắn
nhầm. `SnakeSpriteSkin` đã tách data khỏi renderer để có thể bổ sung family sau mà không sửa scene.
