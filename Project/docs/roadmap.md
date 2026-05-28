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

### 2.3 Persistent Overlays (DontDestroyOnLoad)
These GameObjects are created once in Boot and survive all scene loads:
- [x] `[TransitionOverlay]` — full-screen black `CanvasGroup`, DOTween fade, sort order 300
- [x] `[HeaderOverlay]` — `HeaderView.cs` + `Canvas` sort order 100 (Lobby/Store/Gifts/Account modes); **hidden during Game scene**
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
- [ ] Card press: scale punch 0.15s
- [x] Balance counter: 0.5s ease-out cubic
- [ ] Carousel snap: 0.3s
- [x] Tab switch: color + scale 0.15s
- [ ] Promo banner auto-advance: 0.4s slide

### 4.4 iOS Resolution & Editor Fidelity
- [ ] Standardize all runtime UI canvases and overlays on `1170×2532`, `Scale With Screen Size`, width match `0.0`.
- [ ] Use Unity MCP before layout changes; verify live editor state, console, hierarchy, and screenshots.
- [ ] Treat iPhone portrait as source of truth; use Game View presets for iPhone 14 Pro, iPhone SE 3, and optional Pro Max.
- [ ] Apply safe area only to interactive chrome/content, not decorative full-screen backgrounds.
- [ ] Fix login/section image sizing from source asset aspect instead of hardcoded heights.
- [ ] Add verifier coverage for CanvasScaler policy, iOS portrait lock, safe-area overlays, and URP/iPhone quality settings.

Assumption: this replaces the old `matchWidthOrHeight = 0.5` expectation with width match `0.0` for portrait-first UI.

---

## Phase 5 — Lobby Screen
*Goal: Lobby displays game carousels loaded from mock data. Cards are tappable.*

### 5.1 Mock Catalog Data
- [ ] `Assets/Scripts/Services/MockCatalog.cs` — static class returning hardcoded `List<CategoryModel>`, `List<GameModel>`, `List<BannerModel>` (at minimum: Featured, Top 10, Crash Games, Slot Games categories; 8–10 game entries with placeholder thumbnails)
- [ ] `MockBackendService.GetLobbyData()` returns `MockCatalog` data with simulated delay

### 5.2 PureMVC Wiring for Lobby
- [ ] `Assets/Scripts/PureMvc/Proxies/CatalogProxy.cs` — holds `Categories`, `TopGames`, `Banners`; exposes `Search(query)` and `GetByCategory(id)`
- [ ] `Assets/Scripts/PureMvc/Commands/LoadLobbyDataCommand.cs` — called on `SceneLoaded`="Lobby", populates `CatalogProxy`, fires `CatalogUpdated`
- [ ] `LobbyView.cs` / `LobbyMediator.cs` — listens to `CatalogUpdated`, calls factory methods to spawn carousels

### 5.3 Promo Banner Carousel
- [ ] `Assets/UI/Prefabs/PromoBanner.prefab` — full-width `RawImage` + dot indicators
- [ ] Auto-advance every 5s using DOTween Sequence; swipe gesture switches pages
- [ ] `PromoBannerMediator` listens to `BannersUpdated` and loads images via URL (or placeholder sprites)

### 5.4 GameCard Prefab
- [ ] `Assets/UI/Prefabs/GameCard.prefab` — `Button` → `RawImage` thumbnail + `TMP_Text` name label; width 280px (reference resolution)
- [ ] `GameCardFactory.cs` — instantiates and configures `GameCard` from `GameModel`
- [ ] DOTween scale punch on tap: `1.0 → 1.05 → 1.0` over 0.15s
- [ ] Tap → `SendNotification(LobbyNotifications.LaunchGame, gameId)`

### 5.5 GameCardTop10 Prefab
- [ ] `Assets/UI/Prefabs/GameCardTop10.prefab` — horizontal layout with rank number `TMP_Text` (Saira Condensed Black, 96px) overlapping left of thumbnail; negative spacing for the overlap effect

