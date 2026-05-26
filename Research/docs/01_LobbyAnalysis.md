# CrashMania Lobby - Website Investigation Report

## 1. Platform Overview

- **URL**: https://game.crashmania.com/lobby
- **Type**: React 19.2.4 SPA (Single Page Application), built with Vite
- **Rendering**: Client-side rendered (CSR) — the `<div id="root">` is populated by JS
- **Graphics Engine**: PixiJS 8.x (used for game rendering, likely the crash game animations)
- **CDN for assets**: `https://files.crashmania.com/`
- **API Backend**: `https://api.crashmania.com/api/`
- **Crash Game Server**: `https://crash.crashmania.com/api`

---

## 2. Application Architecture

### Tech Stack
| Layer | Technology |
|-------|------------|
| Frontend Framework | React 19.2.4 |
| Build Tool | Vite |
| Rendering Engine | PixiJS 8.3.4 (WebGL/WebGPU) |
| State Management | Likely React Context/hooks (minified, hard to confirm Redux) |
| Analytics | AWS RUM (Real User Monitoring), Facebook Pixel |
| Customer Support | Intercom |
| Payments | SafeCharge/Pay.com, Smart2Pay |
| Identity/KYC | Sumsub (in.sumsub.com) |
| Auth | Google Sign-In, Facebook Login, custom email/password |

### JS Bundles
| File | Purpose |
|------|---------|
| `index-CBIll7jp.js` | Main application bundle (React, ReactDOM, app logic) |
| `FilterSystem-BTxhDZq7.js` | PixiJS rendering engine (WebGL filter system, textures, math) |
| `browserAll-tfaR-e5t.js` | Browser-specific PixiJS adapters (lazy loaded) |
| `init-CcIkbYkd.js` | PixiJS initialization (lazy loaded) |
| `webworkerAll-D7b6Ui4A.js` | Web worker support for PixiJS (lazy loaded) |

---

## 3. Lobby Page Structure (from CSS analysis)

### Layout Hierarchy
```
.main-layout-wrapper
  └── .main-content-wrapper
       └── .lobby-page
            ├── .sticky-lobby-controls          # Sticky filter/search bar
            │    └── (search, category filters, sorting)
            ├── .lobby-promotions               # Promotional banners
            ├── .games-list (× multiple)        # Horizontal game carousels
            │    ├── .games-list-header
            │    │    ├── .games-list-title      # Category name
            │    │    └── .games-list-header-btns
            │    │         ├── .view-all-btn     # "View All" button
            │    │         └── .games-list-arrow  # Scroll arrows
            │    └── .games-list-container
            │         └── .games-track           # Horizontal scroll container
            │              └── .game-card (× N)  # Individual game tiles
            │                   ├── .game-card-image-wrapper
            │                   │    └── .game-card-image
            │                   └── (game name label)
            └── .footer
```

### Game Card Sizes
| Breakpoint | Card Width |
|-----------|------------|
| Mobile (<600px) | 120px |
| Tablet (≥600px) | 160px |
| Desktop (≥834px) | 180px |

### Top 10 Cards (Special Variant)
- `.game-card.top-10-card` — has a different layout: horizontal, with a large rank number
- Margin-left: 50px (mobile), 60px (tablet+) for the rank number overlay

---

## 4. Design System

### Color Palette
| Usage | Color | Hex |
|-------|-------|-----|
| **Background (main)** | Dark blue-grey | `#282b38` |
| **Primary brand** | Purple | `#8a3dea` |
| **CTA gradient** | Blue | `#4faaff → #1c4fc7` |
| **Accent yellow** | Gold/Yellow | `#fedd24` |
| **Accent green** | Success/SC coins | `#0fd250` |
| **Error/Danger** | Red | `#ff3f3c` / `#ff6b6b` |
| **Text primary** | White | `#ffffff` |
| **Text secondary** | Muted grey | `#a3a8b7` |
| **Card/Surface** | Dark surface | `#3a4250` |
| **Footer bg** | Darker | `#1a1d24` |
| **Header bar** | Dark slate | `#485364` |
| **Black borders** | Pure black | `#000000` |

