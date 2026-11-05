# Onboarding và đường cong độ khó

**Trạng thái:** Accepted  
**Cập nhật:** 2026-10-19

## 1. Nguyên tắc onboarding

Game dạy bằng level trước, text sau. Tutorial chỉ xuất hiện khi hành động cần thiết
không thể suy ra từ bố cục và biến mất ngay khi người chơi thực hiện đúng.

- Một thông điệp chỉ chứa một động từ/hành động.
- Không khóa input đúng để ép người chơi bấm vào một điểm trang trí.
- Có thể xem lại tutorial từ Pause/Help.
- Tutorial đã xem được lưu, nhưng replay level vẫn có nút xem lại.
- Không hiện interstitial trong tutorial hoặc năm level đầu.

## 2. Thứ tự nhận thức

Thứ tự dưới đây là nguyên tắc curate, không bắt buộc đổi thứ tự catalog V1:

1. Di chuyển bốn hướng, thân đi theo đầu.
2. Apple, growth và exit.
3. Gravity và support của toàn thân.
4. Hazard: spike, toxic và rơi khỏi board.
5. Pushable: rock/bomb.
6. Teleporter.
7. Button và door.
8. Laser.
9. Bomb/remote và chain reaction.
10. Spider web và slide bar.
11. Gravity đảo chiều và tổ hợp nhiều mechanic.

Nếu catalog V1 giới thiệu theo thứ tự khác, UI tutorial bám lần xuất hiện thật đầu
tiên của mechanic thay vì hard-code số level ở danh sách trên.

## 3. Difficulty bands

| Band | Kỳ vọng |
|---|---|
| Intro | 1 ý chính, state nhỏ, gần như không có nhánh sai dài |
| Easy | Kết hợp 1–2 rule đã học, Undo ít |
| Medium | Chuỗi chuẩn bị nhiều bước, có soft-lock logic nhưng sửa được bằng Undo |
| Hard | Nhiều mechanic, cần nhìn trước và quản lý hình dạng thân |
| Expert | Bài toán độc lập dài; không thêm rule ẩn hay yêu cầu phản xạ |

Normal đi từ Intro tới Hard. Hard bắt đầu từ Medium/Hard. Challenge được phép trải
từ Easy tới Expert nhưng phải ghi tag để Level Select và analytics hiểu đúng.

## 4. Hint contract

- Hint đưa đúng **một command kế tiếp** từ state hiện tại nếu state nằm trên một replay đã biết.
- Nếu state lệch replay, hệ thống đề nghị Undo tới checkpoint khớp hoặc Restart; không đoán bước sai.
- Hint không tự commit command và không che board.
- Hint miễn phí, không cooldown và không yêu cầu rewarded ad trong V2 1.0.
- Level chưa có verified replay không được quảng bá là có Hint.

## 5. Đánh giá độ khó

Kết hợp dữ liệu và playtest:

- Completion rate và time-to-complete.
- Số Undo, Restart, Hint và move trước khi Win.
- Điểm bỏ cuộc theo level.
- Người chơi có diễn giải đúng lý do blocked/death hay không.

Không tự động sửa level chỉ dựa trên completion rate. Mọi content change cần
review thiết kế, replay mới và giữ bản migration/provenance.
