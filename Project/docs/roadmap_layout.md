# Android Layout Refactor Plan

## Objective
Refactor the Unity UI layout system to natively support a high variety of Android device aspect ratios, transitioning away from iOS-specific rigid scaling and safe area logic. We will leverage web-inspired fluid layouts combined with Unity's Canvas Scaler (Expand mode) and robust edge anchoring. 

**Mandate:** The layout architecture must remain entirely **visual and artist-editable**. Non-developers must be able to adjust it directly in the Unity Editor with zero programmatic structural generation.

---

## 1. Environment & Safe Area Cleanup
**Goal:** Remove all iOS-specific notch/island compensation logic to allow the UI to flush against Android screen edges.

- **Action:** Open `Lobby.unity` and `Game.unity`.
- **Action:** Locate and **Remove** the `SafeAreaPanel` component from all GameObjects (typically found on `MainCanvas/Background` or `UI_Root`).
- **Action:** In the `HeaderBar` and `TabBar` prefabs, ensure there are no "Top Padding" or "Bottom Padding" GameObjects designed to offset the UI from a notch.
- **Verification:** UI elements should now overlap the top/bottom status bar areas in the Game View when simulated.

## 2. Global Canvas Configuration
**Goal:** Establish a flexible scaling baseline that handles everything from 16:9 to 21:9 aspect ratios without stretching.

- **Action:** Select the `Canvas` GameObject in each scene.
- **Action:** Configure `Canvas Scaler`:
    - **UI Scale Mode:** `Scale With Screen Size`
    - **Reference Resolution:** `1080 x 1920` (Standard Android Full HD Portrait)
    - **Screen Match Mode:** `Expand` (This ensures the UI never cuts off; it adds extra "bleed" area on taller/wider screens).
    - **Reference Pixels Per Unit:** `100`
- **Action:** Set `Render Mode` to `Screen Space - Overlay` (unless camera-specific effects are required).

## 3. Visual Layout Architecture (Structural)
**Goal:** Use standard UGUI components to create a responsive "Top-Middle-Bottom" shell.

### 3.1 Header (Top)
- **GameObject:** `HeaderBar`
- **Anchors:** `Top / Stretch` (Min: 0, 1 | Max: 1, 1).
- **Pivot:** `(0.5, 1)`
- **RectTransform:** `Pos Y: 0`, `Height: 168` (Standard 56dp @ 3x density).
- **Component:** `VerticalLayoutGroup` or `HorizontalLayoutGroup` for internal elements, ensuring `Child Force Expand` is off for icons but `Control Child Size` is on.

### 3.2 Bottom Nav (Bottom)
- **GameObject:** `TabBar`
- **Anchors:** `Bottom / Stretch` (Min: 0, 0 | Max: 1, 0).
- **Pivot:** `(0.5, 0)`
- **RectTransform:** `Pos Y: 0`, `Height: 168` (Standard 56dp @ 3x density).

### 3.3 Main Content (Fluid Center)
- **GameObject:** `MainContent`
- **Anchors:** `Stretch / Stretch` (Min: 0, 0 | Max: 1, 1).
- **RectTransform Offset:** `Top: 168` (matches Header), `Bottom: 168` (matches TabBar).
- **Constraint:** Use a `VerticalLayoutGroup` on this container. 
- **Web-Style Clamping:** On the `MainContent` object, add a `LayoutElement`. 
    - Set `Preferred Width: 1080`.
    - Set `Flexible Width: 0`.
    - This ensures on very wide devices (foldables/tablets), the content doesn't stretch to awkward widths while the background remains full-screen.

### 3.4 Sizing Best Practices & Rationale
**Goal:** Align with industry standards for touch ergonomics and visual hierarchy on high-density Android displays.

| Standard | DP Value | PX (at 1080p / 3x) | Use Case |
|----------|----------|--------------------|----------|
| **Minimum Tap Target** | 48dp | 144px | Buttons, Icons, Links |
| **Standard App Bar** | 56dp | 168px | Global Header, Bottom Navigation |
| **Tall App Bar** | 64dp | 192px | Header with subtitle or tabs |
| **Gutter / Margin** | 16dp | 48px | Screen edges, between logical groups |
| **Small Spacing** | 8dp | 24px | Between related items (icon + text) |

- **Rationale:** Android devices range from ~320dpi to ~600dpi. By using a `1080x1920` reference resolution, we are targeting a "High" density bucket where `1dp ≈ 3px`. Adhering to the `56dp` standard ensures that our header and footer feel native and professional across all device physical sizes.

## 4. Artist-Editable Prefabs
**Goal:** Ensure every piece of UI is a standard Prefab that can be tweaked in isolation.

- **Rule:** **No code-generated UI.** If a new section is needed, the artist creates a Prefab and drags it into the `MainContent` VerticalLayoutGroup.
- **Rule:** Use `LayoutElement` on Prefabs to define their "Natural" height (`Min Height` / `Preferred Height`) so the `VerticalLayoutGroup` can stack them correctly without manual Y-coordinate math.

## 5. Verification & Testing
- **Tool:** Use Unity **Device Simulator**.
- **Test Set:** 
    1. **Samsung Galaxy S23** (Modern 19.5:9)
    2. **Google Pixel 4** (Standard 18:9)
    3. **Generic 16:9** (Older Android)
    4. **Samsung Galaxy Z Fold** (Wide/Square aspect ratio)
- **Checklist:**
    - [ ] No dark "gaps" at the very top or bottom of the screen.
    - [ ] Content is scrollable and reaches the bottom of the visible area.
    - [ ] Text remains legible and doesn't "smush" on narrow devices.
