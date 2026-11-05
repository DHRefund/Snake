# Runtime state, snapshot và save data

**Trạng thái:** Accepted

## 1. Phân biệt ba loại dữ liệu

| Loại | Vòng đời | Ví dụ |
|---|---|---|
| Level definition | Bất biến, theo content build | Tile, entity spawn, solution |
| Runtime state | Một lần chơi level | Vị trí snake, door state, apples |
| Player save | Qua nhiều phiên chơi | Progression, setting, cosmetic |

Không serialize trực tiếp GameObject/MonoBehaviour vào bất kỳ loại nào.

## 2. Runtime BoardState

```text
BoardState
├── RulesVersion
├── BoardSize
├── Gravity
├── TileState[]
├── EntityState[]
├── SnakeState
├── TriggerState[]
├── RemainingApples
├── TurnIndex
└── Outcome
```

Entity state dùng stable `EntityId`, không dùng Unity instance ID.

## 3. Snapshot cho Undo

MVP ưu tiên full snapshot vì board nhỏ và ít lỗi hơn delta reverse:

```text
BoardSnapshot
├── TurnIndex
├── SnakeSegments[]
├── EntityStates[]
├── TriggerStates[]
├── RemainingApples
├── Gravity
└── Outcome
```

Snapshot không chứa:

- Sprite/Animator state.
- Transform world position.
- Audio đang phát.
- Reference tới view.

Giới hạn history theo memory budget, mặc định ít nhất 100 accepted turn. Nếu quá
giới hạn, bỏ snapshot cũ nhất; replay command list vẫn có thể giữ lâu hơn.

## 4. Player save schema

```json
{
  "schemaVersion": 1,
  "profileId": "local",
  "progress": {
    "normal": {
      "highestUnlocked": 12,
      "levels": {
        "normal-001": {
          "status": "completed",
          "bestMoves": 6,
          "firstCompletedUtc": "2026-10-18T00:00:00Z"
        }
      }
    }
  },
  "wallet": { "softCurrency": 0 },
  "monetization": {
    "recentRewardGrantsUtc": [],
    "rewardLedger": [],
    "completedLevelsSinceInterstitial": 0,
    "noAdsEntitled": false
  },
  "cosmetics": {
    "selectedSnakeSkin": "default",
    "unlocked": ["default"]
  },
  "settings": {
    "music": true,
    "sfx": true,
    "haptics": true,
    "reducedMotion": false
  },
  "meta": {
    "createdUtc": "...",
    "lastWriteUtc": "...",
    "gameVersion": "..."
  }
}
```

## 5. Migration

Mỗi schema version có migration một chiều:

```text
v1 → v2 → v3
```

- Không sửa dữ liệu trước khi backup.
- Migration idempotent hoặc chỉ chạy sau khi xác nhận version nguồn.
- Nếu migration thất bại, giữ file gốc và dùng profile mặc định tạm thời.
- Test fixture phải có save mẫu từ mọi version đã phát hành.

## 6. Atomicity và corruption

Quy trình save:

1. Serialize sang buffer.
2. Validate buffer bằng deserialize và checksum.
3. Ghi `save.tmp`.
4. Di chuyển save hiện tại thành backup.
5. Replace bằng temp.
6. Giữ tối thiểu một backup hợp lệ.

PlayerPrefs chỉ dùng cho flag không quan trọng hoặc bootstrap pointer, không dùng
làm kho dữ liệu progression chính.

## 7. Autosave policy

Save khi:

- Level completed/skipped.
- Setting hoặc cosmetic selection thay đổi.
- Purchase/reward transaction hoàn tất.
- App pause/quit nếu có dirty data.

Không ghi file sau mỗi turn puzzle nếu không có resume-mid-level feature.

## 8. Resume giữa level

Không nằm trong milestone đầu. Nếu bổ sung, lưu:

- Level ID + content hash + rules version.
- Snapshot committed mới nhất.
- Accepted command history tùy chọn.

Nếu content hash không khớp, restart level và thông báo phù hợp thay vì cố load
state cũ vào luật mới.

## 9. Security

Save local không thể chống gian lận tuyệt đối. Chỉ cần:

- Không lưu secret/API key.
- Receipt do store/SDK quản lý và validate theo yêu cầu.
- Checksum dùng phát hiện corruption, không quảng cáo là chống cheat.
- Analytics consent không nằm lẫn với progression payload nếu platform yêu cầu.
- Consent source of truth do UMP quản lý; không tự suy diễn consent từ player save.
- `rewardLedger` chỉ lưu `adAttemptId`, trạng thái, reward và timestamp cần cho idempotency/cap;
  không lưu advertising ID hoặc raw SDK payload.
