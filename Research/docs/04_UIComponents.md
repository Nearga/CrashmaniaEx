# CrashMania UI Components Specification

## 1. App Header

### Structure
```
┌──────────────────────────────────────────────────────┐
│  [Logo]                              [Login] [Signup]│
│  107×47px                             ◁ skewed btns ▷│
└──────────────────────────────────────────────────────┘
Height: 63px
```

### Specs
- Height: 63px
- Layout: Flex, space-between, align-center
- Logo: 107×47px, object-fit: contain
- Buttons container width: 45vw, max 210px
- Login button: outline style, uppercase
- Signup button: blue gradient fill (#4faaff → #1c4fc7), uppercase
- Both buttons height: clamp(2rem, 5vh, 2.1875rem)
- Font size: clamp(.8rem, 1.5vw + .5rem, 1.125rem)

---

## 2. Sticky Lobby Controls

### Structure
```
┌──────────────────────────────────────────────────────┐
│  [🔍 Search...] [Category1] [Category2] [Category3] │
└──────────────────────────────────────────────────────┘
Position: sticky (top: 144px + safe-area)
```

### Specs
- Position: sticky
- Top offset: `calc(144px + env(safe-area-inset-top, 0px))`
- Z-index: 4
- Background: #282b38 (matches page)
- Uses same grid as .main-container
- Contains search input and filter category chips

---

## 3. Games List (Carousel Section)

### Structure
```
┌──────────────────────────────────────────────────────┐
│  CATEGORY NAME              [View All] [◄] [►]      │
│  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐          │
│  │     │ │     │ │     │ │     │ │     │  ◁ scroll ▷│
│  │ img │ │ img │ │ img │ │ img │ │ img │            │
│  │     │ │     │ │     │ │     │ │     │            │
│  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘          │
│  Name    Name    Name    Name    Name               │
└──────────────────────────────────────────────────────┘
```

### Header Specs
- Layout: Flex, space-between, align-center
- Title: white, uppercase, 16px, MurechoBold font
- Margin: 20px inline (10px on desktop)
- Buttons gap: 12px

### View All Button
- Background: #000 (black)
- Color: white
- Height: 36px
- Padding: 5px 10px
- Border-radius: 1.5px
- Transform: skew(-5deg)
- Font: 14px, letter-spacing: 0.5px

### Arrow Buttons
- Background: #000
- Size: 36×36px
- Border-radius: 1.5px
- Transform: skew(-5deg)
- Arrow icon: 14×14px
- Disabled state: opacity 0.3, cursor not-allowed

### Games Track
- Horizontal scroll with cursor: grab
- Touch action: pan-y (vertical scroll allowed)
- Padding: 5px
- Display: flex, nowrap
- Edge fading: 40px gradient (60px on desktop)
  - Left: hidden by default, shown when scrolled
  - Right: shown by default, hidden at end

### Margin Top
- Mobile: 18px
- Desktop (≥600px): 22px

---

## 4. Game Card (Standard)

### Structure
```
┌─────────────┐
│             │
│  Thumbnail  │
│   Image     │
│             │
├─────────────┤
│  Game Name  │
└─────────────┘
```

### Specs
| Property | Mobile | Tablet (≥600) | Desktop (≥834) |
|----------|--------|---------------|----------------|
| Width | 120px | 160px | 180px |
| Gap (text-image) | 12px | 12px | 12px |
| Hover scale | 1.04× | 1.04× | 1.04× |

- Display: flex, column
- Cursor: pointer
- User-select: none
- Text-align: center
- Transition: transform 0.2s ease-in-out

### Image Wrapper
- `.game-card-image-wrapper` contains `.game-card-image`
- Images are typically 16:9 or 4:3 aspect ratio thumbnails

---

## 5. Game Card (Top 10 Variant)

### Structure
```
┌──────────────────────────────┐
│  ┌───┐  ┌──────────────┐    │
│  │ 1 │  │              │    │
│  │   │  │  Thumbnail   │    │
│  └───┘  │              │    │
│         └──────────────┘    │
│         Game Name           │
└──────────────────────────────┘
```

### Specs
- Layout: flex-row (horizontal), align-center
- Gap: 8px (mobile), 12px (tablet+)
- Width: auto (not fixed)
- Margin-left: 50px (mobile), 60px (tablet+) — space for rank number
- The rank number is positioned absolutely to the left

---

## 6. App Button (Primary)

### Structure
```
┌──────────────────────────────┐
│  BUTTON TEXT                 │  ← skewed container
│  (text counter-skewed)       │
└──────────────────────────────┘
```

### Specs
- Transform: skew(-5deg)
- Text counter-skew: skew(5deg) on inner content
- Background: linear-gradient(#4faaff, #1c4fc7)
- Color: white
- Text: uppercase, MurechoBlack font
- Border: solid black
- Border-radius: 1.5px - 3px
- Box-shadow: subtle black

### Variants
| Variant | Background |
|---------|-----------|
| Primary (CTA) | linear-gradient(#4faaff, #1c4fc7) |
| Success | #0fd250 (green) |
| Danger | #ff3f3c (red) |
| Disabled | linear-gradient(#6c6c6c, #4d4d4d) |
| Outline (Yellow) | transparent, border: 2px solid #fedd24 |
| Dark | #000 (black) |

---

## 7. Store Item Card

### Structure
```
┌─────────────────┐
│    ┌────────┐   │
│    │ coin   │   │  ← skewed card
│    │ image  │   │
│    └────────┘   │
│   💰 250,000    │
│   + 🟢 5.00 SC  │
│  ┌────────────┐ │
│  │   $4.99    │ │  ← price bar at bottom
│  └────────────┘ │
└─────────────────┘
```

### Specs
- Background: #8a3dea with background image pattern
- Border: 2px solid black
- Border-radius: 8px (in-game mini store)
- Transform: skew(-2deg) to skew(-5deg)
- Box-shadow: 2px 2px 0 2px #000
- Width: 140px (mini store), varies in full store
- Min-height: 120px
- Hover: scale(1.05) with skew maintained
- Active: scale(0.98)
- Transition: transform 0.12s

### Price Bar
- Position: absolute bottom
- Background: #000
- Border-radius: 0 0 6px 6px
- Padding: 8px 10px
- Font: 0.875rem, bold, white

---

## 8. Skeleton Loading State

### Specs
- Background: #2a2a2a
- Transform: skew(-5deg) — matches button/card skew
- Shimmer animation: 2s infinite
- Shimmer gradient: linear-gradient(90deg, transparent 0%, rgba(255,255,255,0.05) 50%, transparent 100%)
- Pointer-events: none
- Cursor: default

---

## 9. Modal / Overlay

### Structure
```
┌── backdrop (blur + dark) ──────────────────┐
│                                             │
│   ┌── modal card ────────────────────┐      │
│   │  [×] Close                       │      │
│   │                                  │      │
│   │  Title                           │      │
│   │  Content                         │      │
│   │                                  │      │
│   │  [Cancel]  [Confirm]             │      │
│   └──────────────────────────────────┘      │
│                                             │
└─────────────────────────────────────────────┘
```

### Specs
- Overlay: fixed inset 0, z-index 9
- Backdrop: blur(4px), background #000000b3
- Modal: background #282b38, border-radius 8px
- Box-shadow: 0 8px 32px rgba(0,0,0,0.5)
- Close button: 24×24px, absolute top-right (12px offset)
- Width: 300px mobile, 485px desktop

---

## 10. Divider

### Structure
```
─────────────────────────────────
```
- Full-width image-based divider
- Multiple styles: hero-divider, hero-divider-dark
- Used between sections
- Some have scale/rotation transforms

---

## 11. Circular Progress / Loading

- Used as loading indicator
- Appears in lobby, gifts, and various async operations
- Border-based spinner: 3px border with colored left-border
- Colors: #ffffff1a background, #4ecdc4 active
- Sizes: 40px (mobile), 45px (tablet), 50px (desktop)
- Animation: 1s linear infinite spin
