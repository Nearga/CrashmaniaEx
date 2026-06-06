# Lobby Layout — Architecture & Findings

_Last updated: 2026-06-06. Lobby layout is now considered stable._

---

## Current Settled Architecture

The Lobby and Game scenes use a single-canvas, overlay-first layout following the constitution's iPhone portrait policy.

### Canvas

| Property | Value | Rationale |
|---|---|---|
| **Render Mode** | `ScreenSpaceOverlay` | Canvas always fills the exact physical screen. No camera dependency, no letterboxing. |
| **UI Scale Mode** | `Scale With Screen Size` | Scales UI elements proportionally. |
| **Reference Resolution** | `1170 × 2532` | Constitution-defined portrait source of truth. |
| **Match Width or Height** | `0` (match width) | Correct for portrait apps: width is the primary constraint. |

> [!IMPORTANT]
> `ScreenSpaceOverlay` was chosen over `ScreenSpaceCamera` specifically because `ScreenSpaceCamera` + `matchWidth = 0` caused **~160px letterboxing** at the top and bottom of the screen on non-16:9 devices (e.g. iPhone 828×1792). The canvas physical height was only 1472px on a 1792px screen, leaving the camera clear color exposed as black bars. Overlay mode bypasses the camera entirely and always fills the screen.

### Sibling Order in `LobbyCanvas`

Draw order (lowest index = drawn first / behind):

| Index | GameObject | Notes |
|---|---|---|
| 0 | `MainContent` | Scrollable lobby home content |
| 1 | `StorePanel` | Full-screen tab panel (inactive by default) |
| 2 | `GiftsPanel` | Full-screen tab panel (inactive by default) |
| 3 | `AccountPanel` | Full-screen tab panel (inactive by default) |
| 4 | `RedeemPanel` | Full-screen tab panel (inactive by default) |
| 5 | `HeaderOverlay` | Always on top of all panels |
| 6 | `TabBarOverlay` | Always on top of all panels |
| 7 | `ToastOverlay` | Notification toasts |
| 8 | `ModalManagerOverlay` | Modal dialogs |

> [!IMPORTANT]
> Tab panels **must** sit at lower sibling indices than `HeaderOverlay` and `TabBarOverlay`. A panel at a higher index than the shell chrome will render on top of the header and tab bar, hiding them. This was the original bug causing "no header or bottom panel" on Gifts/Store/Account screens.

### Header (`HeaderOverlay`)

- **Anchors:** Top-stretch (`anchorMin=(0,1)`, `anchorMax=(1,1)`)
- **Height:** `168` canvas units = 56 dp @ 3× density (standard Android/iOS app bar)
- **Background:** `Header Bar` image fills 100% of the overlay — `RGBA(0.018, 0.018, 0.022, 0.98)` (near-black)
- **Shared composition:** Lobby and Game inherit the same prefab layout, colors, typography, Gold Panel, and Right Menu.
- **Game-only difference:** Game enables the fixed left Back slot; Lobby collapses it.
- **Layout ownership:** `Header Bar` uses an artist-editable horizontal layout. Runtime code controls visibility/state only and must not rewrite header anchors.

### Tab Bar (`TabBarOverlay`)

- **Anchors:** Bottom-stretch (`anchorMin=(0,0)`, `anchorMax=(1,0)`)
- **Height:** `168` canvas units
- **Background:** `Tab Bar` image fills 100% — `RGBA(0.102, 0.114, 0.141, 0.98)` (dark blue)
- **Tabs wired (all clickable):** STORE, GIFTS, HOME/LOBBY, REDEEM, ACCOUNT

### Main Content (`MainContent`)

- **Anchors:** Full-stretch (`anchorMin=(0,0)`, `anchorMax=(1,1)`)
- **Offsets:** `offsetMin.y = 168` (above tab bar), `offsetMax.y = -168` (below header)
- Contains the vertical `ScrollRect` that fills it completely (both offsets zero).

### ScrollRect

- **Anchors:** Full-stretch inside `MainContent`
- **Offsets:** `(0, 0, 0, 0)` — fills `MainContent` edge-to-edge with no extra insets.

