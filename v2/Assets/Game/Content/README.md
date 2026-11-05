# Snake & Apple: Puzzle Adventure (V2) — Game Content & Assets

Thư mục này chứa toàn bộ tài nguyên hình ảnh (Sprites) và âm thanh (Audio) gốc của game, được phân loại phục vụ cho tầng **Presentation (Milestone M5)**.

## 1. Cấu trúc thư mục

```text
Assets/Game/Content/
├── Audio/                25 file SFX định dạng OGG chất lượng cao
└── Sprites/
    ├── Snake/            Đầu rắn, đốt thân (thẳng/cua), đuôi và sprite ăn mừng
    ├── Items/            Táo thường (apple.png) và Táo VIP (apple_luxury.png)
    ├── Environment/      Khối tường gạch (box_block.png) và Cổng thoát (MaskPortal.png)
    ├── Movables/         Hòn đá đẩy (rock.png) và Bom nổ (effectBomb)
    ├── Mechanics/        Mạng nhện (spider_net), bệ trượt, cổng teleport
    └── UI/               Icon di chuyển, hint, next level, settings, logo
```

## 2. Chi tiết phân loại

### Audio (`Content/Audio/`)
- `collect2.ogg`, `Cooking_Game_-_Actions_Veggie_Crunch_...ogg`: Âm thanh khi đầu rắn ăn táo.
- `moving2.ogg`: Âm thanh mỗi bước trườn/di chuyển.
- `blocked.ogg`: Âm thanh khi di chuyển vào vật cản không thể đi.
- `slipFall1.ogg`: Âm thanh khi toàn bộ thân rắn rơi do trọng lực.
- `hurt1.ogg`, `splat1.ogg`, `acid.ogg`: Âm thanh khi chết do va phải gai/axit.
- `win.ogg`, `levelUp.ogg`: Âm thanh hoàn thành màn chơi / vào cổng portal.
- `explode4.ogg`: Âm thanh bom nổ.
- `Coins_Single_26.ogg`, `coin_Pickup1.ogg`: Âm thanh nhận xu / thưởng.

### Sprites (`Content/Sprites/`)
- **Snake (`Sprites/Snake/`):**
  - `default_1.png`: Sprite đầu rắn mặc định.
  - `body_0.png`, `body_1.png`: Các đốt thân rắn.
  - `tail_0.png` - `tail_3.png`: Các đốt đuôi rắn.
  - `1_24.png`, `2_14.png`, `3_19.png`, `4_17.png`: Các góc bo cua thân rắn theo 4 hướng.
  - `endgame_worm_WIN.png`: Hình ảnh chú sâu ăn mừng chiến thắng.
- **Items (`Sprites/Items/`):**
  - `apple.png`: Quả táo đỏ thường (tăng 1 đốt).
  - `apple_luxury.png`: Quả táo vàng VIP (tăng 2 đốt).
- **Environment (`Sprites/Environment/`):**
  - `box_block.png`, `block_1_a.png`: Khối gạch tường/đất địa hình.
  - `MaskPortal.png`, `portal_rays.png`: Cổng không gian kết thúc màn chơi.
- **Movables (`Sprites/Movables/`):**
  - `rock.png`, `rock_0.png`: Hòn đá có thể đẩy.
  - `effectBomb 1 (2).png`: Quả bom nổ.
- **Mechanics (`Sprites/Mechanics/`):**
  - `spider_net.png`: Mạng nhện giữ chân.
  - `btn_thanh_truot_on.png`, `btn_thanh_truot_off.png`: Nút và thanh trượt ray.
  - `tele_gate_...png`: Cặp cổng teleport.
- **UI (`Sprites/UI/`):**
  - Nút điều khiển D-pad, nút Gợi ý (hint), nút Cài đặt, nút Tiếp theo, logo game.

## 3. Metadata & Import Settings
Mọi file Sprite và Audio đều đi kèm file `.meta` đã cấu hình sẵn chuẩn Unity 2D:
- Textures: `Texture Type = Sprite (2D and UI)`, `spritePixelsToUnits = 100`, `alphaIsTransparency = true`.
- Audio: `AudioImporter` với chuẩn Ogg Vorbis tối ưu hóa dung lượng cho mobile.
