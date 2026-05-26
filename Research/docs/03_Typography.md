# CrashMania Typography Reference

## Font Families

### Murecho (Primary Font)
- **Source**: Custom TTF files served from `/fonts/`
- **License**: Google Fonts (Open Source)
- **Download**: https://fonts.google.com/specimen/Murecho

| Variant | File | CSS Name | Weight | Usage |
|---------|------|----------|--------|-------|
| Regular | `Murecho-Regular.ttf` | `Murecho` | 400 | Body text, paragraphs |
| SemiBold | `Murecho-SemiBold.ttf` | `MurechoSemiBold` | 600 | Default for div, p, label, a, span |
| Bold | `Murecho-Bold.ttf` | `MurechoBold` | 700 | All headings (h1-h6), buttons, inputs |
| Black | `Murecho-Black.ttf` | `MurechoBlack` | 900 | Prices, emphasis, store values, CTAs |

### SairaCondensed (Secondary Font)
- **Source**: `/fonts/SairaCondensed/SairaCondensed-Black.ttf`
- **Download**: https://fonts.google.com/specimen/Saira+Condensed
- **Usage**: Special display text (likely rank numbers in Top 10)

## Font Size Scale

### Heading Sizes
```
h1: 2rem (32px)
h2: 1.625rem (26px)
h3: 1.25rem (20px)
```

### Component-Specific Sizes
| Element | Size | Font |
|---------|------|------|
| Hero H1 (mobile) | clamp(3rem, 4vw + 1rem, 3.125rem) | MurechoBold |
| Hero H1 (desktop) | clamp(3.125rem, 5vw + 2rem, 5.5rem) | MurechoBold |
| Hero H3 (mobile) | clamp(1.5rem, 2.5vw + .5rem, 1.6875rem) | MurechoBold |
| Store Title | 2.875rem (46px) | MurechoBlack |
| Store Subtitle | 1.125rem (18px) | MurechoBlack |
| Store Item Price | 0.875rem (14px) | MurechoBlack |
| Store Item Coins | 1.125rem (18px) | MurechoBlack |
| Game List Title | 16px | MurechoBold (uppercase) |
| Game Card Label | 0.875rem (14px) | MurechoSemiBold |
| Login/Signup Btn | clamp(.8rem, 1.5vw + .5rem, 1.125rem) | MurechoBlack |
| View All Button | 14px | MurechoSemiBold |
| Body text default | 1rem (16px) | MurechoSemiBold |
| Maintenance heading | clamp(1.8rem, 3vw, 2.5rem) | MurechoBlack |

## Text Styling

### Outline Text Effect
Used for prominent display text (hero section, game titles):
```css
.outline-text {
  --text-color: white;
  --outline-color: black;
  -webkit-text-stroke: var(--outline-size) var(--outline-color);
  paint-order: stroke fill;
}
```
This creates a 3D layered text effect with 3 stacked text layers at slight offsets.

### Text Transform
- **Headings**: Often `text-transform: uppercase`
- **Buttons**: Always `text-transform: uppercase`
- **Letter spacing**: `-0.3px` on MurechoBlack spans, `0.5px` on buttons

## Unity Font Setup Notes

1. Download **Murecho** from Google Fonts in all 4 weights
2. Download **Saira Condensed** Black weight
3. Import as TextMeshPro font assets with appropriate SDF settings
4. Create TMP font asset variants for each weight
5. Set default font to MurechoSemiBold
6. Use font weight switching for different text styles

### Recommended TextMeshPro Material Presets
- **Default**: MurechoSemiBold, white, no outline
- **Heading**: MurechoBold, white, no outline
- **Price/Value**: MurechoBlack, white, with black outline (via TMP outline)
- **Button**: MurechoBlack, white, uppercase
