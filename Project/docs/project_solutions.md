# CrashmaniaEx — Project Solutions

This file records diagnosed bugs and their fixes. Consult it before investigating a recurring or familiar-sounding issue.

---

## ScrollRect unresponsive or masking content (Black Screen)

**Symptom**: 
- A vertical or horizontal `ScrollRect` does not scroll or register drag/scroll input in Play mode.
- OR, after adding a standard `Mask` component, the entire `ScrollRect` content turns completely black/invisible.

**Root cause**:
1. **Unresponsive scrolling (Missing clipping)**: The `Viewport` child of the `ScrollRect` was missing a clipping component. Without clipping, Unity's event system does not constrain drag inputs to the visible viewport boundaries, causing drag events to get eaten by off-screen children.
2. **Unresponsive scrolling (Missing raycast targets)**: Almost all background graphics and parent views (including `LobbyCanvas`, `Viewport`, `PromoSection`, `RecentMultipliers`, `CategoryRail`, and `CarouselSections`) had **`raycastTarget = false`** on their `Image` components. Because there was no graphic element under the mouse/finger capable of catching raycasts, Unity's `EventSystem` ignored pointer drag events entirely in empty spaces or content panels, rendering the `ScrollRect` completely unresponsive to drags.
3. **Black screen/invisible content**: A standard Unity `Mask` component clips based on the alpha channel of the Graphic (`Image`) component on the same GameObject. If the viewport's `Image` color has an alpha of `0` (`RGBA(0,0,0,0)`), the stencil mask treats the entire area as fully masked/invisible, rendering all children black/transparent.

**Fix**:
1. **Set `raycastTarget = true` on Viewports**:
   - Ensure the `Image` component on the `LobbyCanvas/ScrollRect/Viewport` GameObject has **`Raycast Target = true`**. This acts as the pointer hit catcher for empty spaces in the scrolling view.
   - Repeat for the horizontal `CategoryRail/ScrollRect/Viewport` image.
2. **Use `RectMask2D` instead of standard `Mask`**:
   - Select the `Viewport` GameObject.
   - Remove the standard `Mask` component if present.
   - Add a **`RectMask2D`** component. `RectMask2D` clips children perfectly inside its rectangle without requiring stencil buffers or relying on the Graphic's alpha channel.
   - Set the `Image` component's color to fully transparent `RGBA(0,0,0,0)`.
3. **Ensure Canvas renderMode supports Editor views & Screenshots**:
   - Change `LobbyCanvas` renderMode to **`ScreenSpaceCamera`**.
   - Assign the **`Main Camera`** to the canvas `worldCamera` field, and set `planeDistance = 5`. This ensures the UI renders correctly in camera-based viewport screenshots and maintains visual consistency across various aspect ratios.

**Verified**: 2026-05-29, Lobby scene. Replaced `Mask` with `RectMask2D` and enabled `raycastTarget = true` on both vertical and horizontal ScrollRect Viewport `Image` components. Configured `LobbyCanvas` as `ScreenSpaceCamera`. Verified high-fidelity rendering, beautiful screenshots, and working interactive scrolling. **Approved by user.**

---

## Carousel Button Overlap (ViewAllButton and PreviousButton)

**Symptom**:
- The slanted/skewed purple `VIEW ALL` button inside the `GamesCarousel` header overlaps with or sits extremely close to the left/previous navigation button `<`.

**Root cause**:
- The `ViewAllButton` is slanted/skewed at an angle of `-5` degrees using the `SkewRect` component.
- The `ViewAllButton` has a width of `190` and is anchored at `x = -122`.
- The `PreviousButton` has a width of `58` and is anchored at `x = -62`.
- This sets the gap between the rectangular boundaries of the buttons to only `2` units (from `-122` to `-120`).
- Because of the `-5` degree skew tilt, the top-right corner of the slanted `ViewAllButton` extends to the right by approximately `2.5` units, directly overlapping the left boundary of the `PreviousButton` and looking cluttered.

**Fix**:
- Adjusted the `RectTransform` anchored positions on the `GamesCarousel.prefab` template:
  - Kept the `NextButton` anchored at `x = 0`.
  - Moved the `PreviousButton` leftward to `x = -70` (providing a clean `12` unit gap from the `NextButton`).
  - Moved the `ViewAllButton` leftward to `x = -140` (providing a clean `12` unit gap from the `PreviousButton` even after accounting for the `-5` degree slant transformation).

**Verified**: 2026-05-29, GamesCarousel prefab. Headless template modifications applied. Spacing and skewing verified completely clear of overlaps in both Edit and Play Mode. **Approved by user.**

---

## Category Chips Vertical Clipping / Cut Off at Bottom

**Symptom**:
- The category selection chips (`ALL`, `LUCKY WEEK`, `CRASH GAMES` etc.) under the Search bar appear partially cut off or sliced flat at the bottom, looking like they are overlapped or cropped by the carousel or the search results header underneath them.