### Typography
| Font Family | Weight | Usage |
|------------|--------|-------|
| **Murecho** (Regular) | 400 | Body text |
| **MurechoSemiBold** | 600 | Labels, spans, default text |
| **MurechoBold** | 700 | Headings (h1-h6), buttons |
| **MurechoBlack** | 900 | Prices, emphasis, bold labels |
| **SairaCondensed** (Black) | 900 | Special use (likely game numbers) |

Font files are served from `/fonts/` directory as TTF.

### Key Visual Patterns
- **Skew transform**: Buttons and cards use `transform: skew(-5deg)` for dynamic angular look
- **Gradient buttons**: Primary CTAs use blue gradient `linear-gradient(#4faaff, #1c4fc7)`
- **Black borders**: Store items and cards have solid black borders
- **Shimmer loading**: Skeleton screens use a shimmer animation on placeholder items
- **Edge fade on carousels**: Linear gradients at left/right edges of scroll containers

---

## 5. Navigation & Routing

### App Routes (extracted from JS bundle)
| Route | Description |
|-------|-------------|
| `/lobby` | Main game lobby |
| `/lobby/store` | In-lobby store overlay |
| `/login` | Login page |
| `/sign-up` | Registration |
| `/account` | User account/profile |
| `/store` | Full store page |
| `/mobile-store` | Mobile-optimized store |
| `/redeem` | Prize redemption |
| `/mobile-redeem` | Mobile redemption |
| `/gifts` | Gifts/bonuses page |
| `/gifts/invite-friends` | Referral page |
| `/invite-friends` | Referral shortcut |
| `/game/:id` | Generic game page |
| `/mg` | MG (Mancala Games?) game |
| `/mg-poc` | MG proof of concept |
| `/elagame` | ELA game page |
| `/slotmill` | Slotmill game page |
| `/infingame` | Infinity game page |
| `/missions` | Missions/challenges |
| `/wheel` | Spin wheel / mystery wheel |
| `/verify` | Account verification |
| `/idensic` | KYC identity verification |
| `/something-went-wrong` | Error page |
| `/dev-bypass` | Developer bypass (debug) |
| `/privacy-policy` | Privacy policy |
| `/terms-of-service` | Terms of service |
| `/sweepstakes-policy` | Sweepstakes rules |
| `/responsible-social-play-policy` | Responsible play policy |
| `/referral-terms` | Referral T&C |

### Header Structure
- `.main-header` (height: 63px)
  - `.app-logo` (107×47px) — CrashMania logo
  - `.login-signup` — Login/Sign Up buttons (max-width: 210px)
    - `.login-btn` — Outline style
    - `.signup-btn` — Blue gradient fill

---

## 6. API Endpoints

### Base URLs
- **Main API**: `https://api.crashmania.com/api/`
- **Crash Game API**: `https://crash.crashmania.com/api`
- **File CDN**: `https://files.crashmania.com/`

