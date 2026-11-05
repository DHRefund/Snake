# Game Design Document — Snake & Apple: Puzzle Adventure (V2)

**Trạng thái:** Accepted  
**RulesVersion:** 1  
**Cập nhật:** 2026-10-19

Đây là GDD chính thức của V2. Nó mô tả trải nghiệm và hành vi quan sát được;
không quy định class, MonoBehaviour hay thuật toán Unity cụ thể.

## Cách đọc

1. [Trụ cột và core loop](01-design-pillars-and-core-loop.md)
2. [Luật mô phỏng](02-game-rules.md)
3. [Danh mục mechanic](03-mechanics-catalog.md)
4. [Progression và economy](04-progression-economy.md)
5. [Level design, content và validation](05-level-design-and-content.md)
6. [Onboarding và đường cong độ khó](06-onboarding-and-difficulty.md)
7. [Game state, camera và feedback](07-game-state-camera-feedback.md)

Tài liệu UX màn hình nằm tại `../05-UX/`; save, ads, audio và input chi tiết nằm
tại `../03-Data/` và `../04-Systems/`.

## Design contract

- Puzzle theo lượt; không có random trong simulation và không dùng Unity physics làm nguồn sự thật.
- Một command chỉ kết thúc khi toàn bộ phản ứng bắt buộc đạt stable state hoặc terminal state.
- Cùng level, rules version và command sequence luôn cho cùng kết quả.
- 284 level V1 là content chuẩn. Mọi level phát hành phải có lời giải xác minh được.
- Presentation chỉ biểu diễn state; animation, camera, audio và ads không được đổi kết quả.
- Undo/Restart là công cụ suy luận miễn phí, không phải tài nguyên thương mại.
- Rule chưa khớp replay là implementation backlog, không phải giấy phép sửa content.

## Definition of GDD complete

GDD đã khóa fantasy, loop, input, win/lose, turn order, mechanic, mode,
progression, economy, onboarding, content validation, camera và feedback. Thay đổi
một mục ảnh hưởng lời giải cần cập nhật `RulesVersion`, ADR và golden replay.
