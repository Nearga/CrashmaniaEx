# CrashMania iOS — Implementation Roadmap

> This document defines the **ordered, feature-level implementation plan** for the CrashMania iOS Unity application.
> Each phase builds on the previous. Features within a phase can be tackled in the given order — earlier items are prerequisites for later ones.

---

## Phase 1 — Project Bootstrap & Core Infrastructure
*Goal: Unity project compiles, boots to a blank screen, DI wired, and PureMVC facade alive.*

### 1.1 Unity Project Setup
- [x] Create Unity 6 project (`6000.x`) with URP (2D) render pipeline
- [x] Configure iOS build target: IL2CPP, ARM64, minimum iOS 16.0, portrait orientation lock
- [x] Add `.gitignore` for Unity (Library/, Temp/, Logs/, obj/, UserSettings/)
- [x] Set Canvas Scaler: `Scale With Screen Size`, Reference `1170×2532`, width match `0.0`

### 1.2 Package Installation
- [x] Import **TextMeshPro** (built-in package, generate essential resources)
- [x] Import **DOTween Pro** (Asset Store) — configure default ease settings
- [x] Import **UniTask** (OpenUPM: `com.cysharp.unitask`)
- [x] Import **PureMVC** (manual DLL or NuGet source in `Assets/Plugins/PureMVC/`)
- [x] Import **Addressables** (Unity Package Manager built-in)
- [x] Import **Newtonsoft JSON** (`com.unity.nuget.newtonsoft-json`)

### 1.3 Font & Design Token Assets
- [x] Import Murecho font family (Regular 400, SemiBold 600, Bold 700, Black 900) → generate TMP font assets
- [x] Import Saira Condensed Black 900 → generate TMP font asset
- [x] Create `DesignTokens.asset` ScriptableObject with all color and font references (per `spec_mobile_lobby.md §5.1`)
- [x] Create `AppConfig.asset` ScriptableObject (placed in `Assets/Resources/AppConfig.asset`) with all hardcoded settings (per `spec_master.md §4.1`)

### 1.4 Core Architecture — DI Container
Port the exact `DependencyContainer` + `[Inject]` pattern from LastOneOut:
- [x] `Assets/Scripts/Core/InjectAttribute.cs`
- [x] `Assets/Scripts/Core/DependencyContainer.cs` (singleton, `Register<T>`, `Resolve<T>`)
- [x] `Assets/Scripts/Core/ServiceLocator.cs` (static pass-through for convenience access)
- [x] Unit-smoke-test: register a `string` and resolve it in Editor Play Mode

### 1.5 PureMVC Foundation
- [x] `Assets/Scripts/PureMvc/LobbyFacade.cs` — extends PureMVC `Facade`, singleton
- [x] `Assets/Scripts/PureMvc/Notifications/LobbyNotifications.cs` — all notification name constants
- [x] `Assets/Scripts/PureMvc/LobbyFacade.Startup()` — registers all proxies and commands (stubs OK at this phase)
- [x] `Assets/Scripts/Core/Startup.cs` — MonoBehaviour, loads `AppConfig`, registers `IBackendService`, calls `LobbyFacade.GetInstance().Startup()`

### 1.6 Boot Scene
- [x] Create `Scenes/Boot.unity`
- [x] Add persistent root GameObject `[Startup]` carrying `Startup.cs`
- [x] `Assets/Scripts/Services/MockBackendService.cs` — stub implementation returning empty/hardcoded data with `UniTask.Delay(config.mockNetworkDelayMs)`
- [x] Boot immediately sends `LobbyNotifications.NavigateTo` → `"Login"` (auto-login not yet, just navigation smoke test)

---

## Phase 2 — Scene Navigation & Persistent Shell
*Goal: All scenes exist, tab bar works, back button works, fade transitions play.*

### 2.1 Scene Files
- [x] Create empty scenes: `Login.unity`, `Lobby.unity`, `Store.unity`, `Gifts.unity`, `Account.unity`, `Game.unity`
- [x] Add all scenes to Build Settings in the correct order

### 2.2 Navigation Command
- [x] `Assets/Scripts/PureMvc/Commands/Navigation/NavigateCommand.cs` — resolves `NavigationService`, calls `LoadScene(sceneName)`
- [x] `Assets/Scripts/Services/NavigationService.cs` — `LoadScene()` with fade-in/out using DOTween on a `TransitionOverlay` canvas
- [x] `Assets/Scripts/PureMvc/Commands/Navigation/SceneLoadedCommand.cs` — registers mediators for the freshly loaded scene

### 2.3 Scene-Local Shell Overlays
Each scene owns its shell overlays under its single root Canvas. Mediators are rebound after scene loads:
- [x] `[TransitionOverlay]` — full-screen black `CanvasGroup`, DOTween fade, sort order 300
- [x] `[HeaderOverlay]` — shared scene-local prefab used by Lobby and Game; Game adds only the fixed left Back slot
- [x] `[TabBar]` — `TabBarView.cs` + `Canvas` sort order 100; **hidden during Game scene**
- [x] `[ModalManager]` — `ModalView.cs` + `Canvas` sort order 200, queues and stacks modal prefabs
- [x] `[AudioManager]` — `AudioManager.cs`, background music loop + SFX pool (5 AudioSources)

