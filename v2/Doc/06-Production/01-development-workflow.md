# Quy trình phát triển

**Trạng thái:** Accepted

## 1. Nguyên tắc triển khai

Mỗi feature đi theo vertical slice nhỏ:

```text
Rule/document
→ Domain model
→ EditMode tests
→ Application use case
→ Minimal presentation
→ PlayMode tests
→ Content validation
→ Polish
```

Không bắt đầu từ animation/prefab trước khi semantic rule và test được chốt.

## 2. Branch và commit

Dự án dùng short-lived feature branch trên `main`:

- `main` luôn build được.
- Branch nhỏ theo feature, không giữ nhiều tuần.
- Commit mô tả intent, không chỉ “update files”.
- Không commit `Library`, `Temp`, `Logs`, build output hoặc secret.

## 3. Pull request checklist

- Doc/rule liên quan đã cập nhật.
- Có test mới cho hành vi mới và edge case.
- Không tạo dependency ngược module.
- Không thêm singleton/static mutable state.
- Không có warning/error mới.
- Level/content thay đổi đã chạy validator.
- Save schema thay đổi có migration.
- UI đã kiểm tra safe area và localization cơ bản.

## 4. Workflow thêm mechanic

1. Viết mục mechanic trong GDD.
2. Xác định interaction matrix với mọi entity hiện có.
3. Chốt vị trí trong turn pipeline.
4. Tạo ADR nếu thay đổi ordering hoặc data model.
5. Thêm definition/schema.
6. Viết unit test riêng và test kết hợp.
7. Thêm view/pool/animation.
8. Tạo tutorial level.
9. Chạy full level replay validation.
10. Playtest và cập nhật doc.

## 5. Workflow level design

1. Chọn mục tiêu học/kiểm tra của level.
2. Author level canonical format/tool.
3. Chạy structural validator.
4. Chơi và record solution.
5. Chạy headless replay xác minh.
6. Kiểm tra alternate solution nếu quan trọng.
7. Gắn difficulty/tags.
8. Review bởi người không thiết kế level.
9. Thêm vào catalog.

## 6. Import từ bản AssetRipper

- Asset hình ảnh/audio được xem xét license/quyền sử dụng trước khi đưa V2.
- Level đi qua importer và validator, không copy prefab trực tiếp vào runtime.
- Mỗi batch import có report ký hiệu chưa hiểu và level invalid.
- Không sửa file decompile để biến nó thành kiến trúc V2.

## 7. Definition của một task kỹ thuật

Task phải nêu:

- User/gameplay outcome.
- Rule/doc nguồn.
- Acceptance criteria quan sát được.
- Test cases bắt buộc.
- Data migration/content impact.
- Out-of-scope.

## 8. Build configuration

- Development: debug UI, level picker, state inspector, deterministic replay log.
- QA: production-like nhưng có diagnostic export.
- Release: tắt cheat/debug, giữ crash logging phù hợp.
- SDK ads/IAP dùng environment/test ID theo build config, không sửa tay prefab.
