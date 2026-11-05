# ADR-007: Kiến trúc Kết hợp Rắn (Spine Head + 2D Sprite Body & Tail + Juicy Physics Engine)

**Status:** Accepted  
**Date:** 2026-11-03  
**Deciders:** Core Team, Lead Game Designer  

---

## 1. Context (Bối cảnh & Vấn đề)

1. **Yêu cầu chất lượng thẩm mỹ (Visual Excellence / VIP Standard)**:
   - Trò chơi gốc V1 (`SnakeApple_ExportedProject`) sở hữu biểu cảm đầu rắn vô cùng sống động: há to mồm khi đứng cạnh quả táo (`open_mouth`), phồng má nhai nhóp nhép khi nuốt táo (`eating`), choáng váng mắt nheo khi đâm vào tường (`hit_wall`), chớp mắt đảo đồng tử tự nhiên (`idle_1`, `idle_2`), và cười híp mắt khi chiến thắng (`win`).
   - Khảo sát mã nguồn và tài nguyên V1 cho thấy toàn bộ các biểu cảm co giãn này được thực hiện bằng **Spine 2D Mesh Deformation** với bộ khung xương 132 bones, 113 slots và 12 animations trong file `skeleton_1.json`.
   
2. **Hạn chế của các phương án cực đoan**:
   - **Phương án Pure Sprite (Chỉ đổi ảnh tĩnh)**: Cắt các frame phẳng thay thế tuần tự. Hạn chế: Chuyển động giật cục như múa rối giấy (paper doll), mất hoàn toàn độ đàn hồi mềm mại của hàm và má, chất lượng thị giác thua kém V1 rõ rệt.
   - **Phương án Pure Spine Toàn Thân (Rig cả thân rắn vào Spine)**: Đưa toàn bộ các đốt thân và đuôi vào khung xương Spine. Hạn chế: Cơ chế cốt lõi của game là puzzle trên ô lưới $90^\circ$; việc uốn lượn lưới mesh cho thân rắn rất phức tạp, dễ gây biến dạng rách góc texture khi quẹo cua gắt, và tốn tài nguyên GPU/CPU không cần thiết.

---

## 2. Decision (Quyết định được chọn)

Chính thức áp dụng mô hình **Hybrid Architecture (Kiến trúc Kết hợp)** cho toàn bộ hệ thống rắn trong V2:

```text
Con rắn trong Gameplay V2
│
├── 1. ĐẦU RẮN (HEAD) — Sử dụng Spine 2D Runtime
│   ├── Khung xương chung (Common Rig): 132 bones điều khiển cằm, hàm, mắt, mí mắt, lông mày, má, tai, sừng, mang cá.
│   ├── Hệ thống Slot & Skin: Ánh xạ attachment tương ứng theo skin đang chọn (skin_cat, skin_dog, skin_tiger...).
│   └── Bộ 12 Animation VIP: idle_1, idle_2, open_mouth, eating, hit_wall, die, win, start, end_lose, end_win.
│
├── 2. THÂN & ĐUÔI (BODY & TAIL) — Sử dụng 2D Grid Sprites
│   ├── Đốt thân thẳng (Body Straight): Nối liền 2 cạnh đối diện.
│   ├── Đốt góc cua (Corner 90°): Bo góc mượt mà, xoay 4 hướng (0°, 90°, 180°, 270°) theo vector đường đi.
│   └── Đốt chóp đuôi (Tail): Bo tròn nhọn về phía sau, tự xoay theo hướng đốt áp chót.
│
└── 3. JUICE & GAME FEEL ENGINE — Engine Chuyển động Vượt trội V2
    ├── Phase 1 — Trượt mượt (Smooth Slide): 0.12s theo đường cong SmoothStep nội suy mượt mà.
    ├── Chomp Punch Scale: Đớp táo giật nảy 1.25x -> 1.0x trong 0.08s tạo độ nảy đã tay.
    ├── Phase 2 — Rơi trọng lực gia tốc (Gravity Fall): 0.065s/cell theo hàm bậc hai (t²).
    ├── Landing Squash & Stretch: Khi tiếp đất, thân rắn nhún xẹp (Y = 0.86, X = 1.14) rồi nảy bung lại trong 0.09s.
    ├── Bump Head Pulse: Đâm vào tường dội nhẹ đầu 0.10s phản hồi cản bước.
    └── Input Buffering: Hàng đợi phím bấm mượt mà, không bị nuốt lệnh khi thao tác nhanh.
```

---

## 3. Chi tiết Triển khai Kỹ thuật

