# Phase 7 Game Asset Inventory

Source evidence is copied from `Research/deobfuscated/game/unity/ExportedProject/Assets`. The `Research/` files remain unchanged.

## Imported Textures
- `Texture2D/Main.png` -> `Assets/Resources/UI/Game/Extracted/Texture2D/Main.png`
- `Texture2D/Promo.png` -> `Assets/Resources/UI/Game/Extracted/Texture2D/Promo.png`
- `Texture2D/RocketDreams.png` -> `Assets/Resources/UI/Game/Extracted/Texture2D/RocketDreams.png`
- `Texture2D/IntroScreen.png` -> `Assets/Resources/UI/Game/Extracted/Texture2D/IntroScreen.png`

## Imported Sprites
- `Crash_mode_BG_default` -> flight viewport background
- `RocketDreams` -> rocket visual
- `rocket-start 1` -> crash/explosion visual placeholder from extracted game art
- `bet_ui_container` -> active bets and bet panel containers
- `Bet amount` -> bet amount field backing image
- `round_history_bg` -> round history pill template
- `ButtonGrey`, `ButtonRed`, `CancelButton`, `ChangeBetButton` -> game buttons and bet controls
- `Top Bar-*` coin, text-field, toggle, container sprites -> game header/balance/toggle art

## Reference Screenshots
Target screenshots for visual comparison live in `Research/app_patched/screenshots/Screenshots/3 Crash game`:
- `Screenshot_20260527-184442.png`
- `Screenshot_20260527-184456.png`
- `Screenshot_20260527-184503.png`
- `Screenshot_20260527-184513.png`

## Known Gaps
- No screenshot crop assets are used.
- The crash burst uses the closest extracted `rocket-start 1` sprite until a more exact explosion/VFX source is identified.
- Autoplay remains a visual toggle in this pass; no new autoplay gameplay loop was added.
