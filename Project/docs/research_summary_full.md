# Research Phase Summary

Early work focused on downloading and analyzing three versions of CrashMania. Each source answered a different part of the reconstruction problem.

---

## 1. Web App Lobby

**Source:** `game.crashmania.com`

### What we recovered
* React/Vite lobby HTML, CSS, manifest, and JavaScript bundles.
* **318 static assets including:**
    * Lobby promotions and banners.
    * Login/homepage artwork.
    * Game thumbnails.
    * Navigation, store, gift, bonus, and account icons.
    * Murecho and Saira Condensed fonts.
* Authenticated lobby, store, account, gifts, and crash-game screenshots.
* Routes, API endpoints, catalog structures, currencies, responsive breakpoints, animations, and UI component specifications.
* React-to-Unity WebGL communication messages and WebSocket event shapes.
* Game catalog JSON and MultiGame catalog responses.

### How we used it
* Defined the application design language: colors, fonts, spacing, skewed buttons, cards, carousels, and modals.
* Created the login and lobby specifications.
* Used web promotion assets where the Unity export had no equivalent.
* Built mock catalog, profile, balance, store, bonus, and API models.
* Used the discovered message contracts to understand betting, cashout, game lifecycle, and balance updates.

**Key references:**
* [Lobby analysis](../../Research/docs/01_LobbyAnalysis.md)
* [UI components](../../Research/docs/04_UIComponents.md)
* [API endpoints](../../Research/docs/05_APIEndpoints.md)

---

## 2. Unity WebGL Application

**Source:** CrashMania WebGL build loaded by the website.

### What we recovered
* Raw WebGL build files: data, framework JavaScript, WASM, loader, and Unity data archives.
* **An AssetRipper-exported Unity project containing:**
    * Original Boot, Login, Lobby, Game, and Ftue scenes.
    * Roughly 6,900 exported assets.
    * Sprites, texture atlases, fonts, audio, materials, animations, meshes, and controllers.
    * Original script/type names and serialized component relationships.
* **Dependency and architecture clues:**
    * VContainer
    * UniTask
    * DOTween
    * Spine
    * Addressables
    * Native WebSocket
* Crash-game structure, dual-bet controls, round events, rocket assets, balance components, and game header art.

### How we used it
* **Copied selected original sprites into the active project for:**
    * Header and bottom navigation.
    * Game cards and slot thumbnails.
    * Crash-game header, bet controls, history pills, rocket, explosion, and backgrounds.
* Used recovered scenes and component names as structural evidence.
* Reconstructed the crash-game loop, betting state machine, multiplier curve, and local mock service.
* Used discovered libraries and class names as architectural guidance.

### Limitations
* The WebGL build used IL2CPP compiled to WASM with metadata version 39.
* AssetRipper recovered assets and type structure, but not reliable original C# method bodies.
* Some Spine animation data and runtime-generated game debris could not be recovered, so we built native visual fallbacks.

**Key references:**
* [Unity game flow](../../Research/docs/08_UnityGameFlow.md)

---

## 3. Android APK/XAPK

**Source:** Official CrashMania Casino Android split-APK bundle.

### What we recovered
* Original XAPK containing the base APK and ARM64 split.
* Android manifest, resources, native libraries, and Unity metadata.
* `libil2cpp.so` and unencrypted metadata version 39.
* More than 24,500 reconstructed C# type-outline files from metadata.
* **Mobile-specific startup behavior:**
    * Google Play Pairip licensing.
    * Root detection.
    * Version/update blocker flow.
    * Backend configuration endpoints.
* **29 screenshots covering:**
    * Login and signup.
    * Lobby.
    * Crash game.
    * Machine games.
* A patched APK that bypassed the Play licensing check and update blocker sufficiently for visual investigation.

### How we used it
* Treated APK screenshots as the primary mobile visual reference.
* Matched login, popup, lobby, header, navigation, and Game scene proportions against the running Android app.
* Used recovered class names and signatures to understand the original mobile architecture and feature set.
* Confirmed that the original application was a Unity 6 IL2CPP app.
* Identified mobile-only behavior that was not visible from the web lobby.

### Limitations
* Native ARM64 IL2CPP method bodies were stripped and optimized.
* The reconstructed C# catalog is primarily a structural blueprint, not original working source code.

**Key references:**
* [Mobile app analysis](../../Research/docs/06_MobileAppAnalysis.md)
* [Reconstructed script catalog](../../Research/docs/07_ReconstructedScriptCatalog.md)
* [APK licensing flow](../../Research/docs/10_apk_licensing_flow.md)

---

## Overall Approach

The reconstruction combined all three sources:
1. **Web lobby:** product behavior, APIs, design system, and reusable web artwork.
2. **Unity WebGL build:** original Unity assets, scenes, component structure, and game contracts.
3. **APK:** mobile source of truth, native behavior, screenshots, and type metadata.

`Research/` remains immutable source evidence. Selected assets and findings were copied or reimplemented cleanly inside `Project/`, rather than attempting to run the extracted projects directly.