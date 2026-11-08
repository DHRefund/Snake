# Yêu cầu và tiêu chí nghiệm thu

**Trạng thái:** Accepted

ID được dùng trong task, test plan và release checklist. Một yêu cầu thay đổi phải
cập nhật cả tài liệu nguồn và test liên quan.

## 1. Functional requirements — Core gameplay

| ID | Yêu cầu | Tiêu chí nghiệm thu |
|---|---|---|
| FR-GAME-001 | Tải level từ canonical LevelData | Level hợp lệ tạo đúng initial BoardState; level lỗi không vào Playing |
| FR-GAME-002 | Nhận di chuyển bốn hướng | Mỗi command tạo accepted/rejected result deterministic |
| FR-GAME-003 | Duy trì hình dạng snake | Sau mỗi turn committed, các segment liên thông và không chồng solid entity |
| FR-GAME-004 | Ăn apple và tăng chiều dài | Apple thường tăng 1 segment (giảm counter), VIP tăng 2 segment (ghi collect meta) |
| FR-GAME-005 | Trạng thái exit theo mode | Normal/Hard mở khi hết apple thường; Challenge mở từ đầu |
| FR-GAME-006 | Hoàn thành level qua exit | Session chuyển Won đúng rule và khóa move input |
| FR-GAME-007 | Giải quyết gravity | Board đạt stable state với ordering không phụ thuộc collection iteration |
| FR-GAME-008 | Xử lý hazard | Session Lost chứa death reason và source cell/entity |
| FR-GAME-009 | Push movable | Move chỉ accepted khi push destination hợp lệ |
| FR-GAME-010 | Trigger/door | State cửa phản ánh expression của trigger sau movement/gravity |
| FR-GAME-011 | Teleport | Pocket, pair, hướng ra và push movable đúng GDD; command invalid bị reject nguyên tử |
| FR-GAME-012 | Laser/bomb nâng cao | Interaction đúng catalog và có test cho chain reaction |

## 2. Functional requirements — Session và progression

| ID | Yêu cầu | Tiêu chí nghiệm thu |
|---|---|---|
| FR-SES-001 | Undo | Phục hồi chính xác snapshot trước accepted turn |
| FR-SES-002 | Restart | Khôi phục initial state và xóa undo history |
| FR-SES-003 | Replay | Cùng level hash/rules/commands tạo cùng final state hash |
| FR-SES-004 | Hint | Không đưa command sai khi current state lệch solution path |
| FR-SES-005 | Unlock | Hoàn thành level mở level kế tiếp, thao tác idempotent |
| FR-SES-006 | Save | Progress/settings tồn tại sau restart app |
| FR-SES-007 | Mode | Catalog Normal/Hard/Challenge độc lập về thứ tự và progress |
| FR-SES-008 | Mode unlock | Normal mở từ đầu; hoàn tất Normal mở Hard; hoàn tất Hard mở Challenge |
| FR-SES-009 | Catalog completeness | Tải đúng 180 Normal, 50 Hard, 54 Challenge với ID/hash ổn định |

## 3. Functional requirements — UI/input

| ID | Yêu cầu | Tiêu chí nghiệm thu |
|---|---|---|
| FR-UI-001 | Home/Level Select/Game/Result flow | Đi hết core loop không dùng debug/editor |
| FR-UI-002 | Keyboard và touch | Cùng gesture/action sinh cùng semantic command |
| FR-UI-003 | Feedback blocked move | Có visual/audio cue nhưng state không đổi |
| FR-UI-004 | Pause/resume | Không mất hoặc double-commit turn đang animate |
| FR-UI-005 | Safe area | CTA/HUD không nằm trong notch/home indicator |
| FR-UI-006 | Settings | Music/SFX/haptics/reduced motion được lưu độc lập |
| FR-UI-007 | Terminal flow | Win/Lose luôn cho phép Next/Undo/Restart/Home phù hợp mà không chờ network |
| FR-UI-008 | Responsive camera | Toàn board và HUD quan trọng nằm trong safe area ở device matrix mục tiêu |

## 4. Functional requirements — Audio, content và accessibility

