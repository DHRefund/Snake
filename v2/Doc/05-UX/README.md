# UX/UI specification — Snake & Apple: Puzzle Adventure (V2)

**Trạng thái:** Accepted  
**Cập nhật:** 2026-10-20

Đây là nguồn thiết kế chính thức cho UI V2 trước khi thay placeholder bằng asset
cuối. UI được thiết kế portrait-first và phải hoạt động hoàn chỉnh bằng shape,
màu cùng text mặc định.

## Thứ tự đọc

1. [Luồng màn hình](01-screen-flow.md)
2. [Ngôn ngữ hình ảnh và accessibility](02-visual-language-accessibility.md)
3. [UI Design System](03-ui-design-system.md)
4. [Specification từng màn hình](04-screen-specifications.md)
5. [Responsive portrait và safe area](05-responsive-portrait.md)

## Quyết định đã khóa

- Orientation phát hành: portrait cố định.
- Reference resolution: 1080 × 1920.
- Không đổi sang landscape vì level rộng; board tự fit vùng gameplay.
- Touch target tối thiểu: 48 × 48 dp tương đương sau scale.
- Gameplay hỗ trợ swipe và D-pad; hai loại input cho cùng semantic command.
- Asset hiện tại là placeholder. Thay sprite/font sau này không thay hierarchy,
  object name, navigation hoặc controller contract.
- Scene và prefab UI phải tồn tại thành file trong project và nằm trong Build Settings.