### 2.4 HeaderView
- [x] Layout: Logo (107×47px equivalent in UGUI points), CC balance widget, SC balance widget
- [x] `AccumulateToBalance.cs` — DOTween float tween on TMP text, ease-out cubic, 0.5s duration
- [x] `HeaderMediator.cs` — listens to `BalanceUpdated` notification, calls `OnBalanceUpdated()` on view

### 2.5 TabBarView
- [x] 4 buttons: Home, Store, Gifts, Account; each has icon `Image` + label `TMP_Text`
- [x] Active tab: `brandPurple` tint, inactive: `textSecondary` — DOTween color + scale 0.15s transition
- [x] Each button fires `SendNotification(LobbyNotifications.NavigateTo, sceneName)`
- [x] `TabBarMediator.cs` — highlights active tab on `SceneLoaded` notification

### 2.6 SafeAreaPanel Component
- [x] `Assets/Scripts/UI/Components/SafeAreaPanel.cs` — applies `Screen.safeArea` as RectTransform offsets in `Awake()`; works for Dynamic Island and home indicator bar

---

## Phase 3 — Login Screen
*Goal: Tapping "Login" on a styled screen auto-logs in via mock and goes to Lobby.*

### 3.1 Login UI
- [x] Full-screen background image (dark gradient matching `#282b38`)
- [x] Logo centered, large
- [x] "Email" + "Password" TMP InputFields with underline style
- [x] "LOGIN" gradient CTA button (blue gradient, skewed `-5deg` via `SkewRect` component)
- [x] "Continue with Google" outline button
- [x] "Sign Up" text link below

### 3.2 Custom UI Components
- [x] `Assets/Scripts/UI/Components/SkewRect.cs` — `IMeshModifier` that shears all 4 vertices by a configurable angle
- [x] `Assets/Scripts/UI/Components/GradientImage.cs` — vertex color gradient top→bottom on `Image` component

### 3.3 Login Flow (Mock)
- [x] `Assets/Scripts/PureMvc/Proxies/AuthProxy.cs` — holds `PlayerProfile`, `AccessToken`, `IsAuthenticated`
- [x] `Assets/Scripts/PureMvc/Commands/Auth/LoginCommand.cs` — calls `MockBackendService.Login()`, populates `AuthProxy`, fires `LoginSuccess`
- [x] `LoginView.cs` / `LoginMediator.cs` — submit button calls `SendNotification(LoginRequest, credentials)`, listens for `LoginSuccess` → navigate to Lobby

### 3.4 Login Screen Implementation Plan
- [x] Plan how to implement the login screen to closely match the provided screenshots (`Research/app_patched/screenshots/Screenshots/1 Login screen/1.png`, `2.png`, and `3.png`).
- [x] The UI must be as close as possible to the screenshots. Most assets are static (use downloaded/unpacked assets from `Research/raw/index/images/` and `Research/raw/index/images/homepage/`).
- [x] Implement 4 main interactive buttons: "Login" and "Sign up" (top right), plus "Join now" and "Play for free" in the main content area.
- [x] The "Join now" and "Play for free" buttons should trigger the "Sign up" flow.

### 3.5 Login and Signup Popups
- [x] Implement login and signup popups matching screenshots (`Login - popup.png`, `Signup - pre-popup warning.png`, `Signup - popup.png`).
- [x] For the signup flow, the first step must show the pre-popup warning (`Signup - pre-popup warning.png`).
- [x] Keep functionality minimal for now: buttons should only trigger the flows (show/close popups) without any actual login/signup authentication logic.

---

## Phase 4 — Modal System & Shared UI Polish
*Goal: All modals use consistent enter/exit animations. Toast notifications work.*

### 4.1 ModalManager
- [x] `ModalView.cs` — Canvas sort order 200; queue of pending modal prefab requests; shows one at a time
- [x] `ModalMediator.cs` — listens to `ShowModal` / `HideModal` notifications
- [x] Entry animation: DOTween scale `0.8→1.0` + CanvasGroup fade `0→1` over 0.25s
- [x] Exit animation: reverse; then dequeue next modal if any
- [x] Tap outside (background overlay button) triggers dismiss

### 4.2 Toast Notification
- [x] `ToastView.cs` — anchored top-centre; slide down 0.3s, hold 2s, slide up 0.3s
- [x] `ShowToast` notification with message string parameter

### 4.3 DOTween Animation Pass
Review all animations against spec targets:
- [x] Scene transitions: fade 0.25s in + 0.25s out
- [x] Card press: scale punch 0.15s
- [x] Balance counter: 0.5s ease-out cubic
- [x] Carousel snap: 0.3s (arrow buttons snap by card width with DOTween ease-out)
- [x] Tab switch: color + scale 0.15s
- [ ] Promo banner auto-advance: 0.4s slide

