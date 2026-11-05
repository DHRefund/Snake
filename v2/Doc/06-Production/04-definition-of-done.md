# Definition of Done

**Trạng thái:** Accepted

## 1. Gameplay mechanic Done

- Rule và interaction matrix đã được duyệt.
- Vị trí trong turn pipeline được xác định.
- Data schema/config có default rõ.
- Domain implementation không phụ thuộc Unity presentation.
- Unit test happy path, rejection và edge case.
- Undo/replay/state hash hoạt động.
- View có visual cho mọi state.
- Audio/VFX không phải nguồn sự thật.
- Có ít nhất một tutorial/content test level.
- Documentation đã cập nhật.

## 2. Level Done

- Qua structural/gameplay validator.
- Có solution replay cùng content hash/rules version.
- Golden replay kết thúc Won.
- Difficulty/tags/catalog position đã gán.
- Không có unknown legacy symbol.
- Đã playtest bởi người khác designer.
- Tutorial text/hint nếu cần đã localization-ready.
- Không dựa vào bug hoặc timing animation.

## 3. UI screen Done

- State empty/loading/error/content đầy đủ.
- Back navigation đúng.
- Không gọi Domain mutation trực tiếp ngoài command/use case.
- Hỗ trợ safe area và aspect ratio mục tiêu.
- Keyboard/touch focus hợp lý.
- Text localization-ready.
- Reduced motion/setting liên quan hoạt động.
- PlayMode smoke test.

## 4. Service integration Done

- Có interface và null/fake implementation.
- Timeout/fail/cancel/duplicate callback được xử lý.
- Không block core gameplay khi unavailable.
- Log không chứa secret/PII.
- Consent/privacy requirement đáp ứng.
- Transaction idempotent nếu liên quan reward/purchase.
- Ads placement/frequency cap và restore input/audio/focus có automated hoặc device test.
- Development/QA chỉ dùng test ads; release validator xác nhận production ID đúng environment.
- Privacy options, consent denied, offline và SDK timeout đã được kiểm thử.

## 5. Milestone build Done

- Không compile error hoặc warning mới chưa được chấp nhận.
- Tất cả EditMode/PlayMode test xanh.
- Toàn catalog mục tiêu qua golden replay.
- Save migration/recovery xanh.
- Không có blocker/critical bug mở.
- Performance và memory trong budget.
- Build cài/chạy được trên thiết bị mục tiêu.
- Release notes và known issues được cập nhật.