**Root cause**:
1. **Excessive MinHeight constraint**: The `LayoutElement` component on the `CategoryChip` prefab had `m_MinHeight = 94` units, which forced the runtime instantiated chips to be `94` units high even though their background graphic was only `68` units high.
2. **Constrained Scroller Viewport**: The `ScrollRect` scroller and its masking `Viewport` inside the `CategoryRail` parent object only had a height of `76` units.
3. **RectMask2D Masking**: Because the `Viewport` used a `RectMask2D` for horizontal clipping, any pixels exceeding the viewport's `76` height constraint (including the bottom `18` units of the oversized `94` chips) were forcefully clipped and hidden, giving the appearance of a flat cut-off bottom edge.

**Fix**:
- Corrected the `LayoutElement` template properties inside [CategoryChip.prefab](file:///Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Project/Assets/Resources/UI/Prefabs/CategoryChip.prefab): changed `m_MinHeight` from `94` to `68` to match its background height.
- Resized the category scroller in `Lobby.unity`: set the `RectTransform` height to `100` units (sizeDelta `y = 100`, anchoredPosition `y = 0`) on both `LobbyCanvas/ScrollRect/Viewport/Content/CategoryRail/ScrollRect` and its child `Viewport`.
- This ensures the `68`-unit category chips have a generous vertical breathing room inside the `100`-unit viewport mask, rendering fully and beautifully without any bottom clipping or overlapping.

**Verified**: 2026-05-29, CategoryChip prefab and Lobby scene. Resolved vertical constraints. High-fidelity rendering with perfect margins and full category buttons verified in Play Mode. **Approved by user.**

---

## MCP Server Invocation Case-Sensitivity (Gemini/Antigravity Environment)

**Symptom**:
- Attempting to call lazily-loaded MCP tools via direct `unityMCP:execute_code` or `unityMCP_debug_request_context` results in `unknown_tool` errors.
- Calling `call_mcp_tool` with lowercase parameter keys (`serverName`, `toolName`, `arguments`) also fails with `unknown_tool`.

**Root cause**:
- The Antigravity IDE defines a specific schema for `default_api:call_mcp_tool` which strictly requires **Capitalized PascalCase** keys for its arguments (`ServerName`, `ToolName`, `Arguments`, `toolAction`, `toolSummary`).
- Passing standard camelCase parameters fails schema parsing and results in a generic `unknown_tool` or parsing error.

**Fix**:
- Always call `default_api:call_mcp_tool` using the correct PascalCase argument layout:
  ```json
  {
    "name": "default_api:call_mcp_tool",
    "arguments": {
      "ServerName": "unityMCP",
      "ToolName": "debug_request_context",
      "Arguments": {}
    }
  }
  ```

**Verified**: 2026-05-30. Successfully tested with `unityMCP/debug_request_context` to fetch active Unity sessions and plugin status.

---

## Lobby Layout "Holes" — Visible Gaps Between Sections

**Symptom**:
- Dark visible gaps ("holes") appear between the major lobby sections (PromoSection, RecentMultipliers, CategoryRail, CarouselSections) when scrolling through the lobby content.
- Additional gaps appear between individual carousel rows inside CarouselSections.

**Root cause**:
- The `VerticalLayoutGroup` on the `Content` object inside `ScrollRect/Viewport` had `m_Spacing: 24` and `m_Padding.Bottom: 230`, creating 24px dark gaps between every major section and 230px of empty space at the bottom.
- The `VerticalLayoutGroup` on `CarouselSections` had `m_Spacing: 26`, creating 26px gaps between each game carousel row.

**Fix**:
- Changed `Content` VerticalLayoutGroup: `m_Spacing: 24 → 0`, `m_Padding.Bottom: 230 → 20`
- Changed `CarouselSections` VerticalLayoutGroup: `m_Spacing: 26 → 8`
- These values were edited directly in `Lobby.unity` scene YAML.

**Verified**: 2026-05-31, Lobby scene. Sections now flow flush with no visible dark gaps between them. Carousel rows have a minimal 8px spacing. Play-mode screenshot confirmed. **Approved by user.**

---

## Carousel Gradient Fade Overlays

**Symptom**:
- Game cards at the left and right edges of horizontal carousels are visible right up to the edge with no visual fade, making the scroll boundary unclear.

**Root cause**:
- No gradient fade overlays existed on the `GamesCarousel` prefab's `ScrollRect`.

**Fix**:
- Added `LeftFade` and `RightFade` child GameObjects under `ScrollRect` in `GamesCarousel.prefab`.
- Each has: `Image` component, `HorizontalGradientImage` component (left-to-right alpha gradient), `CanvasGroup` with `blocksRaycasts = false`.
- Left fade: opaque background color → transparent (hides left edge content when scrolled).
- Right fade: transparent → opaque background color (hides right edge content when more exists).
- `GamesCarouselView.UpdateFadeVisibility()` toggles fade visibility based on scroll position.

**Verified**: 2026-05-31, GamesCarousel prefab. Left fade hidden at scroll start, right fade visible when content overflows. Fades update on scroll. **Approved by user.**

---

## Carousel Arrow Snap Animation

**Symptom**:
- Arrow buttons nudge by a fixed 320px offset regardless of card size, and the animation duration was 0.18s with no snap behavior.

**Root cause**:
- `GamesCarouselView.Nudge()` used a hardcoded 320px offset and 0.18s duration.

**Fix**:
- Changed `Nudge()` to use `cardWidth` (290f) and `snapDuration` (0.3f) with `Ease.OutCubic`.
- Added `isSnapping` flag to track snap state.
- Set `scrollRect.decelerationRate = 0.01f` for tighter inertial deceleration.
- Added `scrollRect.onValueChanged` listener to update fade visibility on scroll.

**Verified**: 2026-05-31, GamesCarouselView. Arrow buttons snap by one card width with smooth ease-out. **Approved by user.**

---

## Sticky Category Rail on Scroll

**Symptom**:
- The search bar and category chips scroll away with the rest of the lobby content, making it necessary to scroll back up to filter games.

**Root cause**:
- No sticky behavior was implemented for the `CategoryRail` section.

**Fix**:
- Added `mainScrollRect`, `categoryRail`, and `stickyRailAnchor` serialized fields to `LobbyView`.
- Added `LateUpdate()` with `UpdateStickyRail()` that checks if the `CategoryRail`'s world position has scrolled past the viewport top.
- When sticky: reparents `CategoryRail` to the `ScrollRect` viewport and anchors it at the top.
- When not sticky: reparents back to the content with original anchored position.

**Verified**: 2026-05-31, Lobby scene. Category rail sticks to top when scrolled past. **Approved by user.**

---

## Category Chip Centering

**Symptom**:
- Category chips in the horizontal scroll rail are left-aligned when they should be centered when they fit within the viewport, and scrollable from the left edge when they overflow.

**Root cause**:
- The chip `Content` RectTransform had anchors at `(0, 0.5)-(0, 0.5)` (left-center) and pivot at `(0, 0.5)` (left-center), causing the content to always align from the left edge of the viewport regardless of whether it fit or overflowed.
- The `HorizontalLayoutGroup` had no horizontal padding, so chips touched the viewport edges.
- The `ScrollRect` initialized at `horizontalNormalizedPosition = 0.5` (center) when content overflowed with centered anchors, showing the middle of the chip list instead of the start.

**Fix**:
- Changed chip `Content` RectTransform anchors to `(0.5, 0.5)-(0.5, 0.5)` (center) and pivot to `(0.5, 0.5)` (center) — content centers when it fits, and ScrollRect handles overflow.
- Added `HorizontalLayoutGroup` padding: left=16, right=16 — chips have breathing room from viewport edges.
- Added `categoryScrollRect` serialized field to `LobbyView.cs` with auto-find fallback in `Awake()`.
- Added `categoryScrollRect.horizontalNormalizedPosition = 0f` reset after `RenderCategories()` — forces ScrollRect to start at the left edge when content overflows.
- Removed stale `mainScrollRect`, `categoryRail`, `stickyRailAnchor` serialized fields from `LobbyView.cs` and scene YAML.

**Verified**: 2026-05-31, Lobby scene. Chips start from left edge, centered anchors allow centering when content fits. ScrollRect starts at position 0. No console errors.

---

## PromoSection Layout Holes — MissionPill and MainPromo Gaps

**Symptom**:
- Visible dark gaps ("holes") between the MissionPill and MainPromo banner inside the PromoSection.
- MissionPill was positioned 25px from the top with a 39px gap between its bottom edge and the MainPromo top.
- Badges (PersonalOfferBadge, SpecialSaleBadge, WelcomeOfferBadge) had fully transparent backgrounds (`RGBA(0,0,0,0)`), making them invisible.
- PromoSection was 820px tall with wasted vertical space.

**Root cause**:
- MissionPill anchored at `y=-25` (25px from top), size 405×118.
- MainPromo anchored at `y=-182` (182px from top), creating a 39px gap between MissionPill bottom (25+118=143) and MainPromo top (182).
- Badge backgrounds were set to `RGBA(0,0,0,0)` — fully transparent.
- PromoSection `LayoutElement.preferredHeight` was 820px, larger than needed.

**Fix**:
- Moved MissionPill from `y=-25` to `y=-10` (reduced top gap from 25px to 10px).
- Moved MainPromo from `y=-182` to `y=-136` (only 8px gap below MissionPill bottom at 10+118=128, so 136-128=8px gap).
- Widened MissionPill from 405px to 600px for better visual presence.
- Improved MissionPill background color from `RGBA(0.08, 0.02, 0.12, 1.0)` to `RGBA(0.15, 0.05, 0.25, 1.0)` — brighter purple.
- Made badges visible with `RGBA(0.15, 0.05, 0.25, 0.85)` background color.
- Repositioned badges: PersonalOfferBadge at `(110, -136)`, SpecialSaleBadge at `(110, -280)`, WelcomeOfferBadge at `(110, -420)`.
- Reduced PromoSection `LayoutElement.preferredHeight` from 820 to 784.

**Verified**: 2026-05-31, Lobby scene. No visible gaps between MissionPill and MainPromo. Badges visible. PromoSection height reduced to 784px. No console errors.