### 4.4 iOS Resolution & Editor Fidelity
- [x] Standardize scene-owned runtime UI canvases on `1170×2532`, `Scale With Screen Size`, width match `0.0`; embedded shell prefabs inherit the scene canvas.
- [x] Use Unity MCP before layout changes; verify live editor state, console, hierarchy, and screenshots.
- [x] Treat iPhone portrait as source of truth; validate primary Lobby/Game layouts at `1170×2532` and `750×1334`.
- [x] Apply safe area only to interactive chrome/content in Lobby and Game, not decorative full-screen backgrounds.
- [ ] Fix login/section image sizing from source asset aspect instead of hardcoded heights.
- [x] Add verifier coverage for single scene canvases, CanvasScaler policy, iOS portrait lock, scene-level safe areas, and URP/iPhone quality settings.

Assumption: this replaces the old `matchWidthOrHeight = 0.5` expectation with width match `0.0` for portrait-first UI.

---

## Phase 5 — Lobby Screen
*Goal: Lobby displays game carousels loaded from mock data. Cards are tappable.*

### 5.0 Boot Scene — Dev Scene Loader Helper
*Dev-only quality-of-life component; introduces no new business logic or abstractions.*
- [x] `Assets/Scripts/Boot/DevSceneLoader.cs` — `MonoBehaviour` on `[Startup]` in `Boot.unity`; wraps the existing startup flow with two inspector-only conveniences:
  - `[SerializeField] string targetScene` — overrides the default `"Login"` destination so a developer can boot straight into any scene (e.g. `"Lobby"`, `"Game"`) without editing code. Falls back to `"Login"` when left empty.
  - `[SerializeField] bool useMock` — when checked, registers `MockBackendService` instead of the real service before handing off to the existing `Startup.cs` logic. Guards the field with `#if UNITY_EDITOR` so it is stripped from production builds.
  - Contains no pipeline, no new interfaces, and no new abstractions — it just sets the two values and calls into the already-existing `Startup` flow.

### 5.1 Mock Catalog Data
- [x] `Assets/Scripts/Services/MockCatalog.cs` — static class returning hardcoded `List<CategoryModel>`, `List<GameModel>`, `List<BannerModel>` (currently 5 banners, 5 chips, 3 visible carousels with 15 games)
- [x] Phase 5 asset audit recorded in `Project/docs/phase5_asset_inventory.md`; mock game data now prefers exported Unity app sprites where available
- [x] `MockBackendService.GetLobbyData()` returns `MockCatalog` data with simulated delay

### 5.2 PureMVC Wiring for Lobby
- [x] `Assets/Scripts/PureMvc/Proxies/CatalogProxy.cs` — holds `Categories`, `TopGames`, `Banners`; exposes `Search(query)`, `GetCategory(id)`, and `GetGame(id)`
- [x] `Assets/Scripts/PureMvc/Commands/Lobby/LoadLobbyDataCommand.cs` — called by `LobbySceneController.Show()`, populates `CatalogProxy`, fires `CatalogUpdated`
- [x] `LobbyView.cs` / `LobbyMediator.cs` — listens to `CatalogUpdated`, binds data, spawns category chips and carousel/card prefab instances

### 5.3 Promo Banner Carousel
- [x] `Assets/Resources/UI/Prefabs/PromoBanner.prefab` — reusable promo image view with resource sprite binding
- [ ] Add real carousel paging/dot indicators; current lobby binds only the first banner
- [ ] Auto-advance every 5s using DOTween Sequence; swipe gesture switches pages
- [ ] `PromoBannerMediator` listens to `BannersUpdated` and loads images via URL (or placeholder sprites)

### 5.4 GameCard Prefab
- [x] `Assets/Resources/UI/Prefabs/GameCard.prefab` — `Button` + `Image` thumbnail + TMP labels, configured by `GameCardView.Bind(GameModel)`
- [x] Repeated cards are instantiated from reusable prefabs by `GamesCarouselView`
- [x] DOTween scale punch on tap: `1.0 → 1.05 → 1.0` over 0.15s
- [x] Tap raises `GameSelected`; `LobbyMediator` sends `LobbyNotifications.LaunchGame`

### 5.5 GameCardTop10 Prefab
- [x] `Assets/Resources/UI/Prefabs/GameCardTop10.prefab` — separate reusable prefab for Lucky Week / top-style cards
- [x] Reworked top-style card frame/rank treatment with exported Unity MG slot sprites

### 5.6 GamesCarousel Prefab
- [x] `Assets/Resources/UI/Prefabs/GamesCarousel.prefab` — Title `TMP_Text` + "View All" button + horizontal `ScrollRect` content pane + arrow buttons
- [x] Left/right arrow nudge now clamps and eases horizontal content with DOTween
- [x] Left/right gradient fade `Image` overlays (pointer events disabled via `CanvasGroup.blocksRaycasts = false`)
- [x] `GamesCarouselView` instantiates and populates `GameCard` children from `CategoryModel`