### 5.6 GamesCarousel Prefab
- [ ] `Assets/UI/Prefabs/GamesCarousel.prefab` — Title `TMP_Text` + "View All" button + horizontal `ScrollRect` content pane
- [ ] `Assets/Scripts/UI/Components/SnapScrollRect.cs` — snaps to nearest card on release using DOTween, 0.3s
- [ ] Left/right gradient fade `Image` overlays (pointer events disabled via `CanvasGroup.blocksRaycasts = false`)
- [ ] `CarouselFactory.cs` — instantiates `GamesCarousel`, populates with `GameCard` children

### 5.7 Sticky Search & Category Chips
- [ ] Search `TMP_InputField` with 300ms debounce calling `CatalogProxy.Search(query)` → refreshes carousels
- [ ] Horizontal `ScrollRect` of category chip buttons; active chip highlighted with `brandPurple`
- [ ] Sticky behaviour via `LayoutElement` + scroll position listener (or fixed position approach)

### 5.8 Skeleton Loading Placeholders
- [ ] `Assets/UI/Prefabs/SkeletonCard.prefab` — same dimensions as `GameCard` but `Image` with shimmer shader material
- [ ] `Assets/UI/Materials/ShimmerMaterial.mat` — URP Sprite shader with animated UV offset; `Mathf.PingPong` or shader property tween

### 5.9 Implement UI From Screenshots
- [ ] `Research/app_patched/screenshots/Screenshots/2 Lobby` - 5 main games + 5 top games + 5 hot games + 5 promo banners + 5 category chips
- [ ] Clicking any game should play a subtle sound effect and send a notification to the game loader to load the game.


---

## Phase 6 — Balance, Currency & Store
*Goal: Header shows animated balances. Store page lists packages. Mock purchase updates balance.*

### 6.1 Balance Proxy
- [ ] `Assets/Scripts/PureMvc/Proxies/BalanceProxy.cs` — holds `double BalanceCC`, `double BalanceSC`; exposes `Credit(cc, sc)` and `Debit(cc, sc)` — fires `BalanceUpdated` after each change

### 6.2 Currency Toggle
- [ ] Segmented control in header: `CC` / `SC` mode toggle
- [ ] `SettingsProxy.cs` — holds `ActiveCurrency` enum, `MusicOn`, `SFXOn`
- [ ] Switching mode fires `CurrencyModeChanged` → carousel/store items react to show relevant content

### 6.3 Store Scene UI
- [ ] `Assets/UI/Prefabs/StoreItemCard.prefab` — `SkewRect` (-5°), purple background, coin icon `Image`, CC amount `TMP_Text` (fontEmphasis), SC bonus line, black price bar with price `TMP_Text`; hover scale 1.05, tap scale 0.98
- [ ] `StoreView.cs` / `StoreMediator.cs` — populates grid from `MockBackendService.GetStorePackages()`
- [ ] `StoreItemFactory.cs` — instantiates cards from `StorePackage` model

### 6.4 Purchase Flow (Mock)
- [ ] `Assets/Scripts/PureMvc/Commands/Store/PurchaseStoreItemCommand.cs` — calls `MockBackendService.PurchasePackage(id)`, receives `PurchaseResult`, calls `BalanceProxy.Credit(cc, sc)`, fires `PurchaseComplete`
- [ ] Purchase confirmation modal: `"Are you sure?"` with Cancel / Confirm buttons; `ModalMediator` handles show/hide

---

## Phase 7 — Game Scene Shell & Crash Game Machine
*Goal: Tapping a game card loads the Game scene. Crash game runs its full loop.*

