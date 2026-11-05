# Snake & Apple: Puzzle Adventure (V2) — Documentation Hub

Tài liệu này là nguồn sự thật chính thức (single source of truth) cho phiên bản
V2 của trò chơi **Snake & Apple: Puzzle Adventure**. Mọi quyết định về luật chơi, dữ liệu, kiến trúc, UI, kiểm thử và kế hoạch
phát triển phải được cập nhật tại đây trước hoặc cùng lúc với code.

## Mục tiêu của bộ tài liệu

- Cho phép một lập trình viên mới hiểu sản phẩm mà không cần đọc bản decompile.
- Tách rõ yêu cầu game design khỏi chi tiết triển khai Unity.
- Đảm bảo gameplay deterministic, hỗ trợ undo, replay, hint và test tự động.
- Giữ phạm vi V2 thực tế: hoàn thiện game puzzle trước, dịch vụ thương mại sau.
- Chuẩn hóa bằng chứng từ V1 thành quyết định GDD và test, không để câu hỏi mở vô thời hạn.

## Thứ tự đọc khuyến nghị

1. [Product specification](00-Product/README.md)
2. [Tầm nhìn và phạm vi](00-Product/01-product-vision.md)
3. [Phạm vi release và thước đo](00-Product/04-release-scope-and-success.md)
4. [GDD](01-Game-Design/README.md)
5. [Luật mô phỏng](01-Game-Design/02-game-rules.md)
6. [Level design và validation](01-Game-Design/05-level-design-and-content.md)
7. [Kiến trúc tổng thể](02-Architecture/01-architecture-overview.md)
8. [Mô phỏng và turn pipeline](02-Architecture/02-simulation-and-turn-pipeline.md)
9. [Định dạng level](03-Data/01-level-format.md)
10. [UX/UI specification](05-UX/README.md)
11. [Ads, IAP và Analytics](04-Systems/03-ads-iap-analytics.md)
12. [Chiến lược kiểm thử](06-Production/02-testing-strategy.md)
13. [Roadmap](06-Production/03-roadmap.md)
14. [ADR-005: Tương thích V1](07-Decisions/ADR-005-v1-compatible-rules-and-content.md)
15. [Hồ sơ bản AssetRipper](08-Appendix/01-assetripper-findings.md)
16. [Block autotile rendering](02-Architecture/05-block-autotile-rendering.md)
17. [ADR-007: Kiến trúc Hybrid Spine Head + Sprite Body](07-Decisions/ADR-007-snake-spine-head-sprite-body-hybrid.md)

## Cấu trúc tài liệu

```text
Doc/
├── 00-Product/       Tầm nhìn, phạm vi và thuật ngữ
├── 01-Game-Design/   Luật chơi, cơ chế, progression và economy
├── 02-Architecture/  Kiến trúc code và ranh giới phụ thuộc
├── 03-Data/          Level, runtime state và save data
├── 04-Systems/       Input, undo, hint, audio và dịch vụ ngoài gameplay
├── 05-UX/            Luồng màn hình, HUD và accessibility
├── 06-Production/    Workflow, test, roadmap và Definition of Done
├── 07-Decisions/     Architecture Decision Records (ADR)
└── 08-Appendix/      Hồ sơ reverse engineering và dữ liệu tham khảo
```

## Quy ước trạng thái

Mỗi tài liệu có thể dùng một trong các trạng thái:

- `Draft`: đang hình thành, chưa dùng để khóa implementation.
- `Review`: đã đủ nội dung, cần xác nhận.
- `Accepted`: là quyết định chính thức của V2.
- `Superseded`: đã được tài liệu hoặc ADR khác thay thế.

Toàn bộ specification hiện hành của V2 ở trạng thái `Accepted`; tài liệu lịch sử
có thể là `Superseded`. Mọi thay đổi làm khác gameplay, dữ liệu, replay hoặc save
phải cập nhật tài liệu và ADR trước khi merge.

## Nguyên tắc cập nhật

1. Thay đổi luật chơi phải cập nhật Game Design trước.
2. Thay đổi cấu trúc code phải có lý do và cập nhật Architecture hoặc ADR.
3. Không để hành vi quan trọng chỉ tồn tại trong code mà không có tài liệu.
4. Ví dụ trong tài liệu là minh họa; test tự động mới là bằng chứng implementation.
5. Catalog V1 đã phát hành là content chuẩn; không sửa level để che lỗi simulation V2.
6. Bằng chứng V1 phải được chuẩn hóa vào GDD/ADR/test, không để logic phụ thuộc trực tiếp bản decompile.
7. Hồ sơ AssetRipper ghi provenance; điểm không quan sát được phải được khóa thành product decision.

## Phạm vi nguồn tham khảo

Bản AssetRipper được dùng để tham khảo asset, tên cơ chế và dữ liệu level. Nó
không được xem là kiến trúc chuẩn của V2 vì phần lớn thân hàm C# đã mất hoặc bị
biến đổi trong quá trình IL2CPP/decompile.