### 5.7 Sticky Search & Category Chips
- [x] Search `TMP_InputField` is wired to `CatalogProxy.Search(query)` and refreshes results
- [x] Added 300ms debounce and restore full category layout when the search field is cleared
- [x] Horizontal `ScrollRect` of category chip buttons exists and filters to selected category
- [x] Active chip visual state has final yellow/black screenshot-style highlight
- [x] Sticky behaviour via scroll position listener (CategoryRail reparents to viewport when scrolled past)

### 5.8 Skeleton Loading Placeholders
- [ ] `Assets/UI/Prefabs/SkeletonCard.prefab` — same dimensions as `GameCard` but `Image` with shimmer shader material
- [ ] `Assets/UI/Materials/ShimmerMaterial.mat` — URP Sprite shader with animated UV offset; `Mathf.PingPong` or shader property tween

### 5.9 Implement UI From Screenshots
- [x] Runtime currently loads 5 Lucky Week games + 5 Crash Games + 5 Hot Games + 5 category chips + 5 banner records
- [x] Visual pass against `Research/app_patched/screenshots/Screenshots/2 Lobby`: header/tab proportions, promo section, carousel sizing, and card framing were rebuilt through Unity MCP
- [x] Clicking any game sends `LobbyNotifications.LaunchGame`
- [ ] Add subtle sound effect on game tap once a source UI click clip is available
- [x] Wire `LaunchGame` to the Phase 7 game loader

### 5.10 Remaining Work Plan
- [x] Use Unity MCP to compare current Boot→Lobby screenshots against the three Lobby target screenshots before each visual pass.
- [x] Finish shared lobby chrome: header, CC balance bar, right menu/gift block, and bottom tab bar proportions.
- [x] Rebuild `Lobby.unity` scene-owned layout directly in hierarchy: mission/promo area, multipliers strip, category rail, and carousel spacing.
- [x] Locate/download exact Lucky Twins promo and card art; if unavailable, document the gap and use closest raw/exported assets without screenshot crops.
- [x] Add carousel arrow easing and search debounce.
- [x] Add verifier checks for screenshot-critical scene sections, no builder dependency, card counts, and LaunchGame wiring.

### 5.11 MainPromo & Mission Pill Visual Pass
- [ ] Improve MainPromo view: add proper background, reposition elements, remove visible gaps/holes between promo and adjacent sections
- [ ] Improve Mission pill: add background styling, reposition, remove gaps
- [ ] Match reference screenshot `Research/app_patched/screenshots/Screenshots/2 Lobby/Screenshot_20260527-184342.png`


---

## Phase 6 — Balance, Currency & Store
*Goal: Header shows animated balances. Store page lists packages. Mock purchase updates balance.*

### 6.1 Balance Proxy
- [x] `Assets/Scripts/PureMvc/Proxies/BalanceProxy.cs` — holds `double BalanceCC`, `double BalanceSC`; exposes `Credit(cc, sc)` and `Debit(cc, sc)` — fires `BalanceUpdated` after each change

### 6.2 Currency Toggle
- [x] Header `CC` / `SC` mode toggle updates the active balance and highlight
- [x] `SettingsProxy.cs` — holds `ActiveCurrency` enum, `MusicOn`, `SFXOn`
- [ ] Switching mode fires `CurrencyModeChanged`; currency-aware carousel/store presentation is deferred

### 6.3 Store Scene UI
- [x] `Assets/Resources/UI/Prefabs/StoreItemCard.prefab` displays CC amount, SC bonus, and price with explicit serialized wiring
- [x] Lobby-owned `StorePanelView` / `LobbyMediator` populate store cards from `MockBackendService.GetStorePackages()`
- [x] `StorePanelView` instantiates reusable cards from the `StorePackage` model; a separate factory is unnecessary

### 6.4 Purchase Flow (Mock)
- [x] `Assets/Scripts/PureMvc/Commands/Lobby/PurchaseStoreItemCommand.cs` directly calls `MockBackendService.PurchasePackage(id)`, credits `BalanceProxy`, and fires `PurchaseComplete`
- [ ] Purchase confirmation modal is deferred; the current mock purchase remains immediate

---

## Phase 7 — Game Scene Shell & Crash Game Machine
*Goal: Tapping a game card loads the Game scene. Crash game runs its full loop.*

### 7.1 Game Loader
- [x] `Assets/Scripts/Services/IGameLoader.cs` — interface: `UniTask LoadGame(GameModel)`, `UniTask UnloadGame()`
- [x] `Assets/Scripts/Services/EmbeddedGameLoader.cs` — additive `SceneManager.LoadSceneAsync(game.SceneAddress, Additive)` with duplicate `EventSystem` / `AudioListener` sanitizing for editor multi-scene setups
- [x] `Assets/Scripts/PureMvc/Proxies/ActiveGameProxy.cs` — holds active `GameModel` and `GameSession`
- [x] `Startup` registers `IGameLoader` and `ICrashGameService` alongside the mock backend