### 7.1 Game Loader
- [ ] `Assets/Scripts/Services/IGameLoader.cs` — interface: `UniTask LoadGame(GameModel)`, `UniTask UnloadGame()`
- [ ] `Assets/Scripts/Services/EmbeddedGameLoader.cs` — `SceneManager.LoadSceneAsync(game.SceneAddress, Additive)`
- [ ] `Assets/Scripts/PureMvc/Proxies/ActiveGameProxy.cs` — holds active `GameModel` and `GameSession`

### 7.2 LaunchGameCommand
- [ ] `Assets/Scripts/PureMvc/Commands/Game/LaunchGameCommand.cs` — reads game from `CatalogProxy`, stores in `ActiveGameProxy`, navigates to `"Game"` scene, hides TabBar and HeaderOverlay

### 7.3 Game Scene Canvas & Layout
- [ ] `GameCanvas` with `SafeAreaPanel`
- [ ] **Game Header** row: back button + Level Badge + CC/SC balance widget + Currency Toggle + Menu Button
- [ ] **Viewport Container**: masked `RectTransform` occupying ~50% of portrait height. Contains the Round History horizontally scrollable pill badges in the top left corner.
- [ ] **Active Bets Accordion**: collapsible vertical list of active players situated between the viewport and bet panels. Has a header (PLAYER, BET, MULTI, WIN).
- [ ] **Dual Bet Container**: `VerticalLayoutGroup` with two `BetPanel` prefabs stacked vertically.

### 7.4 Core Visual Components
- [ ] `Assets/Scripts/UI/Components/ScrollingGridBackground.cs` — `RawImage` material UV offset, speed driven by `SetSpeedFactor(multiplier)` (per `spec_game.md §5.2`)
- [ ] `Assets/Scripts/UI/Components/AccumulateToBalance.cs` — DOTween float tween on TMP text
- [ ] `Assets/Scripts/Game/CrashCurveEvaluator.cs` — `GetMultiplierAtTime(t)` and `GetTimeAtMultiplier(m)` static helpers
- [ ] Rocket sprite GameObject with `ParticleSystem` flame effect (intensity `emission.rateOverTime` = `multiplier * 5`)
- [ ] Explosion prefab (deactivated by default); plays on crash

### 7.5 BetPanel Prefab & State Machine
- [ ] `Assets/UI/Prefabs/BetPanel.prefab` — bet amount input with `[-]`/`[+]`, quick buttons (`10K`/`20K`/`40K`/etc.), autoplay toggle, action button
- [ ] `BetPanelController.cs` — tracks 4 states: `Idle` → `"BET"` (blue), `Pending` → `"CANCEL BET"` (red/yellow), `InFlight` → `"CASHOUT [amount]"` (orange), `Won/Lost` → wait for next round
- [ ] Action button is a `GradientImage` button with DOTween colour transition between states

### 7.6 Crash Game Mock WebSocket Loop (`MockBackendService` extension)
- [ ] `StartCrashGameLoop(AppConfig config)` — `async UniTask` infinite loop:
  - **PREPARATION** (8s): countdown events every 0.5s; pre-calculate crash point using HMAC-SHA256 formula from `spec_backend.md §7.2`; accept/reject bets
  - **FLIGHT** (dynamic): `while (currentMultiplier < crashPoint)` — tick every 50ms, call `CrashCurveEvaluator.GetMultiplierAtTime(elapsed)`, fire `GameMultiplierUpdate` event; check each pending bet for auto-cashout threshold
  - **CRASHED** (2.5s): fire `GameEnd` event; settle all uncashed bets
  - **INTERMISSION** (1.5s): clear player list; loop restarts
- [ ] Mock 5–8 AI player names with random bet amounts placed during countdown
- [ ] `CrashGameController.cs` — implements `IGameController`; subscribes to mock loop events; drives all visual state changes (per `spec_game.md §5.1`)

### 7.7 Exit Game Flow
- [ ] Back button → `ExitGameCommand` → `EmbeddedGameLoader.UnloadGame()` → navigate back to Lobby → restore TabBar and HeaderOverlay

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
