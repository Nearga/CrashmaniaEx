# CrashmaniaEx Project Constitution

This document is the first project source of truth. Read it before planning, editing, or verifying work in this repository.

## Mission

CrashmaniaEx is a Unity-first reconstruction of the CrashMania mobile experience. The goal is a high-fidelity native iOS demo that can run without a live backend while preserving a clean production-style architecture.

The current deliverable is not a generic prototype. It is a screenshot-faithful Unity app with:
- Native Unity scenes for login, lobby, shell overlays, and games.
- Local mock services that simulate backend and auth/game data.
- PureMVC orchestration for app flow.
- Artist-editable scene and prefab layouts.

## Main Project Roots

- `Project/` is the active Unity reconstruction project.
- `Project/Assets/Scenes/` contains canonical Unity scenes: `Boot`, `Login`, `Lobby`, `Game`, and future shell scenes.
- `Project/Assets/Scripts/` contains app code.
- `Project/Assets/Resources/` contains runtime-loaded prefabs, sprites, config, and mock assets.
- `Project/Assets/Screenshots~/` contains generated Unity verification screenshots. The trailing `~` keeps screenshot artifacts out of Unity's imported asset database.
- `Project/docs/` contains working specifications, roadmap, asset inventories, and this constitution.
- `Research/raw/` is source evidence downloaded from the web app. Do not edit it.
- `Research/deobfuscated/` is source evidence from deobfuscated web/game extraction and AssetRipper. Do not edit it.

When source evidence is needed, copy selected assets into `Project/Assets/...` and document the source. Do not mutate `Research/`.

## Architecture Rules

- The app uses PureMVC as the main application coordination pattern.
- Views are passive Unity components. They expose events and render/bind data; they must not call `LobbyFacade.GetInstance()` or `SendNotification`.
- Mediators translate view events into notifications and update views in response to notifications.
- Commands perform flow work such as scene loading, mock data loading, login, and startup.
- Proxies own queryable app state such as catalog, profile, auth, balance, and game state.
- Services wrap infrastructure: mock backend, navigation, storage, and future real backend clients.
- Scene controllers implement scene-specific PureMVC lifecycle through `Show`/`Close` semantics. Generic scene loading code must not hard-code scene-specific mediators/views.

## Unity Layout Rules

- **Single Canvas Architecture**: We always have a single canvas for rendering per scene. Each scene (e.g., `Login.unity`, `Lobby.unity`, `Game.unity`) must feature a single root Canvas component (e.g., `LoginCanvas`, `LobbyCanvas`, `GameCanvas`) containing all screen-level UI elements (including Header, TabBar, Toasts, and Modals). There must be no separate runtime-instantiated global Canvas overlays (except the system-level `TransitionOverlay`). All shell prefabs (Header, TabBar, etc.) are embedded directly inside the scene Canvas in the hierarchy so the Editor reflects the Game screen exactly.
- **Dynamic Mediator Binding**: Because shell overlay views are local to each scene Canvas and are destroyed/loaded with the scene, PureMVC mediators (like `HeaderMediator`, `TabBarMediator`) must be re-registered to bind to the new scene's local view instances on every scene load.
- **Shared Header Contract**: Lobby and Game use the same scene-local `HeaderOverlay` prefab and visual composition. Game enables only an additional fixed left Back slot; it must not carry independent header layout overrides or runtime anchor rewrites.
- Use Unity MCP before Unity changes. Inspect live editor state, hierarchy, components, console, and screenshots before and after scene/prefab work.
- Do not guess blindly from YAML or code when Unity MCP can verify the actual editor state.
- **Visual-First Layout Rule**: All layout is always as visual as possible, for designers to work on. Avoid code creation of elements as much as possible. One-off visual screens are laid out directly in the scene hierarchy.
- Repeated visual units may be prefabs, for example game cards, category chips, carousel rows, promo cards, shell header, and tab bar.
- Runtime code may populate repeated prefabs and bind behavior, but must not be the primary source of one-off visual hierarchy.
- Do not add editor “builder” scripts for scene layout unless explicitly approved. Verifiers are allowed and encouraged.

## Resolution & iOS Policy

- Portrait iPhone is the source of truth.
- Runtime UI canvases use `1170 x 2532` reference resolution.
- CanvasScaler policy is `Scale With Screen Size`, `Match Width Or Height`, width match `0.0`.
- Decorative full-screen backgrounds should not be safe-area constrained.
- Interactive chrome and content should respect safe area where needed.
- Validate layouts at least against iPhone 14 Pro style `1170 x 2532` and smaller portrait `750 x 1334`.

## Current App Flow

- `Boot.unity` starts the app.
- `Startup` initializes config, dependency container, DOTween, PureMVC facade, shell overlays, and initial navigation.
- `DevSceneLoader` is a dev-only helper on Boot that lets the editor load a target scene such as `Login`, `Lobby`, or `Game` while still preparing mocks and shared startup.
- `Login.unity` owns the login landing hierarchy directly in-scene.
- `Lobby.unity` owns the lobby layout directly in-scene.
- Shared shell overlays currently use reusable prefabs: header, tab bar, modal overlay, toast overlay, and transition overlay.

## Mock Backend & Storage

- The demo defaults to local mocked services.
- Mock backend methods should be deterministic enough for verification and screenshot work.
- Login/register/token persistence should use local client storage plus mocked backend state until a real backend exists.
- If a token exists, startup may attempt mocked auth and skip login when the mock backend accepts it.
- Do not introduce external network dependencies for core demo flow unless explicitly requested.