### 7.2 LaunchGameCommand
- [x] `Assets/Scripts/PureMvc/Commands/Lobby/LaunchGameCommand.cs` resolves the selected game from `CatalogProxy`, starts a mock session, stores it in `ActiveGameProxy`, loads `Game.unity` additively, and sends `SceneLoaded("Game")` so shell overlays hide
- [x] Direct Boot-to-Game dev flow works through `DevSceneLoader.targetScene = "Game"`; `GameSceneController` creates a safe mock session when no active game exists

### 7.3 Game Scene Canvas & Layout
- [x] `GameCanvas` with `SafeAreaPanel` and `1170 x 2532` / width-match CanvasScaler policy
- [x] **Game Header** row: back button, level badge, CC/SC balance text, currency toggle button
- [x] **Viewport Container**: masked `RectTransform` with round-history pill area, scrolling grid background, multiplier/status text, rocket visual, flame particles, and crash explosion object; this proves the functional shell exists, not final animation fidelity
- [x] **Active Bets Accordion**: active-bets section with PLAYER / BET / MULTI / WIN header and runtime mock player rows
- [x] **Dual Bet Container**: `VerticalLayoutGroup` with two `BetPanel` prefab instances stacked vertically
- [x] Visual polish pass: replaced placeholder geometric rocket/menu treatment with extracted source art and retuned proportions against game references; the rocket is still static extracted art unless Phase 7.11 animator fallback or recovered Spine assets are present
- [ ] Accordion behavior polish: add actual collapse/expand interaction if still desired after screenshot pass

### 7.4 Core Visual Components
- [x] `Assets/Scripts/UI/Components/ScrollingGridBackground.cs` — `RawImage` material UV offset, speed driven by `SetSpeedFactor(multiplier)`; useful motion fallback, not the original layered space/countdown scene
- [x] `Assets/Scripts/Game/CrashCurveEvaluator.cs` — `GetMultiplierAtTime(t)` and `GetTimeAtMultiplier(m)` static helpers
- [x] `Assets/Scripts/Game/CrashGameController.cs` — implements `IGameController`, subscribes to the mock crash loop, and drives multiplier, rocket, history, player rows, and bet panels
- [x] Rocket GameObject with flame `ParticleSystem`; presence only, not source-faithful rocket animation
- [x] Crash explosion object deactivated by default and shown on crash; still uses temporary `rocket-start 1` art until a closer VFX source is identified
- [x] Optional fidelity pass: flame emission now scales with multiplier; rocket/background/button/header placeholders use extracted game sprites, but animated background/rocket state still requires Phase 7.11

### 7.5 BetPanel Prefab & State Machine
- [x] `Assets/Resources/UI/Prefabs/BetPanel.prefab` — bet amount display with `[-]`/`[+]`, quick buttons (`10K`/`20K`/`40K`/`60K`/`80K`), autoplay toggle, action button
- [x] `Assets/Scripts/UI/Game/BetPanelController.cs` — tracks `Idle`, `Pending`, `InFlight`, `Won`, and `Lost` states
- [x] Action button text/colour transitions with DOTween-backed `Image` colour changes
- [x] Runtime smoke test placed a mock bet during `Preparation`
- [x] Polish pass: replaced current template styling with extracted bet panel art; full autoplay behavior is implemented in Phase 11.2

### 7.6 Crash Game Mock WebSocket Loop (`MockBackendService` extension)
- [x] `ICrashGameService` contract added for crash loop events and local place/cancel/cashout requests
- [x] `MockBackendService` runs the local loop:
  - **PREPARATION** (8s): countdown events every 0.5s; pre-calculates crash point using HMAC-SHA256 formula from `spec_backend.md §7.2`; accepts/rejects bets
  - **FLIGHT** (dynamic): ticks every 50ms, evaluates `CrashCurveEvaluator.GetMultiplierAtTime(elapsed)`, emits multiplier updates, and resolves mock cashouts
  - **CRASHED** (2.5s): emits round end and settles uncashed local bets as losses
  - **INTERMISSION** (1.5s): resets and starts the next round
- [x] Mock 5–8 AI player names with deterministic bet amounts/cashouts per round
- [x] Local cashout/cancel/place bet paths are wired through `ICrashGameService`

### 7.7 Exit Game Flow
- [x] Back button → `ExitGameCommand` → `EmbeddedGameLoader.UnloadGame()` → navigate back to Lobby → restore TabBar and HeaderOverlay
- [x] Game uses the same shared Lobby header composition, with only the fixed left Back slot enabled
- [x] Runtime smoke test verified `Boot → Game → Back → Lobby` without new console errors/warnings

