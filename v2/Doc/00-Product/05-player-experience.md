# Người chơi và trải nghiệm mục tiêu

**Trạng thái:** Accepted  
**Cập nhật:** 2026-10-19

## 1. Người chơi chính

### Casual puzzle player

- Chơi trên điện thoại trong phiên 2–10 phút.
- Muốn hiểu mục tiêu ngay, thao tác ít và sửa sai nhanh.
- Không muốn đọc hướng dẫn dài hoặc bị buộc xem quảng cáo để tiếp tục.

### Completion player

- Muốn hoàn thành catalog, xem level nào đã qua và mở mode khó hơn.
- Quan tâm tới personal best nhưng không muốn bị khóa content vì số sao.
- Chấp nhận level khó nếu rule nhất quán và luôn có Undo/Restart.

### Expert puzzle player

- Tìm các tương tác nhiều bước giữa thân rắn, trọng lực và vật thể.
- Ưu tiên Hard/Challenge, lời giải gọn và khả năng replay.
- Không cần thao tác nhanh; độ khó đến từ suy luận.

Đây là các nhu cầu sử dụng, không phải phân khúc thu thập dữ liệu cá nhân.

## 2. Lời hứa trải nghiệm

Trong 30 giây đầu, người chơi phải hiểu được ba điều: vuốt để di chuyển, ăn mục
tiêu cần thiết và đi vào cổng. Trong suốt game, họ phải luôn trả lời được:

1. Tôi đang cần làm gì?
2. Vì sao bước vừa rồi thành công hoặc bị chặn?
3. Tôi có thể sửa sai nhanh bằng cách nào?

## 3. Nhịp một phiên chơi

```text
Mở game
→ Continue hoặc chọn level
→ Quan sát và thử lời giải
→ Undo/Restart khi sai
→ Win
→ xem kết quả ngắn
→ Next Level hoặc quay về Level Select
```

Mục tiêu là vào lại gameplay trong tối đa ba thao tác từ Home. Không thêm daily
popup, sale popup hoặc interstitial khi vừa mở ứng dụng.

## 4. Nguyên tắc friction

- Input bị chặn không tiêu lượt, không lưu Undo và phải có phản hồi ngắn.
- Lose không trừ mạng, coin hoặc progression.
- Undo và Restart luôn khả dụng, trừ lúc modal hệ thống đang chặn input.
- Hint không làm người chơi mắc kẹt nếu ad/network không sẵn sàng.
- Animation không kéo dài thời gian chờ suy nghĩ; người chơi có thể giảm motion.
- Back luôn quay về nơi hợp lý và không tự thoát app từ một modal chưa xử lý.

## 5. Tiếp cận và bao hàm

- Không yêu cầu phản xạ, multi-touch hoặc giữ nút chính xác theo thời gian.
- Mọi trạng thái quan trọng có hình dạng/icon ngoài màu và âm thanh.
- Vùng bấm tối thiểu 48 × 48 dp theo layout tham chiếu Android.
- Text hỗ trợ font tiếng Việt, scale hợp lý và chuỗi dài hơn tiếng Anh.
- Swipe và D-pad là hai cách điều khiển tương đương về semantic command.
- Reduced motion không thay đổi kết quả hoặc lợi thế gameplay.

## 6. Nguyên tắc thương mại

- Quảng cáo là lớp tài trợ, không phải một phần của lời giải puzzle.
- Rewarded phải do người chơi chủ động chọn và nói rõ phần thưởng trước khi xem.
- Interstitial chỉ xuất hiện sau Win và theo frequency cap.
- Không dùng fake close, fake urgency, nút đánh lừa hoặc làm CTA quảng cáo nổi bật
  hơn CTA gameplay bắt buộc.
- Trẻ em, consent và quyền riêng tư được xử lý theo cấu hình thị trường phát hành.
