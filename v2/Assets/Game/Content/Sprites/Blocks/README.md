# Block sprite workspace

Kiến trúc chính thức được mô tả tại
`Doc/02-Architecture/05-block-autotile-rendering.md`.

```text
Blocks/
├── LegacyDefault/             Asset/layer gốc V1, chỉ dùng reference và migration
└── Skins/
    └── Default/
        ├── Source/            File nguồn hoặc output trung gian trước khi bake
        ├── Runtime/           48 sprite hoàn chỉnh: 16 mask x 3 variant
        └── Preview/           Contact sheet và ảnh kiểm tra, không dùng trong gameplay
```

Không đặt sprite production mới vào `LegacyDefault`. Mọi file trong `Runtime` phải theo tên
`block_default_mXX_vYY.png` và mỗi sprite phải là một tile hoàn chỉnh.

Quy ước hình ảnh production: camera giả lập ở trên-trái, block lộ mặt trước + mặt trên + mặt trái.
Mặt trước/logical footprint là `256 x 256 px`; visual được tràn `62 px` sang trái và `65 px` lên trên.
Pivot logic luôn nằm ở tâm mặt trước, không phải tâm toàn bộ PNG. Thông số đầy đủ và quy tắc import nằm
trong tài liệu kiến trúc nêu trên.

Cấu hình skin production nằm tại `Content/BlockSkins/Default`; atlas nằm tại
`Content/SpriteAtlases/Blocks`.