### 7.8 Verification
- [x] Added `Assets/Editor/Phase7GameVerifier.cs` with `Crashmania/Verify Phase 7 Game`
- [x] Verifier checks contracts, commands, proxy, loader, `Game.unity` hierarchy, two `BetPanelController` instances, CanvasScaler policy, build settings, and UI/PureMVC boundaries
- [x] `Crashmania/Verify Phase 7 Game` completed successfully
- [x] Play Mode screenshot captured under `Assets/Screenshots~/`; single-frame acceptance does not validate animated state transitions
- [x] Fixed runtime duplicate `EventSystem` / `AudioListener` warnings in additive/editor multi-scene flows
- [x] Fixed TMP missing-glyph warning by replacing the lobby online counter emoji with supported text

### 7.9 Crash Room Visual Fidelity Pass
- [x] Copied selected source art from `Research/deobfuscated/game/unity/ExportedProject/Assets` into `Assets/Resources/UI/Game/Extracted` without mutating `Research/`
- [x] Added `docs/phase7_game_asset_inventory.md` mapping copied sprites/textures to scene usage and source screenshots
- [x] Retuned `Game.unity` header, flight viewport, active bets, and dual bet panel hierarchy to use extracted sprites instead of blank geometric placeholders
- [x] Updated `BetPanel.prefab` so future panel instances carry extracted container/button/amount art
- [x] Extended `Crashmania/Verify Phase 7 Game` to assert imported art, assigned scene sprites, prefab art, CanvasScaler policy, and duplicate EventSystem/AudioListener safety
- [ ] Remaining visual gap: replace the temporary extracted `rocket-start 1` crash burst with a closer explosion/VFX asset if one is identified
- [x] Full autoplay behavior implemented and verified in Phase 11.2


### 7.10 Crash Room Graphics Recovery
- [x] Captured broken baseline screenshot under `Assets/Screenshots~/`
- [x] Added `docs/phase7_game_reference_map.md` with `720 x 1600` screenshot bands mapped to the `1170 x 2532` Unity canvas
- [x] Recovered `Game.unity` layout from screenshot proportions instead of stretching extracted atlas sprites across large surfaces
- [x] Rebuilt large game/header/viewport/active-bets/bet-panel surfaces as reference-colored scene-owned UI panels
- [x] Kept extracted sprites only for small icons and the aspect-preserved rocket where they fit the reference
- [x] Updated `BetPanel.prefab` to use non-stretched screenshot-style solid/tinted controls
- [x] Relaxed Phase 7 verifier away from forced large-surface sprite checks; it now checks layout bands, visible surfaces, controller refs, CanvasScaler, duplicate listener safety, and PureMVC boundaries
- [x] Captured recovery screenshots under `Assets/Screenshots~`: `phase7_game_recovery_playmode_720x1600.png`, `phase7_game_recovery_playmode_750x1334.png`, `phase7_game_recovery_playmode_1170x2532.png`
- [x] Band sanity check: broken baseline had 4 dark middle bands; recovery screenshots have 0 dark middle bands
- [x] Play Mode acceptance passed: Boot-to-Game, countdown/flight/crash, cancel, cashout win, lost state, Back-to-Lobby

### 7.11 Animation Fidelity Recovery
- [x] Spine feasibility audit: extracted project contains `spine-unity` / `spine-csharp` runtime source and raw build strings mention Spine animation fields, but no usable rocket `.json`, `.skel.bytes`, `.atlas.txt`, or `_SkeletonData` assets were found
- [x] Do not install Spine only for decoration; keep Spine as source-preferred if real skeleton data is recovered or provided later
- [x] Added `CrashRocketAnimator` passive component for pseudo-Spine fallback: countdown idle bob, launch squash/tilt, multiplier-driven flight drift, flame intensity, glow pulse, crash hide/burst, and intermission reset
- [x] Added `CrashBackgroundAnimator` passive component for scene-owned layered background state: countdown, flight, crash tint, intermission, and multiplier-driven parallax
- [x] Refactored `CrashGameController` to delegate visual state to the passive animators while keeping mock crash loop, betting, balance, history, and PureMVC behavior unchanged
- [x] Built and wired editable scene layers under `ViewportContainer`: `CountdownBackground`, `FlightSpaceBackground`, `Asteroids`, `Stars`, `Planet`, `GroundOrMoonLayer`, `SpeedLines`, and `CrashTint`
- [x] Extended Play Mode verification with timed probes for countdown, later flight (`5.50x` / `20.65x` observed), crash, and intermission/reset frames; captured `screenshot-20260601-124420.png`, `screenshot-20260601-124441.png`, and `screenshot-20260601-124456.png`
- [ ] Replace `rocket-start 1` crash burst with a closer original VFX asset if one is identified


---

## Phase 8 — Bonuses & Gifts Screen
*Goal: Gifts tab shows bonus cards with live timers. Hourly and daily bonuses claimable.*

### 8.1 Bonus Proxy
- [ ] `Assets/Scripts/PureMvc/Proxies/BonusProxy.cs` — holds `List<BonusStatus>`; runs client-side countdown coroutine; fires `BonusTimerTick` every second

