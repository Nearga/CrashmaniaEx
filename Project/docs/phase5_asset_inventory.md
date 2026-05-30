# Phase 5 Lobby Asset Inventory

## Target Evidence
- Visual target: `Research/app_patched/screenshots/Screenshots/2 Lobby/`.
- Web promo source: `Research/raw/index/images/promotions/lobby-images/`.
- Web game thumbnails: `Research/raw/index/images/games/homepage-thumbnails/`.
- Exported Unity source: `Research/deobfuscated/game/unity/ExportedProject/Assets/`.

## Selected Sources
- Header/top bar: exported Unity sprites from `Assets/Sprite/Top Bar-*` with `Main.png`.
- Bottom tab bar: exported Unity sprites from `Assets/Sprite/Bottom Nav-*` with `Main.png`.
- Search/category chrome: exported Unity `Body-Search` and simple Unity UI color blocks.
- Crash game cards: exported Unity `Crash-*_thumbnail` sprites with `GameThumbnails.png`.
- Slot/top cards: exported Unity `MGSlots-*` sprites with `GameThumbnails.png`.
- Promo area: raw web lobby promo images remain the best available source for `mission`, `gift`, `gift-sweep`, `lobby-bg`, and `front-image`.

## Imported Into Reconstruction
- `Project/Assets/Resources/UI/NativeSprites/` contains selected exported sprite assets and their source texture atlases.
- `Project/Assets/Resources/UI/Promotions/Lobby/` contains the lobby promo images from the web bundle.

## Policy
- Prefer exported Unity sprites for app chrome and game cards.
- Prefer raw web assets only where exported Unity sprites do not contain the lobby promo artwork.
- Do not use screenshot-cropped reconstructed assets for Phase 5 unless no source asset exists and the user explicitly approves that fallback.
