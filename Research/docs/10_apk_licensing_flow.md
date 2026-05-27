# 10 — APK Licensing Flow (CrashMania Casino)

> Researched from: `Research/CrashMania_Casino.xapk` (v1.1.0, versionCode 1193, targetSdk 35)
> Package: `com.rocketdreams.crashmaniacasino`

---

## Overview

The app uses **Google's pairip License Check SDK** (a newer replacement for the classic
Android License Verification Library / LVL). It enforces that the app was installed
from the Google Play Store by communicating with the Play Store service at runtime.
Sideloaded installs (via `adb install`) receive a `NOT_LICENSED` response and are
blocked with a paywall or error dialog, then force-closed.

---

## Components

### 1. `LicenseContentProvider` (auto-init trigger)

```
com.pairip.licensecheck.LicenseContentProvider
```

- Declared in `AndroidManifest.xml` as a `<provider>` — Android instantiates all
  ContentProviders automatically when the app process starts, **before** `Application.onCreate()`.
- Its `onCreate()` method creates a `LicenseClient` and calls `initializeLicenseCheck()`.
- This is the **entry point** — the license check fires on every cold start, invisibly.

Manifest registration:
```xml
<provider
    android:authorities="com.rocketdreams.crashmaniacasino.com.pairip.licensecheck.LicenseContentProvider"
    android:exported="false"
    android:name="com.pairip.licensecheck.LicenseContentProvider"/>
```

---

### 2. `LicenseClient` (IPC with Google Play)

```
com.pairip.licensecheck.LicenseClient
com.pairip.licensecheck.ILicenseV2ResultListener      (AIDL interface)
com.pairip.licensecheck.ILicenseV2ResultListener$Stub (binder stub)
```

- Binds to `com.android.vending` (Google Play Store) via `ILicensingService`.
- Sends the app's package name + signing certificate hash for server-side validation.
- Receives a signed response blob (LICENSED / NOT_LICENSED / ERROR).
- On failure: launches `LicenseActivity` with appropriate `ActivityType`.

Relevant permission required (declared in manifest):
```
com.android.vending.CHECK_LICENSE
com.google.android.finsky.permission.BIND_GET_INSTALL_REFERRER_SERVICE
```

---

### 3. `LicenseActivity` (paywall / error UI)

```
com.pairip.licensecheck.LicenseActivity
```

Declared in manifest as `android:exported="false"` (only launchable by the app itself):
```xml
<activity android:exported="false" android:name="com.pairip.licensecheck.LicenseActivity"/>
```

Receives an `Intent` extra `"activitytype"` of enum `ActivityType`:

| Ordinal | Name      | Behaviour |
|---------|-----------|-----------|
| `0`     | `PAYWALL` | Extracts `"paywallintent"` (a `PendingIntent`) from the Intent, launches it (redirects to Play Store), then calls `System.exit(0)` |
| `1`     | `ERROR`   | Shows an `AlertDialog` with message "Check that Google Play is enabled…", then calls `System.exit(0)` |

Both paths terminate the process via `System.exit(0)` after closing the activity.

---

### 4. Root Detection (`scottyab/rootbeer`)

```
com.scottyab.rootbeer.*  (smali_classes4)
```

The app also ships **RootBeer** — a popular root detection library. It checks:
- presence of `su` binary in common paths
- test-keys build tag
- dangerous props (`ro.debuggable`, etc.)
- installed root management apps

This is a secondary check, independent of the pairip license flow.

---

## Startup Call Chain

```
App process start
  └── Android framework auto-inits ContentProviders
        └── LicenseContentProvider.onCreate()
              └── new LicenseClient(context)
                    └── LicenseClient.initializeLicenseCheck()
                          └── bindService(com.android.vending / ILicensingService)
                                └── async IPC response via ILicenseV2ResultListener
                                      ├─ LICENSED       → do nothing, app runs normally
                                      ├─ NOT_LICENSED   → startActivity(LicenseActivity, PAYWALL)
                                      └─ ERROR          → startActivity(LicenseActivity, ERROR)
```

---

## Manifest Stamps (Play Integrity markers)

These meta-data entries are injected by Play's APK build pipeline and act as
additional signals the SDK checks:

```xml
<meta-data android:name="com.android.vending.splits.required" android:value="true"/>
<meta-data android:name="com.android.stamp.source" android:value="https://play.google.com/store"/>
```

---

## Bypass Attempts & Results

### Attempt 1 — `adb install-multiple --installer-package com.android.vending`
- **Result**: `IllegalArgumentException: Unknown option --installer-package`
- Android 14 (API 34) removed this flag.

### Attempt 2 — `adb shell pm set-installer … com.android.vending`
- **Result**: `SecurityException: Caller does not have same cert as new installer package`
- Requires matching certificate signature of the Play Store — not possible without root.

### Attempt 3 — Smali patch (applied ✅)

Two files patched in `Research/app_patched/decompiled/`:

#### `smali/com/pairip/licensecheck/LicenseContentProvider.smali`
`onCreate()` patched to **skip** `LicenseClient` creation and return `true` immediately.
The license check never fires.

```smali
# BEFORE
new-instance v0, Lcom/pairip/licensecheck/LicenseClient;
invoke-direct {v0, v1}, Lcom/pairip/licensecheck/LicenseClient;-><init>(Landroid/content/Context;)V
invoke-virtual {v0}, Lcom/pairip/licensecheck/LicenseClient;->initializeLicenseCheck()V

# AFTER (removed — returns true immediately)
const/4 v0, 0x1
return v0
```

#### `smali/com/pairip/licensecheck/LicenseActivity.smali`
`onStart()` patched to **return-void** immediately after `super.onStart()`.
Even if `LicenseActivity` is somehow launched, it does nothing — no dialog, no `System.exit()`.

```smali
# BEFORE — reads ActivityType intent extra, branches to showErrorDialog() or showPaywallAndCloseApp()
# AFTER
invoke-super {p0}, Landroid/app/Activity;->onStart()V
return-void
```

---

## Files

| File | Role |
|------|------|
| `Research/CrashMania_Casino.xapk` | Original XAPK (source) |
| `/tmp/xapk_extract/com.rocketdreams.crashmaniacasino.apk` | Extracted base APK |
| `/tmp/xapk_extract/config.arm64_v8a.apk` | Extracted arm64 split APK |
| `Research/app_patched/decompiled/` | apktool-decompiled + patched smali tree |

---

## Next Steps

- [ ] Rebuild APK with `apktool b`
- [ ] Sign with a debug key (`keytool` + `apksigner`)
- [ ] Reinstall: `adb install-multiple -r <base.apk> <config.arm64_v8a.apk>`
- [ ] Verify the app launches past the license gate
- [ ] Investigate RootBeer checks if further blocking occurs