## Asset Policy

- Treat `Research/raw/` and `Research/deobfuscated/` as evidence.
- Prefer original exported Unity sprites from `Research/deobfuscated/game/unity/ExportedProject/Assets/` for app chrome, native UI sprites, and extracted game cards.
- Prefer `Research/raw/index/images/` for web lobby/login artwork when no exported Unity sprite exists.
- Avoid screenshot-cropped assets unless no source asset exists and the user approves the fallback.
- Record important imports or source choices in `Project/docs/*asset_inventory*.md`.

## Visual Fidelity Policy

- Target screenshots are requirements, not loose inspiration.
- For login and lobby, compare against screenshots under `Research/app_patched/screenshots/Screenshots/`.
- If current result differs materially from target, investigate source assets/layout first, then fix through scene/prefab editing.
- Use screenshots from Unity MCP as part of verification for visual work.

## Verification Rules

- **After any Unity scene, prefab, or component change: take a screenshot via Unity MCP and visually confirm the result looks correct before finishing.** Do not end a turn without this step when visual output is involved.
- **Screenshot Artifact Location**: Save all generated Unity screenshots under `Project/Assets/Screenshots~/`. For Unity MCP screenshot tools, use `Assets/Screenshots~` as the project-relative output folder. Do not save generated verification screenshots under `Assets/Screenshots`, `Builds/Automation`, `Project/Builds`, or another location.
- **Rule of Specific Visual Evidence**: For visual fixes, reopen the saved screenshot artifact with an image viewer/tool and check the exact requested feature, not just that the scene rendered. If the requested change is subtle (stars, contrast, spacing, small icons, background layers), use a crop/zoom, layer/component inspection, or measurable pixel/feature check. If the artifact does not plainly show the intended result, keep iterating or report a concrete blocker.
- **Rule of Visual Continuity**: Full-screen and cropped screenshot review must explicitly check for holes, exposed camera color, empty bands, abrupt layer disruptions, clipping, overlap, and unintended asymmetry at screen edges and section boundaries.
- **Rule of Play Mode Screenshotting**: For dynamic, runtime-instantiated, or populated UI (like the Lobby carousels, game cards, and category chips), screenshots MUST be captured in active Play Mode (`manage_editor` play, wait, take screenshot, then stop). Never claim visual fidelity or completeness based on an empty Edit Mode template.
- **Rule of Console Exceptions**: Always check the Unity editor console (`read_console`) in both Edit and Play Mode after *any* C# logic or layout change. Fix all unassigned references, null pointers, compiler warnings, or runtime exceptions (e.g., unassigned TMP font atlas textures) immediately before finishing.
- Verifiers live under `Project/Assets/Editor/` and should assert phase-critical structure, assets, policy, and boundaries.
- Run the relevant `Crashmania/Verify ...` menu item after related work.
- For scene work, smoke test through Unity where practical.
- Report existing warnings separately from new errors. New compile/runtime errors are not acceptable.
- Last step of the task, before finishing the turn: open `Boot` scene, select the `[Startup]` object, and set the `Dev Scene Loader` property to load the scene we are currently working on.

## Coding Style

- C# uses standard Unity conventions: 4-space indentation, `PascalCase` public types/members, `camelCase` locals and private fields unless local style differs.
- Keep comments sparse and useful.
- Use DOTween for UI motion where the project already depends on it.
- Avoid adding abstractions unless they reduce real complexity or match an established project pattern.

## Non-Negotiables

- Read this constitution first.
- Use Unity MCP for Unity work.
- Do not guess through missing, stale, or contradictory evidence. If required context, screenshots, editor state, or tool results are unavailable or unreliable, stop and report the concrete error/blocker before proceeding.
- Keep visual layouts artist-editable in scenes/prefabs.
- Keep PureMVC boundaries clean.
- Do not mutate source evidence in `Research/`.
- Do not reintroduce layout builder scripts for Phase 5-style scene work.
- If Unity MCP or the editor is unavailable, stale, compiling, or broken, tell the user exactly what is blocking safe work.

## Technical Integrity & Efficiency Mandates

### 1. UI Reconstruction Policy
- **Threshold for Reconstruction**: If a UI component (e.g., TextMesh Pro, Button) fails to render or behave correctly after two surgical property edits, **do not continue patching**. Immediately destroy the component and recreate it from scratch via script. This bypasses hidden serialization corruption.
- **Prefab Synchronicity**: Always verify if scene instances are disconnected from their source prefabs (`PrefabUtility.GetPrefabInstanceStatus`). If disconnected and corrupted, delete and re-instantiate from the prefab rather than attempting to fix overrides.

### 2. Asset Health Verification
- **Font Sanity Checks**: Before implementing features using project font assets, perform a 1-turn "sanity check" by creating a temporary `GameObject` with the font to ensure atlas textures and materials are valid and generating meshes.
- **Batch Reference Binding**: Use single-execution "Bootstrap" scripts to find and link multiple serialized references in a component, rather than making sequential tool calls. This reduces tool-overhead and prevents partial state errors.

### 3. Verification Rigor
- **Mesh Validation**: When verifying text visibility, use `execute_code` to check `mesh.vertexCount > 0`. Never rely solely on screenshots if the rendering pipeline is suspect.
- **Redundancy Cleanup**: After any scene-level re-instantiation, explicitly search for and delete duplicate GameObjects by name or component type to prevent "ghost" UI elements.
