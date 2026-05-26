# Repository Guidelines

## Project Structure & Module Organization

This repository is a Crashmania research and reconstruction workspace. `Research/raw/` stores original downloaded lobby and game assets; keep these files as source evidence and avoid editing them directly. `Research/deobfuscated/index/` contains processed web lobby bundles and annotations. `Research/deobfuscated/game/` contains AssetRipper output, including Unity assets and the exported project under `Research/deobfuscated/game/unity/ExportedProject/`. Reconstructed Unity scripts live in `Research/deobfuscated/game/unity/ExportedProject/Assets/Scripts/`, with Crashmania-specific code under `Assets/Scripts/Crashmania/`. Utility scripts are in `Research/scripts/`, and local third-party tooling is in `Research/tools/`.

## Build, Test, and Development Commands

- `./Research/scripts/01_download_assets.sh`: download lobby assets into `Research/raw/`.
- `./Research/scripts/03_deobfuscate.sh`: process downloaded JavaScript bundles into `Research/deobfuscated/index/`.
- `node Research/scripts/05_fetch_game_info.js`: fetch game metadata for analysis.
- `node Research/scripts/06_fetch_mg_games.js`: refresh the MultiGame catalog data.
- `./Research/tools/AssetRipper/AssetRipper.GUI.Free`: open AssetRipper for Unity extraction work.

There is no top-level app build command in this checkout. For Unity reconstruction, open `Research/deobfuscated/game/unity/ExportedProject/` in the Unity Editor and validate from there.

## Coding Style & Naming Conventions

Use 2-space indentation for JavaScript and JSON research scripts. Use standard C# conventions in Unity code: 4-space indentation, `PascalCase` for types and public members, `camelCase` for locals and private fields unless the surrounding file uses a different recovered pattern. Keep generated or deobfuscated filenames unchanged when they map back to original bundles, such as `index-CBIll7jp.js`. Name new research notes with a numeric prefix and clear topic, for example `Research/04_GameFlow.md`.

## Testing Guidelines

No automated test suite is currently defined. Validate script changes by running the specific script against a small, known input and checking generated files for deterministic output. For Unity work, open the exported project in Unity, check console errors, and verify affected scenes or prefabs manually. If adding tests later, place script tests beside the relevant utility or in `Research/tests/`, and use descriptive names like `deobfuscate-bundle.test.js`.

## Commit & Pull Request Guidelines

This checkout has no commit history, so use concise imperative commit subjects such as `Document research workflow` or `Annotate lobby bundle parsing`. Pull requests should describe the affected area, list commands or Unity checks performed, and note whether raw source evidence, deobfuscated output, or reconstructed Unity assets changed. Include screenshots only for visible lobby or Unity scene changes.

## Security & Configuration Tips

Do not commit credentials, session cookies, or private API responses. Treat `Research/raw/` as reproducible input and document download sources or dates when refreshing assets.
