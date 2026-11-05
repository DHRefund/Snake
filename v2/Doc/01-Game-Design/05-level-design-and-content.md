# Level design, content và validation

**Trạng thái:** Accepted  
**Cập nhật:** 2026-10-19

## 1. Catalog phát hành

| Mode | Số level | Vai trò |
|---|---:|---|
| Normal | 180 | Campaign chính và đường học mechanic |
| Hard | 50 | Bài toán kết hợp cho người đã hiểu core rule |
| Challenge | 54 | Puzzle độc lập với điều kiện exit riêng |
| **Tổng** | **284** | Phạm vi bắt buộc của V2 1.0 |

`legacy-catalog.json` là canonical content hiện tại. `sourcePath` và `sourceHash`
giữ provenance; không dùng prefab AssetRipper làm runtime source.

## 2. Quy tắc bảo toàn content

- Không di chuyển tile, đổi solution hoặc loại level chỉ để engine hiện tại pass.
- Nếu solution V1 không replay được, điều tra rule/ordering trước dữ liệu level.
- Nếu dữ liệu thật sự hỏng, phải có bằng chứng, content patch có version và golden replay mới.
- Mỗi level có ID ổn định theo dạng `mode-NNN`; save không dựa vào index list thuần túy.
- Thay đổi content làm đổi hash và làm replay cũ incompatible một cách rõ ràng.

## 3. Điều kiện một level được nghiệm thu

Một level chỉ đạt `Verified` khi:

1. Qua structural validation.
2. Không có symbol hoặc reference không biết.
3. Load ra đúng initial state và không tự resolve trước input đầu nếu V1 không làm vậy.
4. Có ít nhất một command sequence kết thúc `Won` trên rules version hiện hành.
5. Replay lặp lại cho cùng terminal state/hash.
6. Undo/Restart không làm sai state.
7. Có playtest đọc được mục tiêu và feedback, không chỉ pass bằng automation.

Solution từ APK là golden candidate, không phải mã runtime. Level không có solution
phải được solver hoặc người thiết kế giải và lưu replay trước release.

## 4. Trạng thái validator ngày 2026-10-19

```text
276/284 Won
2 Incomplete
1 Lost
5 Rejected
0 Exception
```

- Normal: 172/180.
- Hard: 50/50.
- Challenge: 54/54.

### Backlog 8 level còn lại

| Level | Kết quả hiện tại | Việc cần hoàn tất |
|---|---|---|
| normal-090 | Rejected bước 46 | Khớp slide/gravity khi rắn leo và rơi |
| normal-093 | Rejected bước 21 | Khớp timing slide, đá và gravity |
| normal-115 | Rejected bước 10 | Khớp rule leo/rơi toàn thân |
| normal-119 | Hint hết trước Win | Sửa support/gravity khi thân còn bắc qua portal và spider web |
| normal-136 | Rejected bước 16 | Khớp slide/gravity |
| normal-175 | Lost bước 22 bởi spike | Sửa thời điểm gravity/hazard; V1 còn đi tiếp được |
| normal-177 | Không có solution V1 | Tìm lời giải bằng solver hoặc playtest rồi lưu golden replay |
| normal-179 | Rejected bước 10 | Hoàn thiện slide bar mang/đẩy rắn và vật thể |

Các mục trên là **release blocker** nhưng không chặn việc làm UI/asset độc lập.
Không thay đổi catalog trong lúc xử lý nếu chưa có bằng chứng content lỗi.

## 5. Nguyên tắc authoring và pacing

- Một level mới chỉ giới thiệu một mechanic chưa biết.
- Level xác nhận xuất hiện trước level kết hợp.
- Không yêu cầu input nhanh, pixel precision hoặc lợi dụng animation.
- Hazard phải nhìn thấy trước khi command được commit, trừ khi chính discovery đó
  là tutorial an toàn có Undo.
- Độ khó đến từ số tương tác cần suy luận, không từ che thông tin.
- Không đặt quảng cáo, reward hoặc economy vào bên trong state puzzle.

## 6. Difficulty metadata

Mỗi level nên có metadata phục vụ UI/QA, không ảnh hưởng simulation:

- `mode`, `number`, `difficultyBand`.
- `mechanicTags` và `tutorialTag`.
- `solutionLength` và `parMoves` nếu đã được curate.
- `contentHash`, `rulesVersion`, `verificationStatus`.

`bestMoves` là personal best. Par và difficulty không khóa progression hay phần thưởng P0.
