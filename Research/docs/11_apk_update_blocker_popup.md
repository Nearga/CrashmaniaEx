# 11 — APK Update Blocker Popup

> Researched from: `Research/CrashMania_Casino.xapk` (v1.1.0)
> Package: `com.rocketdreams.crashmaniacasino`

---

## Overview

After bypassing the Play Store license check, the app successfully launches but presents a blocking "Update Required" popup. This popup prevents further interaction with the game and forces the user to the Play Store.

## Findings

### 1. Implementation Location
Unlike the license check, the update blocker is implemented in **Unity C# code**, which is compiled to native C++ via IL2CPP (`libil2cpp.so`). It is not present in the Java/Kotlin (Smali) layer.

Key classes found in Unity's `global-metadata.dat`:
- `AppUpdatePopup` - UI controller for the popup.
- `CheckUpdateRequired` - Logic evaluating if an update is needed.
- `SendAppVersionRequest` & `SendGetAppConfigurationsRequest` - Network calls fetching remote config.
- `EditorForceUpdate` / `editorForceUpdate` - Debug flags for testing the popup.

### 2. Network API Endpoints
The update check queries the backend API at:
- `https://api.rocketdreams.net/api/configurations/appversion?platform=android`
- `https://api.rocketdreams.net/api/configurations`

### 3. Server Response Payload
Directly probing the `appversion` endpoint reveals the version constraints set by the backend:
```json
{
    "platform": "android",
    "minRequiredVersion": "1.1.1",
    "minRecommendedVersion": "1.1.1",
    "minRecommendedVersionPersistent": "1.1.1",
    "isShowRedirectButton": false,
    "showCrashCoinsRedeemScreenBlocker": false,
    "isShowRepeated": true,
    "isUnderMaintenance": false
}
```

### 4. The Mechanism
1. The Unity app queries the Android OS (`PackageManager`) to get its current `versionName`.
2. It makes an HTTP GET request to `/api/configurations/appversion`.
3. It compares its local version against `minRequiredVersion`.
4. Since the extracted APK has version `1.1.0` and the server demands `1.1.1`, the `AppUpdatePopup` is triggered.

## Bypass Strategy

Since the logic is compiled natively into `libil2cpp.so`, patching it directly via assembly is difficult and brittle. Instead, we can spoof the app's version:

1. **Modify `apktool.yml` (and `AndroidManifest.xml`)**:
   Bump `versionName` to `1.1.1` (or higher) and `versionCode` to `1194`.
2. When the Unity app asks Android for its version, it receives the spoofed version.
3. The comparison `localVersion >= minRequiredVersion` evaluates to true, bypassing the popup natively.

## Alternative: Network Interception
For deeper analysis or dynamic overriding, we set up `mitmproxy` on the host machine and configured the Android device to route HTTP/HTTPS traffic through it. This allows us to inspect raw backend communication, rewrite JSON responses on the fly, and understand the expected emulated game version.
