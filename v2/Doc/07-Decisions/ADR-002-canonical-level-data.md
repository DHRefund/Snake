# ADR-002: Canonical level data không dùng prefab

**Status:** Accepted  
**Date:** 2026-10-18

## Context

Bản AssetRipper lưu level bằng prefab chứa MapData và fileID/GUID. Prefab khó diff,
khó validate ngoài Unity và liên kết mechanic bằng list position dễ hỏng.

## Decision

Dùng JSON có schema version làm canonical level source. Unity có thể tạo cache
ScriptableObject/binary khi import/build, nhưng không chỉnh cache thay cho JSON.

## Alternatives

### Tiếp tục dùng prefab

Tận dụng dữ liệu cũ nhanh nhưng giữ coupling với Unity serialization.

### Chỉ dùng ScriptableObject

Inspector tốt nhưng diff/merge và tool ngoài Unity kém hơn JSON.

### Custom binary

Nhẹ khi chạy nhưng không phù hợp authoring và migration giai đoạn đầu.

## Consequences

- Cần importer từ legacy prefab/char map.
- Có schema, validator, provenance và content hash rõ ràng.
- Designer cần editor tool để không phải sửa JSON bằng tay lâu dài.
- Build pipeline phải đảm bảo mọi JSON đã validate trước packaging.