| ID | Yêu cầu | Tiêu chí nghiệm thu |
|---|---|---|
| FR-CONT-001 | Catalog V1 nguyên vẹn | Không sửa layout/solution để che lỗi rule; content patch có version và provenance |
| FR-CONT-002 | Verified solution | 284/284 level có replay kết thúc Won trên rules version phát hành |
| FR-CONT-003 | Tutorial | Mechanic mới có onboarding tại lần xuất hiện đầu, có thể xem lại và localization-ready |
| FR-AV-001 | Gameplay feedback | Move/blocked/collect/trigger/hazard/win/lose có visual feedback; audio không là kênh duy nhất |
| FR-AV-002 | Audio settings | Music và SFX tắt/bật riêng, giữ state qua restart app |
| FR-ACC-001 | Không phụ thuộc màu | Portal, door, hazard và state level có shape/icon/text bổ trợ |
| FR-ACC-002 | Control alternatives | Swipe và D-pad tạo cùng command; không yêu cầu multi-touch hoặc timing chính xác |

## 5. Functional requirements — Monetization

| ID | Yêu cầu | Tiêu chí nghiệm thu |
|---|---|---|
| FR-MON-001 | Consent trước ads | UMP được cập nhật mỗi launch; chỉ request ads khi consent state cho phép; gameplay không bị khóa khi denied/error |
| FR-MON-002 | Rewarded tự nguyện | Reward callback hợp lệ cấp đúng 50 coin một lần; close/fail/cancel không cấp thưởng |
| FR-MON-003 | Reward cap | Không cấp quá 5 rewarded grants trong cửa sổ 24 giờ và UI phản ánh số lượt còn lại |
| FR-MON-004 | Interstitial placement | Chỉ sau Win tại natural break, không ở 5 level đầu/tutorial/Lose và không xuất hiện muộn nếu chưa Ready |
| FR-MON-005 | Frequency cap | Interstitial cách nhau tối thiểu 3 completed level và 120 giây |
| FR-MON-006 | SDK failure isolation | Offline, timeout, load/show failure hoặc callback lặp không khóa input, audio, navigation hay progression |
| FR-MON-007 | Build safety | Development dùng test ads; Release validation từ chối sample/rỗng/sai-environment ad unit ID |
| FR-MON-008 | Format scope | Release bật Banner adaptive ở đáy Gameplay, Rewarded và Interstitial; App Open/Native/Rewarded Interstitial bị tắt |

## 6. Non-functional requirements

| ID | Yêu cầu | Mục tiêu |
|---|---|---|
| NFR-001 | Determinism | Golden replay cho catalog có final hash ổn định |
| NFR-002 | Testability | Domain test chạy không cần Unity scene/GameObject |
| NFR-003 | Performance | Turn core thông thường dưới 1 ms trên desktop baseline |
| NFR-004 | Loading | Boot vào Home không quá 3 giây ngoài thời gian form hệ thống; đổi level không quá 1 giây trên thiết bị baseline |
| NFR-005 | Reliability | Không mất cả primary và backup khi save bị gián đoạn |
| NFR-006 | Maintainability | Assembly dependency tuân theo Architecture doc |
| NFR-007 | Accessibility | Không mechanic nào chỉ phân biệt bằng màu hoặc âm thanh |
| NFR-008 | Offline | Core campaign chạy không cần network/SDK thành công |
| NFR-009 | Compatibility | Save/replay được migrate hoặc báo incompatible rõ |
| NFR-010 | Privacy | Không gửi tracking trước consent tại nơi yêu cầu |
| NFR-011 | Frame rate | Mục tiêu 60 FPS, không thấp hơn 30 FPS bền vững trên thiết bị Android baseline |
| NFR-012 | Input latency | UI phản hồi trong frame hiện tại; move bắt đầu biểu diễn không quá 100 ms sau input hợp lệ |

## 7. Traceability tối thiểu

Mỗi task implementation cần ghi:

```text
Requirement IDs: FR-GAME-004, FR-SES-001
Design docs: 01-Game-Design/02-game-rules.md §4
Tests: AppleConsumptionTests, UndoAfterAppleTests
```

## 8. Release gates

- Không requirement bắt buộc nào thiếu acceptance test.
- Mọi NFR có phép đo hoặc checklist cụ thể.
- Feature ngoài scope phải tắt sạch, không để UI chết.
- Known deviation từ APK phải ghi trong release notes hoặc ADR.
- Catalog phải đạt 284/284 golden replay; không chấp nhận giảm scope bằng cách bỏ 8 level backlog.
