# Repository Guidelines

## First Source of Truth

**STOP. Before any action — planning, editing, or verifying — call `view_file` on `Project/docs/project_constitution.md` and confirm you have read it.** Do not proceed until that file has been read in the current session. It overrides all other guidance in this file when the two conflict.

## Project Structure & Module Organization

This repository is a Crashmania research and reconstruction workspace. `Project/` is the active Unity reconstruction project. `Project/Assets/Scenes/` contains canonical app scenes, `Project/Assets/Scripts/` contains runtime/editor C# code, and `Project/docs/` contains the working specifications, roadmap, asset inventories, and constitution. `Research/raw/` stores original downloaded lobby and game assets; keep these files as source evidence and avoid editing them directly. `Research/deobfuscated/index/` contains processed web lobby bundles and annotations. `Research/deobfuscated/game/` contains AssetRipper output, including Unity assets and the exported project under `Research/deobfuscated/game/unity/ExportedProject/`. Utility scripts are in `Research/scripts/`, and local third-party tooling is in `Research/tools/`.

## Build, Test, and Development Commands

- `./Research/scripts/01_download_assets.sh`: download lobby assets into `Research/raw/`.
- `./Research/scripts/03_deobfuscate.sh`: process downloaded JavaScript bundles into `Research/deobfuscated/index/`.
- `node Research/scripts/05_fetch_game_info.js`: fetch game metadata for analysis.
- `node Research/scripts/06_fetch_mg_games.js`: refresh the MultiGame catalog data.
- `./Research/tools/AssetRipper/AssetRipper.GUI.Free`: open AssetRipper for Unity extraction work.

There is no top-level app build command in this checkout. For active Unity reconstruction, open `Project/` in the Unity Editor and validate from there. Treat `Research/deobfuscated/game/unity/ExportedProject/` as source evidence, not the active reconstruction project.

## Android Deployment

Use the checked build output at `Project/Builds/Android/Crashmania.apk` when installing the current Android APK to a plugged device. If `adb` is not on `PATH`, use Unity's bundled Android SDK tool from the editor version recorded by the project build, for example:

- `& "C:\Program Files\Unity\Hub\Editor\6000.4.8f1-x86_64\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe" devices`: confirm the plugged device is listed as `device`.
- `& "C:\Program Files\Unity\Hub\Editor\6000.4.8f1-x86_64\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe" -s <device-id> install -r "D:\Local\Projects\Unity\CrashmaniaEx\Project\Builds\Android\Crashmania.apk"`: install or replace the app on that device.

If there is exactly one connected device, the `-s <device-id>` argument may be omitted. Keep the installed APK path explicit so agents do not accidentally deploy the generated Gradle intermediate APK from `Project/Library/`.

## Coding Style & Naming Conventions

Use 2-space indentation for JavaScript and JSON research scripts. Use standard C# conventions in Unity code: 4-space indentation, `PascalCase` for types and public members, `camelCase` for locals and private fields unless the surrounding file uses a different recovered pattern. Keep generated or deobfuscated filenames unchanged when they map back to original bundles, such as `index-CBIll7jp.js`. Name new research notes with a numeric prefix and clear topic, for example `Research/04_GameFlow.md`.

## Testing Guidelines

No automated test suite is currently defined. Validate script changes by running the specific script against a small, known input and checking generated files for deterministic output. For Unity work, open the exported project in Unity, check console errors, and verify affected scenes or prefabs manually. If adding tests later, place script tests beside the relevant utility or in `Research/tests/`, and use descriptive names like `deobfuscate-bundle.test.js`.

## Unity MCP Workflow

Always use Unity MCP for Unity Editor work. Before changing scenes, prefabs, UI layout, GameObjects, components, or Unity scripts, inspect the live editor state with MCP resources/tools, including relevant scene hierarchy, selected/persistent objects, component values, console errors, and screenshots when visual fidelity matters. Do not guess layout or behavior blindly from file contents alone when the editor can verify it.

If Unity MCP exposes an issue that can be fixed safely, fix it and re-check the editor state/console afterward. If Unity MCP is unavailable, disconnected, stale, blocked by compilation, or otherwise reporting something genuinely wrong, stop and tell the user exactly what is wrong and what must be opened, connected, compiled, or clarified before proceeding.

**CRITICAL MULTI-PHASE ALIGNMENT WORKFLOW RULES**:
1. **Always Verify in Active Play Mode**: For any scene or UI containing dynamic or runtime-instantiated contents (like the Lobby's game card lists, carousels, and category chips), screenshots MUST be captured in active Play Mode (`manage_editor(action="play")`, wait, take screenshot, then stop). Never make visual layout, padding, alignment, or completeness assertions based on static Edit Mode templates.
2. **Proactive Console Exception Auditing**: Check the Unity editor console logs (`read_console`) immediately after *any* C# logic or UI changes, and specifically *during/after* Play Mode. Fix all unassigned references, compiler warnings, or runtime errors (e.g. unassigned TMP font atlas textures) immediately.
3. **Typography & Asset Integrity**: Never force custom TMP font assets from `DesignTokens` onto UI components without first verifying that their atlas textures and dependencies are validly configured and assigned in the project resources.
4. **Screenshot Artifact Location**: Save every generated Unity verification screenshot under `Project/Assets/Screenshots~/`. For Unity MCP screenshot tools, pass `Assets/Screenshots~` as the output folder. Do not use `Assets/Screenshots`, `Builds/Automation`, or `Project/Builds`.

Avoid code-created UI components and child hierarchies for visual layout work. The point of visual editing is to split work cleanly between developers and artists: reusable UI should be built and adjusted as prefabs, while single-use UI should be laid out directly in the scene or prefab hierarchy. Runtime code may bind behavior, repair small non-visual settings, or populate data into existing slots, but it should not be the primary source of visual structure unless the user explicitly asks for generated UI.

## Commit & Pull Request Guidelines

This checkout has no commit history, so use concise imperative commit subjects such as `Document research workflow` or `Annotate lobby bundle parsing`. Pull requests should describe the affected area, list commands or Unity checks performed, and note whether raw source evidence, deobfuscated output, or reconstructed Unity assets changed. Include screenshots only for visible lobby or Unity scene changes.

## Security & Configuration Tips

Do not commit credentials, session cookies, or private API responses. Treat `Research/raw/` as reproducible input and document download sources or dates when refreshing assets.