> [!WARNING]
> Do not re-introduce non-zero offsets on the `ScrollRect`. A prior safe-area-fitter pass had left `offsetMin.y = 142` and `offsetMax.y = -159` on the ScrollRect, which created large dark gaps above and below the scroll content that appeared as "holes" in the lobby.

### Content (inside ScrollRect/Viewport)

- `VerticalLayoutGroup` with `spacing = 0`, `padding.top = 0`, `padding.bottom = 20`
- `ContentSizeFitter` with `verticalFit = PreferredSize`

---

## Tab Panels

All tab panels (StorePanel, GiftsPanel, AccountPanel, RedeemPanel) share the same layout contract:

- **Anchors:** Full-stretch (`anchorMin=(0,0)`, `anchorMax=(1,1)`)
- **Offsets:** `(0, 0, 0, 0)` — fills the full canvas
- **Activation:** Controlled exclusively by `LobbyView.ShowTab(string tabName)`
- **Inactive by default** in the scene (only one active at runtime)

Because the panels are full-screen but sit *below* `HeaderOverlay` and `TabBarOverlay` in sibling order, the header and tab bar always render on top of whatever panel is showing — no extra positioning needed per panel.

---

## Tabs & Routing

Tab navigation is wired through PureMVC:

```
[TabBarView] TabSelected event
    → TabBarMediator.OnTabSelected(sceneName)
        → SendNotification(LobbyNotifications.NavigateToTab, sceneName)
            → NavigateLobbyTabCommand
                → If already in Lobby scene: sends ShowTab + SceneLoaded notifications
                → Otherwise: loads Lobby scene then sends both notifications
```

`LobbyView.ShowTab` handles all five tabs:

```csharp
if (lobbyPanel   != null) lobbyPanel.SetActive(tabName == "Lobby");
if (storePanel   != null) storePanel.SetActive(tabName == "Store");
if (giftsPanel   != null) giftsPanel.SetActive(tabName == "Gifts");
if (redeemPanel  != null) redeemPanel.SetActive(tabName == "Redeem");
if (accountPanel != null) accountPanel.SetActive(tabName == "Account");
```

`IsShellScene` in both `TabBarView` and `HeaderView` includes all five tabs (`Lobby`, `Store`, `Gifts`, `Redeem`, `Account`, plus `Game` for the header).

---

## Sizing Reference

| Standard | Canvas Units (px at 1080p/3×) | Use Case |
|---|---|---|
| Standard App Bar | `168` | Global Header, Bottom Navigation |
| Tall App Bar | `192` | Header with subtitle/tabs |
| Min Tap Target | `144` | Buttons, icons |
| Gutter / Margin | `48` | Screen edges, logical group gaps |
| Small Spacing | `24` | Icon + text pairs |

---

## Known Remaining Items

- [ ] **Safe area insets** — `HeaderOverlay` and `TabBarOverlay` currently have no runtime safe-area offset. On notched devices the header content (gold panel, balance) sits partially behind the status bar. A `SafeAreaFitter` script or manual per-device offsets should be added.
- [ ] **StorePanel content** — `StorePanelView` component is unassigned (the `StorePanel` is a placeholder label). Wire real store content when the Store tab design is finalized.
- [ ] **RedeemPanel content** — `RedeemPanelView` is a placeholder. Populate with redeem-code input and reward claim UI.
- [ ] **AccountPanel content** — placeholder label only.
- [ ] **Carousel section empty on some runs** — `CarouselSections` can appear empty if mock data loads late. Verify `LobbyMediator` receives `CatalogUpdated` reliably on every fresh lobby load.
- [ ] **Device Simulator testing** — verify at: Samsung Galaxy S23 (19.5:9), Pixel 4 (18:9), generic 16:9, Galaxy Z Fold wide/square.
- [ ] **Screenshot continuity audit** — verify Lobby and Game through Boot at `1170 x 2532` and `750 x 1334`, including holes, clipping, overlaps, disruptions, and unintended asymmetry.
- [ ] Save all generated verification captures under `Assets/Screenshots~` so Unity does not import screenshot artifacts.