### 8.2 Gifts Scene UI
- [ ] Grid of `BonusCard` prefabs: icon, title, timer or "CLAIM" CTA
  - **Hourly Bonus**: clock icon, countdown `HH:MM:SS`; when available, green "CLAIM" button
  - **Daily Streak**: flame icon, current streak day indicator (day 1–7 pills); countdown; claim CTA
  - **Monthly Calendar**: compact 5×6 grid of day cells; claimed = purple tick; today = highlighted; future = locked
  - **Mystery Wheel**: "SPIN" button, triggers `WheelModal`
- [ ] `ClaimBonusCommand.cs` — calls `MockBackendService.ClaimBonus(type)`, receives `ClaimResult`, calls `BalanceProxy.Credit()`, fires `BonusClaimed`

### 8.3 Welcome Gift Modal (FTUE)
- [ ] On first login (`AuthProxy.IsFirstLogin`), show `WelcomeGiftModal` prefab
- [ ] Animated coin burst particle; shows `110,000 CC + 2 SC`; single "COLLECT" CTA
- [ ] `ModalMediator` queues this before any other modals

---

## Phase 9 — Account Screen
*Goal: Account tab shows profile info, sound/music toggles, logout.*

### 9.1 Account Scene UI
- [ ] Avatar `Image` (placeholder circle with initials), display name, email, VIP tier badge
- [ ] XP progress bar (`DOTween` fill on `Image` fillAmount)
- [ ] Sound toggle + Music toggle → update `SettingsProxy`, fire `ToggleSound` / `ToggleMusic`
- [ ] Logout button → `LogoutCommand` → clear `AuthProxy` → navigate to Login

### 9.2 Settings Persistence
- [ ] `SettingsProxy` saves `MusicOn`, `SFXOn`, `ActiveCurrency` to `PlayerPrefs` on change; restores on boot

---

## Phase 10 — iOS Polish & TestFlight
*Goal: App runs flawlessly on a real device. First TestFlight build delivered.*

### 10.1 iOS-Specific
- [ ] Verify `SafeAreaPanel` on iPhone 14 Pro (Dynamic Island) and iPhone SE 3 (no notch)
- [ ] Home indicator bottom safe area applied to TabBar
- [ ] Portrait lock enforced in `Player Settings → iOS → Allowed Orientations`
- [ ] App icon set (all required sizes) via `Assets/XcodeIcons/`
- [ ] Launch screen storyboard: logo centered on `#282b38` background
- [ ] `Info.plist` usage description strings for any permissions requested (none currently needed)

### 10.2 Performance Profiling
- [ ] Unity Profiler: target steady 60 FPS on iPhone 12 or newer
- [ ] Draw call budget: < 40 for Lobby, < 60 for Game scene
- [ ] Memory budget: < 200MB total (lobby), < 350MB (during game)
- [ ] Texture atlasing: sprite atlas for all UI icons and card thumbnails
- [ ] Enable IL2CPP stripping level `Medium`

### 10.3 Build & Delivery
- [ ] Archive Xcode build, upload to TestFlight
- [ ] Smoke test checklist:
  - Boot → auto-login → Lobby (< 3s)
  - Navigate all tabs; transitions < 0.5s
  - Launch Crash game; full round loop plays (countdown → flight → crash → next round)
  - Place bet, cash out; balance animates correctly
  - Claim hourly bonus; balance updates
  - Purchase store item (mock); balance updates
  - Logout → Login screen


---

## Phase 11 — Qualityautodecreas Polishing
*Goal: Final UI/UX refinements and animation tuning.*

### 11.1 Game Scene UI Layout Refinement
- [x] Move `ActiveBetsAccordion` under `DualBetContainer`
- [x] Make `ActiveBetsAccordion` scrollable for large record sets
- [x] Add header to the Game scene (reuse one from the lobby) and adjust position of `GameViewportContainer` (also, rename it from `ViewportContainer`)

### 11.2 Crash Game Autoplay Submenu
*Goal: tapping the autoplay control expands a compact bet-panel submenu that matches the reference screenshot and can repeatedly place/cash out mock Crash bets without breaking manual play.*
- [x] Use Unity MCP Play Mode screenshots as the visual source of truth before and after implementation; verify the submenu in the active runtime bet panel, not only in the edit-mode prefab.
- [x] Add `AutoplaySettings` data class to `CrashGameModels.cs`: enabled flag, selected round count index (0=∞, 1=10, 2=25, 3=50, 4=100), remaining rounds (-1=∞), cash-out multiplier (default 1.5x, min 1.1x, step 0.1x, max 100x).
- [x] Add `AutoCashOutMultiplier` nullable double field to `CrashPlayerBet` model.
- [x] Update `MockBackendService.PlaceBet()` to store `autoCashOutMultiplier` into `CrashPlayerBet.AutoCashOutMultiplier`.
- [x] Update `MockBackendService` flight tick loop to auto-resolve local player bets when `CurrentMultiplier >= bet.AutoCashOutMultiplier`.
- [x] Update `BetPanel.prefab` with artist-editable submenu hierarchy (initially code-created in `BetPanelController.Awake()`, to be migrated to prefab via Unity MCP):
  - Collapsed state: existing autoplay toggle + `AUTOPLAY` label in the bottom-left of each bet panel.
  - Expanded state: `AutoplaySubmenu` GameObject with `RoundPresets` horizontal group (∞, 10, 25, 50, 100 buttons) and `CashOutRow` ([-], value, [+] controls).
  - Preserve existing bet amount controls, quick bet buttons, and main `BET` action button proportions.
