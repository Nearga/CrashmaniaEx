# Phase 7 Game Reference Map

Visual source of truth: `Research/app_patched/screenshots/Screenshots/3 Crash game/*.png`.
Canonical screenshot size: `720 x 1600`.
Unity reference canvas: `1170 x 2532` with width match `0.0`.
Scale used for layout conversion: `x = 1170 / 720 = 1.625`, `y = 2532 / 1600 = 1.5825`.

## Measured Vertical Bands

The four screenshots share stable major regions even though round state colors differ.

| Region | Screenshot rect | Canvas rect | Notes |
| --- | --- | --- | --- |
| Top safe/status area | `0,0,720,96` | `0,0,1170,152` | mostly black/status overlay |
| Game header / balances | `12,96,696,174` | `20,152,1131,275` | blue top bar, back, level, balances, toggle/menu |
| Flight viewport | `12,270,696,510` | `20,427,1131,807` | dark/blue/gray composite, round history at top, multiplier/rocket center |
| Active bets table | `12,780,696,140` | `20,1234,1131,222` | player/bet/multi/win header and rows |
| Bet panel A | `12,930,696,222` | `20,1472,1131,351` | first controls/action panel |
| Bet panel B | `12,1160,696,222` | `20,1836,1131,351` | second controls/action panel |
| Bottom/safe area | `0,1388,720,212` | `0,2197,1170,335` | dark/brown app footer and black bottom safe band |

## Color Anchors

Average row samples from the references:
- Header rows around y=160-240: `#3B5E8D`, `#396496`.
- Flight body around y=400-560: `#3C4554`, `#3D6593` depending on round state.
- Active/bet region around y=800-1280: blue, cyan, or purple depending on game state, but never a large flat `#080913` void.
- Bottom safe rows y>=1520: black.

## Recovery Rules

- Large surfaces should be solid/tinted panels and simple layered bands, not stretched atlas sprites.
- Extracted sprites are allowed for small controls/icons and the rocket only when aspect-preserved.
- `Crash_mode_BG_default` is not used as the full viewport background in this recovery pass.
- `rocket-start 1` is not used as a large explosion graphic.
- Runtime gameplay bindings remain unchanged; only scene/prefab layout and visual surfaces are adjusted.
