# CrashMania Color Palette Reference

## Primary Colors

### Background Colors
```
Main Background:     #282b38  (dark blue-grey)
Card/Surface:        #3a4250  (lighter dark)
Footer:              #1a1d24  (darkest)
Header Bar:          #485364  (slate)
Store Item BG:       #2a2a2a  (skeleton/loading)
```

### Brand Colors
```
Primary Purple:      #8a3dea
CTA Blue Start:      #4faaff
CTA Blue End:        #1c4fc7
Accent Yellow/Gold:  #fedd24
Accent Green:        #0fd250
```

### Status Colors
```
Error/Danger Red:    #ff3f3c
Error Light:         #ff6b6b
Error BG:            #ff4a4a1f (10% opacity)
Error Border:        #ff4a4a59 (35% opacity)
Info Blue:           #6ec6ff
Success Green:       #0fd250
Spinner Teal:        #4ecdc4
```

### Text Colors
```
Primary Text:        #ffffff
Secondary Text:      #a3a8b7
Muted Text:          #ffffff99 (60% white)
Very Muted:          #ffffff66 (40% white)
Subtle:              #ffffffb3 (70% white)
Disabled Text:       #00000080 (50% black)
```

### Border & Shadow
```
Black Border:        #000000
White Inset Shadow:  #ffffff42 (26% white)
Black Shadow:        #00000029 (16% black)
Dark Shadow:         #00000080 (50% black)
Backdrop Blur BG:    #000000b3 (70% black)
Scrollbar Track:     #eeeeee70 (44% light)
Scrollbar Thumb:     #a3a8b7
```

## Unity Color Reference (RGB float values)

```csharp
// Main Background
public static readonly Color BG_MAIN = new Color(0.157f, 0.169f, 0.220f);        // #282b38
public static readonly Color BG_CARD = new Color(0.227f, 0.259f, 0.314f);        // #3a4250
public static readonly Color BG_FOOTER = new Color(0.102f, 0.114f, 0.141f);      // #1a1d24
public static readonly Color BG_HEADER = new Color(0.282f, 0.325f, 0.392f);      // #485364

// Brand
public static readonly Color BRAND_PURPLE = new Color(0.541f, 0.239f, 0.918f);   // #8a3dea
public static readonly Color CTA_BLUE_TOP = new Color(0.310f, 0.667f, 1.000f);   // #4faaff
public static readonly Color CTA_BLUE_BTM = new Color(0.110f, 0.310f, 0.780f);   // #1c4fc7
public static readonly Color ACCENT_YELLOW = new Color(0.996f, 0.867f, 0.141f);  // #fedd24
public static readonly Color ACCENT_GREEN = new Color(0.059f, 0.824f, 0.314f);   // #0fd250

// Status
public static readonly Color ERROR_RED = new Color(1.000f, 0.247f, 0.235f);      // #ff3f3c
public static readonly Color ERROR_LIGHT = new Color(1.000f, 0.420f, 0.420f);    // #ff6b6b
public static readonly Color INFO_BLUE = new Color(0.431f, 0.776f, 1.000f);      // #6ec6ff

// Text
public static readonly Color TEXT_PRIMARY = Color.white;
public static readonly Color TEXT_SECONDARY = new Color(0.639f, 0.659f, 0.718f);  // #a3a8b7
```

## Gradient Definitions

### Primary CTA Button
```
Direction: Top to Bottom
Start: #4faaff (Light Blue)
End:   #1c4fc7 (Dark Blue)
```

### Welcome Offer Sign Background
```
Social login gradient: 45deg
#f09433 → #e6683c → #dc2743 → #cc2366 → #bc1888
(Instagram-style gradient)
```

### Edge Fade (Carousel)
```
Left:  linear-gradient(90deg, #282b38, transparent)
Right: linear-gradient(270deg, #282b38, transparent)
Width: 40px (mobile), 60px (desktop)
```
