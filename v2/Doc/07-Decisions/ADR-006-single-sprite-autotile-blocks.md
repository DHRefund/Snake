# ADR-006: Single-sprite autotile cho block địa hình

Status: Accepted  
Date: 2026-10-25

## Context

Block V1 xác định hình theo bốn hàng xóm nhưng dựng một node từ body và nhiều layer cạnh/góc.
Cách này tái sử dụng asset tốt, song tạo nhiều renderer, phụ thuộc pivot/offset và phức tạp khi
đưa vào UI grid của V2. Phương án dùng một viên hoàn chỉnh rồi chồng các viên kế tiếp cũng phụ
thuộc draw order và không giải quyết ổn định các góc lõm, chữ T hoặc vòng kín.

## Decision

V2 dùng autotile 4 hướng với connection mask 4 bit. Mỗi mask ánh xạ đến một hoặc nhiều sprite
hoàn chỉnh; mỗi cell chỉ render một sprite. Các biến thể được chọn deterministic theo level và
tọa độ. Sprite của một skin dùng chung atlas/material.

Chi tiết asset, naming, import và validation nằm tại
[Block Autotile Rendering](../02-Architecture/05-block-autotile-rendering.md).

## Alternatives

- **Chồng một sprite block duy nhất:** ít asset nhưng dễ lộ viền nội bộ và sai draw order.
- **Ghép body + edge + corner như V1:** đúng hình và linh hoạt, nhưng tăng renderer và chi phí
  căn chỉnh/runtime complexity.
- **Unity Rule Tile ngay lập tức:** hợp lệ, nhưng thêm package/workflow trong khi board hiện tại
  đã có dữ liệu grid và lookup riêng.

## Consequences

- Mỗi skin cần tối thiểu 16 sprite đã bake; ba biến thể cần 48 sprite.
- Runtime đơn giản hơn, không phụ thuộc thứ tự chồng tile và dễ batch hơn.
- Cần công cụ bake/validation để chuyển bộ layer V1 thành sprite production.
- `LegacyDefault` tiếp tục tồn tại làm reference trong giai đoạn migration, không định nghĩa
  format skin production.