### Endpoints (from JS bundle analysis)
| Endpoint | Purpose |
|----------|---------|
| `/api/lobby` | Lobby data (game categories, featured) |
| `/api/catalog` | Full game catalog |
| `/api/catalog/games` | Games listing |
| `/api/auth/` | Authentication |
| `/api/token/refresh` | Token refresh |
| `/api/stats/` | Player statistics |
| `/api/player` | Player profile data |
| `/api/stores` | Store items |
| `/api/Offers` | Promotional offers |
| `/api/promotions` | Promotions data |
| `/api/events` | Events data |
| `/api/payment` | Payment processing |
| `/api/payment-provider/current` | Current payment provider |
| `/api/transactions` | Transaction history |
| `/api/referrals` | Referral system |
| `/api/app-ratings` | App rating prompts |
| `/api/mg` | Mancala Games integration |
| `/social/login` | Social login (Google/Facebook) |
| `/social/register` | Social registration |
| `/hourly-bonus` | Hourly bonus status |
| `/hourly-bonus/claim` | Claim hourly bonus |
| `/weekly-streak-bonus` | Weekly streak bonus |
| `/weekly-streak-bonus/claim` | Claim weekly bonus |
| `/monthly-calendar-bonus` | Monthly calendar bonus |
| `/monthly-calendar-bonus/claim` | Claim monthly bonus |
| `/welcome-bonus/active` | Welcome bonus status |
| `/welcome-bonus/claim` | Claim welcome bonus |
| `/welcome-bonus/logged-in` | Welcome bonus for logged users |
| `/coinback` | Coinback rewards |
| `/coinback/claim` | Claim coinback |
| `/coinback/presented` | Mark coinback as presented |
| `/levelup-rewards/claim` | Claim level up rewards |
| `/rolling-offers` | Rolling offers data |
| `/rate_us` | Rate app prompt |
| `/coin_back` | Alternative coinback endpoint |

### WebGL Game Assets
- Crash game is loaded from: `https://files.crashmania.com/WebglBuilds/CrashManiaProd`

---

## 7. Currency System (Two-Token Model)

The platform uses a **dual currency** sweepstakes model:
1. **CC (Crash Coins)** — Play currency (yellow, `#fedd24`)
2. **SC (Sweep Coins)** — Prize currency (green, `#0fd250`)

Both are displayed in the hero section and store with distinct coin icons.

---

## 8. Game Categories & Lists

From CSS class analysis, the lobby contains multiple horizontal carousels (`.games-list`), each representing a category. Categories are dynamically loaded from the API.

### Game Card Structure
```
.game-card
  ├── .game-card-image-wrapper
  │    └── .game-card-image (thumbnail)
  └── Game Name (text below)
```

### Special Card Types
- **Top 10 cards** (`.top-10-card`): Horizontal layout with large rank numbers
- **Skeleton cards** (`.skeleton-item`): Loading placeholders with shimmer animation

---

## 9. Key UI Components

### Sticky Lobby Controls
- Position: sticky at top (below header)
- Contains: Search bar, category filter chips, sort controls
- Z-index: 4
- Background matches page bg (`#282b38`) to blend seamlessly

### Promotional Banners/Carousel
- `.lobby-promotions` section exists in CSS
- Contains promotional carousel (`.carousel`)

### Store (In-Game Overlay)
- `.game-mini-store-drawer` — Slides down from top
- Shows purchasable coin packages in horizontal scroll
- Each item: 140px wide with skew transform, purple bg (`#8a3dea`)
- Items show: Coin amount, icon, bonus amount, price at bottom

### Mystery Wheel
- Has idle spin animation and glow pulse
- Part of gifts/bonus system

### Modals
- Delete account confirmation
- Store popup overlay (with backdrop blur)
- Welcome offer animations (elaborate coin/machine entry animations)

---

## 10. Responsive Design Breakpoints

| Breakpoint | Target |
|-----------|--------|
| < 600px | Mobile (portrait) |
| ≥ 600px | Large mobile / Small tablet |
| ≥ 834px | Tablet / Small desktop |
| ≥ 1000px | Desktop |
| ≥ 1340px | Large desktop |

### Landscape Lock
On mobile landscape (≤900px width AND ≤900px height AND coarse pointer):
- Shows a "rotate phone" animation (📱 emoji rotating)
- Hides main content
- Exception: Game pages (`.allow-landscape` class)

### Content Grid
```
Mobile:       12px | content | 12px
Desktop:      minmax(50px,1fr) | minmax(auto,1020px) | minmax(50px,1fr)
Large:        minmax(50px,1fr) | minmax(auto,1200px) | minmax(50px,1fr)
```

---

## 11. Animations Catalog

