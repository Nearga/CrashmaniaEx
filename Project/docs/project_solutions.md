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





