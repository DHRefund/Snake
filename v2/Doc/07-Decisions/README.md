# Architecture Decision Records

ADR ghi lại quyết định khó đảo ngược hoặc có ảnh hưởng rộng. ADR không thay thế
GDD/TDD; nó giải thích vì sao một lựa chọn được chấp nhận so với lựa chọn khác.

## Trạng thái

- Proposed
- Accepted
- Deprecated
- Superseded

## Danh sách

- [ADR-001: Headless deterministic core](ADR-001-headless-deterministic-core.md)
- [ADR-002: Canonical level data không dùng prefab](ADR-002-canonical-level-data.md)
- [ADR-003: Full snapshot cho Undo](ADR-003-full-snapshot-undo.md)
- [ADR-004: Ruleset V2 độc lập](ADR-004-locked-v2-ruleset.md) — Superseded
- [ADR-005: Tương thích rules/content V1](ADR-005-v1-compatible-rules-and-content.md)
- [ADR-006: Single-sprite autotile cho block địa hình](ADR-006-single-sprite-autotile-blocks.md)
- [ADR-007: Kiến trúc Kết hợp Rắn (Spine Head + 2D Sprite Body & Tail + Juicy Physics Engine)](ADR-007-snake-spine-head-sprite-body-hybrid.md)

## Template

```markdown
# ADR-NNN: Tên quyết định

Status: Proposed
Date: YYYY-MM-DD

## Context
Vấn đề và constraint.

## Decision
Quyết định được chọn.

## Alternatives
Các lựa chọn đã cân nhắc.

## Consequences
Hệ quả tích cực, tiêu cực và việc cần làm tiếp.
```