| Animation | Duration | Usage |
|-----------|----------|-------|
| fadeIn/fadeOut | default | General transitions |
| slideIn/slideOut | default | Dropdown menus |
| slideInUp/slideOutDown | default | Bottom sheets |
| slideLeft/slideRight | default | Carousel transitions |
| shimmer | 2s infinite | Skeleton loading |
| spin | 1s linear infinite | Loading spinners |
| piggy-enter/exit | default | Piggy bank feature |
| piggy-swing | default | Piggy bank idle |
| lock-swing/lock-fall | default | Lock unlock animation |
| chains-disappear | default | Chain breaking effect |
| pile-drain | default | Coin pile drain |
| spill-fall | default | Coin spill effect |
| starBurst | default | Star particle burst |
| mystery-wheel-idle-spin | default | Wheel spinning |
| mystery-wheel-glow-pulse | default | Wheel glow |
| welcome-offer-* | various | Welcome offer modal entrance |
| rotatePhone | 2s ease-in-out infinite | Landscape lock indicator |

---

## 12. Game Providers (from CSS/JS)

| Provider | Integration Type |
|----------|-----------------|
| **MG (Mancala Games)** | iframe-based, fullscreen |
| **ELA Game** | iframe-based |
| **Slotmill** | iframe-based |
| **Infin Game** | iframe-based |
| **Crash (in-house)** | PixiJS WebGL, loaded from CDN |

Each provider has its own game page layout (`.mg-game-page`, `.ela-game-page`, etc.) with similar structures but slightly different styling.

---

## 13. PWA / Mobile App Features

- **manifest.json** present with standalone display mode
- Apple Touch Icons: 152px, 192px, 512px
- Android icons from 16px to 512px
- Theme color: `#ffffff`
- Safe area inset handling for notched devices
- `apple-mobile-web-app-capable: yes`

---

## 14. Assets & Image Paths

### Static Assets (from CSS references)
| Path | Content |
|------|---------|
| `/fonts/Murecho-*.ttf` | Custom font files |
| `/fonts/SairaCondensed/SairaCondensed-Black.ttf` | Condensed font |
| `/images/bg-rocket.png` | Background rocket pattern |
| `/images/gifts/gifts-pattern.png` | Gifts page background |
| `/icons/store/store-bg-pattern.png` | Store background pattern |
| `/icons/store/item-bg.png` | Store item background |
| `/icons/redeem/redeem-bg-pattern.png` | Redeem page background |
| `/icons/favicons/favicon.ico` | Favicon |
| `/Website-Icons/*.png` | Various app icons |

### Dynamic Assets (CDN)
| URL Pattern | Content |
|-------------|---------|
| `files.crashmania.com/WebglBuilds/CrashManiaProd` | WebGL game builds |
| `files.crashmania.com/...` | Game thumbnails, banners, etc. |
| `game.crashmania.com/lobby-images/...` | Lobby promotional images |
| `game.crashmania.com/lobby-icons/...` | Category/filter icons |

---

## 15. Key Takeaways for Unity Clone

### Must-Have Components
1. **App Header** with logo + Login/Signup buttons
2. **Sticky filter bar** with category chips and search
3. **Horizontal game carousels** (multiple per page, each with title + arrows)
4. **Game cards** with thumbnail images (120-180px wide)
5. **Top 10 ranked cards** (special variant)
6. **Promotional banner carousel** at top
7. **Bottom navigation** (if present on mobile)
8. **Store overlay** (slide-down drawer)

### Design Language to Replicate
- Dark theme (#282b38 background)
- Skewed elements (5-degree skew on buttons/cards)
- Murecho font family (4 weights)
- Dual-currency display (yellow CC, green SC)
- Purple accent (#8a3dea) for premium/store items
- Blue gradient CTAs
- Shimmer loading states
- Edge-fade carousels

### Data Requirements
- Game catalog with categories, thumbnails, names
- Player balance (CC + SC)
- Promotional banners
- Featured/Top 10 games list
- Store items with prices
- Bonus timers (hourly, daily, weekly, monthly)
