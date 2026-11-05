# Ngôn ngữ hình ảnh và accessibility

**Trạng thái:** Accepted

## 1. Visual hierarchy

Thứ tự chú ý mong muốn:

1. Snake head và hướng có thể đi.
2. Apple và exit.
3. Hazard/interactive object.
4. Wall/background.
5. HUD.

Background không cạnh tranh saturation/contrast với gameplay object.

## 2. Semantic consistency

- Cùng mechanic luôn có silhouette và animation state nhất quán.
- Red không tự động dùng cho cả apple và hazard nếu gây nhầm.
- Open/closed door khác nhau bằng hình dạng, không chỉ alpha.
- Teleporter pair có symbol/pattern ngoài màu.
- Button pressed có displacement và icon state.

## 3. Motion language

- Valid move: tween nhanh, ease-out nhẹ.
- Blocked: squash/nudge ngắn, không thay cell.
- Gravity: chuyển động tuyến tính dễ đọc.
- Teleport: entry disappear → exit appear, thời lượng ngắn.
- Undo: có thể snap hoặc reverse nhanh; không phát lại mọi VFX dài.
- Win: celebration sau khi trạng thái board đã rõ.

## 4. Text

- Tutorial dùng động từ và một ý mỗi message.
- Tránh paragraph trong gameplay.
- Font hỗ trợ đầy đủ tiếng Việt và ngôn ngữ mục tiêu.
- Không bake text vào sprite.
- UI layout hỗ trợ text dài hơn tiếng Anh khoảng 30–50%.

## 5. Color and contrast

- Text quan trọng đạt contrast phù hợp trên background.
- Không truyền trạng thái chỉ bằng đỏ/xanh.
- Có color-blind friendly palette hoặc icon thay thế.
- Laser/hazard vẫn nhận biết khi giảm saturation.

## 6. Reduced motion

Khi bật:

- Tắt camera shake.
- Giảm particle và scale bounce.
- Teleport dùng fade nhanh.
- UI transition rút ngắn.
- Không thay đổi timing simulation hoặc thời điểm nhận input một cách bất lợi.

## 7. Motor accessibility

- Swipe threshold không quá nhỏ.
- Button hit area tối thiểu phù hợp mobile.
- Không yêu cầu multi-touch hoặc thao tác thời gian chính xác.
- Cho phép D-pad thay swipe.
- Undo luôn dễ tiếp cận.

## 8. Cognitive accessibility

- Tutorial từng bước và có thể xem lại.
- Hint không dùng câu mơ hồ.
- Level select thể hiện rõ tiến độ.
- Cơ chế mới có visual introduction trước khi kết hợp.
- Không dùng fake urgency cho puzzle theo lượt.

## 9. Localization

- Mọi string dùng localization key.
- Số level và số lượng dùng format locale phù hợp.
- Không concatenate câu từ nhiều fragment nếu trật tự ngôn ngữ có thể khác.
- Screenshot test cho ngôn ngữ dài và font fallback.
