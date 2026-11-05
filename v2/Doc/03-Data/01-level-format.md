# Định dạng và pipeline dữ liệu level

**Trạng thái:** Accepted

## 1. Mục tiêu

- Human-readable và diff-friendly trong source control.
- Có schema version.
- Validate được ngoài Play Mode.
- Không phụ thuộc fileID/GUID của prefab Unity để mô tả logic.
- Import được dữ liệu ký tự từ bản AssetRipper.
- Hỗ trợ stable ID cho entity, trigger và teleport.

## 2. Định dạng canonical chính thức

JSON là canonical source cho V2 giai đoạn đầu vì dễ sinh, đọc và validate. Có thể
import vào ScriptableObject cache để dùng trong build, nhưng JSON vẫn là nguồn gốc.

```json
{
  "schemaVersion": 1,
  "id": "normal-001",
  "mode": "normal",
  "displayNumber": 1,
  "size": { "width": 10, "height": 8 },
  "gravity": "down",
  "initialResolve": false,
  "tiles": [
    "##########",
    "#........#",
    "#..a.....#",
    "#........#",
    "#..tooH..#",
    "#......E.#",
    "##########",
    "##########"
  ],
  "entities": [],
  "links": [],
  "solution": {
    "rulesVersion": 1,
    "commands": ["right", "right", "right"]
  },
  "metadata": {
    "difficulty": 1,
    "tags": ["tutorial", "apple"],
    "author": "team",
    "notes": "Introduces the exit rule"
  }
}
```

Ví dụ trên chỉ minh họa schema; validator phải quyết định ký hiệu chính thức.

## 3. Symbol table dành cho authoring

| Ký hiệu | Semantic |
|---|---|
| `.` | Empty floor |
| `#` | Wall |
| `H` | Snake head |
| `o` | Snake body |
| `t` | Snake tail, tùy chọn; parser có thể tự suy ra |
| `a` | Apple |
| `A` | VIP apple |
| `E` | Exit |
| `R` | Rock |
| `^ > v <` | Directional spike |
| `~` | Toxic |

Mechanic có ID/link phức tạp như teleport, door hoặc laser nên nằm trong
`entities`, không cố nhồi thành một ký tự khó mở rộng.

## 4. Entity definition

```json
{
  "id": "door-main",
  "type": "door",
  "position": { "x": 7, "y": 4 },
  "orientation": "vertical",
  "initialState": "closed",
  "properties": {
    "openPolicy": "latch"
  }
}
```

```json
{
  "id": "button-a",
  "type": "pressure-button",
  "position": { "x": 3, "y": 5 },
  "properties": {
    "mode": "latch-open",
    "acceptedPressers": ["snake", "rock", "bomb"]
  }
}
```

## 5. Link definition

```json
{
  "type": "controls",
  "sources": ["button-a", "button-b"],
  "target": "door-main",
  "logic": "paired"
}
```

Teleport dùng `pair` link hoặc cùng `pairId`. Canonical V2 dùng stable ID cho
door/button; importer legacy có thể nhận quan hệ theo thứ tự author nhưng phải
normalize thành link ổn định trước khi ghi canonical JSON.

## 6. Validation level

### Structural

- JSON hợp schema và schemaVersion được hỗ trợ.
- ID level/entity duy nhất.
- Kích thước dương và số dòng/cột đúng.
- Symbol hợp lệ.
- Position nằm trong board.

### Gameplay

- Chính xác một head và exit.
- Snake liên thông, không tự chồng.
- Apple count hợp lệ.
- Solid entities không chồng nhau.
- Link chỉ tham chiếu ID tồn tại và type tương thích.
- Teleporter đủ cặp.
- Không spawn trên hazard nếu không chủ ý.

### Solvability

- Solution command sequence chạy thành `Won` với rules version tương ứng.
- Không command nào trong solution bị reject.
- Hash state cuối đúng kỳ vọng nếu dùng golden snapshot.
- Optional: solver xác nhận có ít nhất một lời giải.

## 7. Import legacy AssetRipper

Dữ liệu cũ dùng `MapData` với `char[]` 40×30 và ký hiệu như `0,1,2,3,9,X`.
Importer thực hiện:

1. Đọc prefab cũ hoặc file trung gian đã extract.
2. Decode `char[]` và xác định bounding box nội dung.
3. Map ký hiệu cũ sang semantic type.
4. Chuyển list metadata teleport/door/slide bar thành stable entity ID.
5. Chuyển `HintData` thành solution candidate.
6. Chạy simulation validator.
7. Xuất JSON V2 và migration report.

Không chỉnh tay file import mà không lưu source provenance.

## 8. Provenance

Mỗi level import nên có metadata:

```json
"source": {
  "kind": "assetripper-prefab",
  "originalPath": "Resources/snakeapple/easy/Level 1.prefab",
  "sourceHash": "...",
  "importerVersion": 1
}
```

Thông tin này giúp biết level nào cần import lại khi sửa importer.

## 9. Content hash

Hash được tính từ canonical JSON đã normalize, bỏ metadata không ảnh hưởng
gameplay như notes/author. Hash dùng cho replay compatibility và save migration.

## 10. Level catalog

Catalog riêng chứa thứ tự phát hành:

```json
{
  "catalogVersion": 1,
  "normal": ["normal-001", "normal-002"],
  "hard": ["hard-001"],
  "challenge": ["challenge-001"]
}
```

Không suy ra thứ tự bằng tên file hoặc `Resources.LoadAll`.