1. **Tài nguyên Spine Đầu**:
   - Sử dụng bộ runtime `Spine-Unity` (hoặc `spine-csharp`).
   - Tái sử dụng file dữ liệu gốc: `skeleton_1.json` (hoặc binary `.skel.bytes`), `skeleton.atlas.txt`, và texture atlas `skeleton.png`.
   - Component `SnakeSpineHead` quản lý state máy trạng thái animation (FSM):
     - Kiểm tra ô lân cận có táo $\rightarrow$ kích hoạt `open_mouth`.
     - Vừa nuốt táo $\rightarrow$ kích hoạt `eating`.
     - Di chuyển bị từ chối do đâm tường $\rightarrow$ kích hoạt `hit_wall`.
     - Thắng màn $\rightarrow$ `win` / `end_win`.
     - Rơi vào bẫy/chết $\rightarrow$ `die` / `end_lose`.
     - Trạng thái nghỉ $\rightarrow$ `idle_2`.

2. **Dữ liệu Skin ScriptableObject (`SnakeSpriteSkin`)**:
   - Khai báo trường `spineSkinName` (chuỗi string truyền vào Spine `skeleton.SetSkin(spineSkinName)`).
   - Danh sách sprite thân: `bodySprite`, `cornerSprite`, `tailSprite`.
   - Giúp việc tạo skin mới diễn ra trong vài phút: chỉ cần chọn tên skin Spine và kéo thả 3 sprite thân vào Inspector.

3. **Quy trình Thiết kế Asset (Figma ➔ Unity)**:
   - **Vẽ Đầu mới (ví dụ Husky)**: Họa sĩ chỉ cần bám theo kích thước bounding box của các attachment trong `skin_dog` (đã trích xuất sẵn: đầu, tai trước, tai sau, mõm, mắt, mũi).
   - **Vẽ Thân mới**: Chỉ cần vẽ đúng 3 hình phẳng kích thước chuẩn $256 \times 256\text{ px}$ (thẳng, cua, đuôi).

---

## 4. Alternatives Considered (Các lựa chọn đã cân nhắc)

| Phương án | Ưu điểm | Nhược điểm | Kết luận |
| :--- | :--- | :--- | :--- |
| **A. Pure Flat Sprites** | Rất dễ làm, không cần thư viện Spine | Biểu cảm đơ cứng, không có độ giãn má/hàm, visual thua kém V1 | **Bác bỏ** |
| **B. Pure Full Spine Rig** | Thân rắn uốn lượn liên tục | Cực kỳ phức tạp khi rẽ góc vuông $90^\circ$, dễ rách lưới mesh, hiệu năng nặng | **Bác bỏ** |
| **C. Hybrid (Spine Head + Sprite Body)** | Biểu cảm đỉnh cao ngang V1, thân đuôi chuẩn xác theo grid, tối ưu hiệu năng tuyệt đối | Cần tích hợp Spine runtime cho riêng phần đầu | **CHẤP NHẬN (Accepted)** |

---

## 5. Consequences (Hệ quả)

### Tích cực:
- **Đạt chuẩn VIP**: Đáp ứng trọn vẹn yêu cầu visual đẳng cấp của V1.
- **Game Feel vượt trội V1**: Nhờ kết hợp với engine vật lý đàn hồi (squash & stretch, chomp punch, smooth gravity) độc quyền của V2.
- **Tối ưu tài nguyên**: Mỗi màn chơi chỉ tồn tại đúng 1 Spine GameObject (đầu rắn), toàn bộ phần còn lại là sprite UI/2D siêu nhẹ.
- **Mở rộng dễ dàng**: Nghệ sĩ có thể liên tục bổ sung skin thú mới (Husky, Corgi, Gấu trúc, v.v.) mà không cần can thiệp lại code logic.

### Trạng thái Triển khai (Đã hoàn thành):
1. **Gói Spine Runtime chính thức**: Sử dụng `spine-unity-4.1-2024-06-19` tại [`Assets/Spine/`](file:///g:/hackgame/game%20%20con%20r%E1%BA%AFn/v2/Assets/Spine), loại bỏ DLL cũ, liên kết `spine-unity` và `spine-csharp` vào cả `Game.Presentation.asmdef` và `Game.EditModeTests.asmdef`.
2. **Component `SnakeSpineHead`**: Tự động nạp tài nguyên runtime từ `Resources/Spine/skeleton-data.json`, `skeleton.atlas.txt`, `skeleton.png`.
3. **Multi-Renderer UI**: Bật `skeletonGraphic.allowMultipleCanvasRenderers = true` để render đa texture / submesh mượt mà trên Canvas UI.
4. **Đồng bộ Animation**: Kết nối đầy đủ máy trạng thái hoạt họa (`idle_2`, `open_mouth`, `eating`, `hit_wall`, `die`, `win`) trong `GameplayScreenController`.
5. **Đầu mèo sống động**: Đã test và xác nhận đầu mèo Spine hiển thị sắc nét cùng hoạt họa đầy đủ trên màn chơi.
