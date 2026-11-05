# Hồ sơ tham khảo từ bản AssetRipper

**Trạng thái:** Accepted  
**Mục đích:** lưu sự thật quan sát được, không biến suy đoán thành luật V2

## 1. Nguồn và giới hạn

- APK/XAPK tham khảo: Snake + Apple 1.1.6.
- Project được xuất bằng AssetRipper.
- Project hiện khai báo Unity Editor `6000.3.21f1`; đây có thể không phải version
  Unity gốc đã build APK.
- Scene chính quan sát được là `Assets/AppleWorm/Scenes/SampleScene.unity`.

Các class và serialized field còn tên, nhưng đa số method body chỉ còn stub. Có
thể suy ra cấu trúc data và inventory mechanic, không thể coi stub là behavior.

## 2. Inventory level quan sát được

- 180 prefab trong `Resources/snakeapple/easy`.
- 50 prefab trong `Resources/snakeapple/hard`.
- 54 prefab trong `Resources/challenge`.
- Một prefab ở `Resources/snakeapple`, tạo tổng cộng 285 file.
- Mọi file đã quét đều có đúng một ký hiệu head và một exit.

## 3. Legacy MapData

Map thường có `SizeX = 40`, `SizeY = 30`, lưu bằng `char[] Data`. Metadata gồm
offset/scale, slide bar, button/door, teleport, reverse gravity và hint steps.
Offset/scale là presentation legacy và không cần đưa vào Domain V2.

## 4. Symbol mapping có độ tin cậy cao

| Legacy | Field | Ý nghĩa |
|---|---|---|
| `0` | VAL_FREE | Empty |
| `1` | VAL_BLOCK | Wall/block |
| `2` | VAL_APPLE | Apple |
| `3` | VAL_SNAKE | Snake body |
| `9` | VAL_SNAKEHEAD | Snake head |
| `8` | VAL_APPLEVIP | VIP apple |
| `X` | VAL_DESTINATION | Exit |
| `5` | VAL_ROCK | Rock |
| `6` | VAL_TOXIC | Toxic |
| `A` | VAL_SPIDER_NET | Spider net |
| `K` | VAL_BOMB | Bomb |
| `k` | VAL_BOMB_REMOTE | Remote bomb |
| `g,h,i,j` | VALS_SPIKE | Directional spikes |
| `q,r,s,t` | VALS_LASER | Directional lasers |
| `c,d,e,f` | VALS_TELEPORT | Teleport group |
| `C,D,E,F` | VALS_TELEPORT1 | Second teleport group |
| `L,M,N,O` | VALS_BUTTON1 | Button group 1 |
| `Q,R,S,T` | VALS_BUTTON2 | Button group 2 |
| `l,m,n,o` | VALS_DOOR1 | Door group |

Một số field như `VAL_BLOCK_DOOR`, `VAL_ROCKNSPIKE`, `VAL_ROCKNTOXIC` tồn tại
nhưng ký hiệu và behavior thực tế cần đối chiếu trước khi import.

## 5. Symbol đã thấy trong 285 prefab

```text
0 1 2 3 5 6 8 9 A X b c d e f g h i j k L m N o q R s T
```

Ký hiệu không xuất hiện vẫn có thể được lưu qua metadata hoặc pool state.

## 6. Class legacy quan trọng

- Điều phối: `GameController`, `GamePlayManager`, `LevelManager`,
  `GameplayController_AppleWorm`.
- Data/map: `MapData`, `AppleWormLevelMap`, `AppleWormData`, `MapPreviewer`.
- Snake: `AppleWorm_Worm`, `AppleWorm_WormHead`, `AppleWorm_WormNode`.
- Mechanic: `RockObject`, `BombObject`, `Teleport`, `Laser`, `OpenDoor`,
  `SlideBar`, `SpiderNet` và các button tương ứng.
- Meta/UI: `UndoManager`, `HintManager`, `SkinManager`, các Panel và SDK manager.

Tên class chỉ giúp lập inventory, không phải lý do giữ nguyên kiến trúc legacy.

## 7. Chính sách đối với khác biệt APK

Các điểm về ordering, spike, toxic, teleport, laser, bomb, spider net, slide bar
và exit đã được chuẩn hóa trong GDD và ADR-005. Catalog V1 là content chuẩn;
khác biệt làm solution chính thức thất bại phải được điều tra ở simulation trước.

Nếu quan sát APK sau này cho kết quả khác, ghi một compatibility note:

```text
Observation ID
APK version/device
Level ID và input sequence
Observed APK behavior
Corresponding V2 rule / ADR section
Impact on imported content
```

Compatibility note là bằng chứng đầu vào. Nếu tái hiện đáng tin cậy cho thấy GDD
khác V1 và ảnh hưởng solution/catalog, nhóm cập nhật GDD, test và ADR cùng một
thay đổi. Deviation cố ý mới cần quyết định Product owner và migration riêng.

## 8. Ruleset authority

Thứ tự ưu tiên khi có mâu thuẫn:

1. Hành vi tái hiện được từ game V1 phát hành và verified solution.
2. Dữ liệu level có provenance/source hash.
3. GDD và ADR Accepted mới nhất đã chuẩn hóa bằng chứng.
4. Requirements, schema và tests.
5. Implementation V2 hiện tại.
6. Tên class/field hoặc method body decompile không đầy đủ.

AssetRipper không tự nó là specification; giá trị của nó nằm ở dữ liệu có
provenance và bằng chứng có thể replay/tái hiện.