- [x] Add autoplay state and logic to `BetPanelController`:
  - `[SerializeField]` refs for submenu GameObject, round preset buttons, cash-out multiplier text, and increment/decrement buttons.
  - Toggle click expands/collapses submenu with 0.15s DOTween animation; selecting a preset enables autoplay and highlights the selected button.
  - Cash-out multiplier ±buttons adjust in 0.1x steps, clamped to [1.1x, 100x].
  - Auto-place during PREPARATION when autoplay enabled and panel is Idle (immediately on `OnCountdown` transition to Idle).
  - Auto-cashout during FLIGHT when `currentMultiplier >= autoplay.CashOutMultiplier`.
  - After round resolution, decrement finite round counts; stop at zero; infinite continues.
  - Manual cancel/cashout/disable toggle → cancel bet if Pending, cash out if InFlight, then deterministic Idle state.
  - New public `ResetAutoplay()` method for clean shutdown.
- [x] Update `CrashGameController.Shutdown()` to call `panel.ResetAutoplay()` on each bet panel.
- [x] Keep PureMVC boundaries clean: `BetPanelController` remains a passive UI/game component; no direct facade access.
- [x] Add verification coverage to `Crashmania/Verify Phase 7 Game` or a Phase 11 verifier for the submenu hierarchy, required button references, CanvasScaler policy, and two bet-panel instances.
- [x] Run Play Mode validation at `1170 x 2532` and a smaller portrait size:
  - open submenu, choose `10`, adjust cash-out multiplier, let one round auto-place and auto-cash-out;
  - verify finite count decrements, infinity does not decrement, manual disable stops new bets, and Back-to-Lobby clears autoplay state;
  - check console in Edit and Play Mode after script/prefab changes and fix all new warnings/errors.
- [x] Migrate code-created submenu hierarchy to `BetPanel.prefab` via Unity MCP for artist editability.


---

## Appendix — File Creation Order Summary

The following lists the **exact order** to create files from scratch (each depends on the previous group):

```
Group 1 — Core
  InjectAttribute.cs
  DependencyContainer.cs
  ServiceLocator.cs
  LobbyNotifications.cs
  LobbyFacade.cs
  AppConfig.cs (ScriptableObject)
  DesignTokens.cs (ScriptableObject)
  Startup.cs

Group 2 — Services & Models
  IBackendService.cs
  MockBackendService.cs (stubs)
  IGameLoader.cs
  EmbeddedGameLoader.cs
  NavigationService.cs
  [All Model classes: GameModel, CategoryModel, PlayerProfile, etc.]

Group 3 — Proxies
  AuthProxy.cs
  BalanceProxy.cs
  CatalogProxy.cs
  BonusProxy.cs
  SettingsProxy.cs
  ActiveGameProxy.cs

Group 4 — Commands
  LoginCommand.cs
  NavigateCommand.cs
  SceneLoadedCommand.cs
  LoadLobbyDataCommand.cs
  LaunchGameCommand.cs
  ExitGameCommand.cs
  PurchaseStoreItemCommand.cs
  ClaimBonusCommand.cs

Group 5 — UI Components (scripts only, no prefabs yet)
  SkewRect.cs
  GradientImage.cs
  SafeAreaPanel.cs
  SnapScrollRect.cs
  AccumulateToBalance.cs
  ScrollingGridBackground.cs
  ShimmerEffect.cs

Group 6 — Views & Mediators (paired with scene prefab work)
  HeaderView.cs + HeaderMediator.cs
  TabBarView.cs + TabBarMediator.cs
  ModalView.cs + ModalMediator.cs
  LoginView.cs + LoginMediator.cs
  LobbyView.cs + LobbyMediator.cs
  StoreView.cs + StoreMediator.cs
  GiftsView.cs + GiftsMediator.cs
  AccountView.cs + AccountMediator.cs
  GameView.cs + GameMediator.cs

Group 7 — Game Machine
  CrashCurveEvaluator.cs
  IGameController.cs
  CrashGameController.cs
  BetPanelController.cs
  MockBackendService.cs (crash loop added)
  GameCatalogMap.cs (AssetMap ScriptableObject)

Group 8 — Factories
  GameCardFactory.cs
  CarouselFactory.cs
  StoreItemFactory.cs
```
