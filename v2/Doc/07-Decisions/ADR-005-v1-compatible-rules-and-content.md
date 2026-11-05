# ADR-005: Tương thích rules và content V1

**Status:** Accepted  
**Date:** 2026-10-20  
**Supersedes:** ADR-004

## Context

ADR-004 từng cho phép V2 định nghĩa ruleset độc lập và sửa/migrate level legacy
khi khác biệt. Sau khi import đủ 284 level và replay solution từ bản game đã phát
hành, cách đó tạo rủi ro sửa content đúng để phù hợp engine V2 sai. Product owner
đã quyết định level V1 có độ tin cậy cao và phải điều tra rule V2 trước.

Validator hiện đạt 276/284, trong đó toàn bộ Hard và Challenge đã pass. Tám level
Normal còn lại tập trung ở slide bar, portal, gravity và một level không có solution
trong dữ liệu APK. Đây là bằng chứng ruleset cần hoàn thiện, không phải bằng chứng
catalog cần thay đổi.

## Decision

- Catalog V1 gồm 180 Normal, 50 Hard và 54 Challenge là content chuẩn của V2 1.0.
- Không sửa layout hoặc solution legacy chỉ để golden replay pass.
- Hành vi có thể tái hiện từ bản phát hành và solution chính thức có ưu tiên cao
  hơn implementation V2 hiện tại.
- GDD là đặc tả đã chuẩn hóa của hành vi đó. Khi phát hiện bằng chứng mới, cập
  nhật GDD, test và code trong cùng thay đổi; không để quyết định chỉ nằm trong code.
- Deviation cố ý khỏi V1 cần Product owner chấp thuận, ADR riêng, tăng
  `RulesVersion` nếu ảnh hưởng replay và kế hoạch migration content/save.
- Level không có solution chính thức phải được solver hoặc playtest tạo verified
  replay; việc thiếu hint không cho phép loại level khỏi release.

Các rule tương thích đã khóa trong GDD hiện hành gồm:

- Apple thường tăng một segment; VIP Apple tăng hai segment (hỗ trợ các level puzzle cần chiều dài đặc biệt để bắc cầu/leo vách).
- Normal/Hard cần hết apple thường để mở exit; Challenge không có gate này.
- Apple, VIP Apple và remote là support trong gravity.
- Rắn resolve gravity trước, movable resolve sau; movable có thể đứng trên rắn.
- Portal dùng pocket và cho phép thân tạm thời bắc qua hai đầu.
- Door mở theo latch; frame cửa vẫn chặn laser.
- Movable trên emitter vô hiệu hóa laser.
- Bomb nổ vùng 3 × 3 và có thể phá wall/door/web theo rule.
- Tại cùng contact checkpoint, Win hợp lệ được khóa trước static hazard.
- Slide bar phải mang/đẩy occupant nguyên tử; phần implementation còn thiếu là backlog.

## Nguồn sự thật

Khi có xung đột, dùng thứ tự:

1. Hành vi tái hiện được từ game V1 đã phát hành và verified solution.
2. Dữ liệu level/import có provenance và source hash.
3. GDD/ADR hiện hành sau khi đã chuẩn hóa bằng chứng.
4. Test hiện hành.
5. Implementation V2 hiện tại.
6. Tên class/field hoặc method body decompile không đầy đủ.

Thứ tự này không cho phép reverse engineering trì hoãn vô hạn: với trường hợp
không thể quan sát, nhóm khóa quyết định trong GDD và viết test, nhưng phải ghi rõ
đó là product decision thay vì giả vờ là hành vi V1.

## Consequences

- Tám level backlog là release blocker của simulation, không phải content task.
- ADR-004 không còn là nguồn rule hiện hành.
- Golden replay 284/284 trở thành release gate bắt buộc.
- Mọi sửa rule phải kiểm tra regression toàn catalog.
- UI, asset và service có thể tiếp tục song song vì không thay đổi canonical content.
