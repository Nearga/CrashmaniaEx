# Phase 7 Game Asset Inventory

Source evidence is copied from `Research/deobfuscated/game/unity/ExportedProject/Assets`. The `Research/` files remain unchanged.

## Imported Textures
- `Texture2D/Main.png` -> `Assets/Resources/UI/Game/Extracted/Texture2D/Main.png`
- `Texture2D/Promo.png` -> `Assets/Resources/UI/Game/Extracted/Texture2D/Promo.png`
- `Texture2D/RocketDreams.png` -> `Assets/Resources/UI/Game/Extracted/Texture2D/RocketDreams.png`
- `Texture2D/IntroScreen.png` -> `Assets/Resources/UI/Game/Extracted/Texture2D/IntroScreen.png`
- `Texture2D/IntroScreen.png` crop -> `Assets/Resources/UI/Game/Extracted/Texture2D/IntroScreen_MoonMountains.png`
- `Texture2D/IntroScreen.png` star-only derived overlay -> `Assets/Resources/UI/Game/Extracted/Texture2D/IntroScreen_StarsOnly.png`
- `Texture2D/IntroScreen.png` standalone circle crop -> `Assets/Resources/UI/Game/Extracted/Texture2D/IntroScreen_FlightPlanet.png`

## Imported Sprites
- `Crash_mode_BG_default` -> flight viewport background
- `BG` -> source starfield sprite reference, sourced from the exported `IntroScreen.png` sprite rect
- `IntroScreen_StarsOnly` -> visible transparent star overlay, derived from the exported `IntroScreen.png` `BG` sprite rect so it can render above the Game scene blue/slate overlay bands
- `IntroScreen_MoonMountains` -> distant moon/mountain layer, cropped from the exported `IntroScreen.png` atlas
- `IntroScreen_FlightPlanet` -> source-derived flight planet fallback from the exported `IntroScreen.png` atlas; used only because no standalone flight planet/debris sprite was found
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
- Spine runtime source is present in the AssetRipper export and the raw WebGL build strings mention Spine animation fields, but no importable `.json`, `.skel.bytes`, `.atlas.txt`, or `_SkeletonData` rocket assets were found in `Project/Assets`, `Research/raw/game/unity_crash_game`, or `Research/deobfuscated/game/unity/ExportedProject/Assets` during the Phase 7.11 audit.
- The rocket animation therefore uses the available `RocketDreams` sprite plus layered glow, flame particles, squash/tilt, launch bob, and drift tweens as a pseudo-Spine fallback.
- The flight background uses scene-owned layered UI objects (`CountdownBackground`, `FlightSpaceBackground`, `Asteroids`, `Stars`, `Planet`, `GroundOrMoonLayer`, `SpeedLines`, `CrashTint`) instead of a recovered Spine background.
- Search evidence did not find standalone meteor, asteroid, pipe, tube, or debris sprites in `Research/deobfuscated/game/Assets/Sprite`, `Research/deobfuscated/game/Assets/Texture2D`, `Research/deobfuscated/game/unity/ExportedProject/Assets`, or `Research/raw/index`; the visible tube/rock pieces in `Research/raw/index_screenshots/03_crash_game/03_crash_game_1004_mobile.png` appear to be runtime/generated or unrecovered 3D/scene assets rather than exported 2D sprites.
- The crash burst uses the closest extracted `rocket-start 1` sprite until a more exact explosion/VFX source is identified.
- Autoplay remains a visual toggle in this pass; no new autoplay gameplay loop was added.
