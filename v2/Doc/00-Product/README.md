# Product specification

**Trạng thái:** Accepted  
**Chủ sở hữu:** Product owner  
**Cập nhật:** 2026-10-19

Thư mục này trả lời câu hỏi **V2 là sản phẩm gì, dành cho ai, phát hành cái gì và
được xem là thành công khi nào**. Luật chi tiết của puzzle nằm trong
`../01-Game-Design/` và cách triển khai nằm trong `../02-Architecture/`.

## Bộ tài liệu chính thức

1. [Tầm nhìn và phạm vi](01-product-vision.md)
2. [Thuật ngữ](02-glossary.md)
3. [Yêu cầu và tiêu chí nghiệm thu](03-requirements.md)
4. [Phạm vi release và thước đo thành công](04-release-scope-and-success.md)
5. [Người chơi và trải nghiệm mục tiêu](05-player-experience.md)

## Quyết định sản phẩm đã khóa

| Chủ đề | Quyết định |
|---|---|
| Nền tảng đầu tiên | Android, Unity 6, portrait |
| Thể loại | Puzzle lưới theo lượt, không có yếu tố phản xạ thời gian thực |
| Nội dung release | 284 level: 180 Normal, 50 Hard, 54 Challenge |
| Nguồn level | Catalog đã trích từ bản phát hành V1; không tự sửa level để che lỗi engine |
| Offline | Toàn bộ gameplay và progression chơi được khi không có mạng |
| Progression | Tuyến tính trong từng mode; không dùng tiền hoặc quảng cáo để khóa tiến trình |
| Undo/Restart | Miễn phí, không giới hạn |
| Hint | Miễn phí, từng bước; không ép xem quảng cáo |
| Quảng cáo | Rewarded tự nguyện và Interstitial tại điểm nghỉ tự nhiên, có frequency cap |
| Economy | Một loại coin chỉ dành cho cosmetic |
| Art | UI/gameplay dùng placeholder trước; asset cuối không được thay đổi hitbox/rule |
| Ngôn ngữ | Hạ tầng localization-ready; nội dung gameplay ưu tiên biểu tượng trực quan |
| Multiplayer/server | Ngoài phạm vi release V2 |

## Ranh giới giữa quyết định và trạng thái triển khai

- Tài liệu `Accepted` là hành vi sản phẩm phải đạt, không có nghĩa code hiện tại đã hoàn tất.
- Sai khác giữa validator và catalog được ghi là backlog implementation, không tự động đổi GDD.
- Nếu bằng chứng từ V1 cho thấy rule cần sửa để level chính thức chạy đúng, cập nhật GDD,
  ADR, test và `RulesVersion` cùng một thay đổi.
- Asset placeholder và polish có thể chưa hoàn thiện, nhưng luồng và chức năng không được để
  nút chết trong build nghiệm thu.

## Definition of product complete

Product/GDD được xem là hoàn chỉnh khi phạm vi, luật, content, UX, economy, ads,
accessibility và release gate đều có quyết định rõ; không còn mục “chờ xác minh APK”.
Game chỉ được xem là release-ready khi các tiêu chí trong
[phạm vi release](04-release-scope-and-success.md) đều đạt.
