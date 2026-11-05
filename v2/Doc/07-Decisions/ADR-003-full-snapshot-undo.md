# ADR-003: Full snapshot cho Undo

**Status:** Accepted  
**Date:** 2026-10-18

## Context

Undo có thể được hiện thực bằng reverse command/delta hoặc snapshot. Game có
nhiều phản ứng dây chuyền như gravity, trigger, laser, bomb và slide bar nên
reverse chính xác từng hiệu ứng dễ sai.

## Decision

Milestone đầu lưu full logical snapshot trước mỗi accepted turn. Snapshot không
chứa view/animation. History có giới hạn memory nhưng tối thiểu 100 turn.

## Alternatives

### Reverse command/delta

Tiết kiệm memory nhưng mỗi mechanic phải viết và test inverse operation.

### Replay từ đầu khi Undo

State tối giản nhưng độ trễ tăng theo số turn và phức tạp khi animation/UI.

## Consequences

- Implementation đơn giản và đáng tin cậy.
- Memory cao hơn nhưng board nhỏ nên chấp nhận được.
- Cần clone/serialization hiệu quả và benchmark.
- Có thể chuyển sang structural sharing/delta sau khi đo đạc, không tối ưu trước.
