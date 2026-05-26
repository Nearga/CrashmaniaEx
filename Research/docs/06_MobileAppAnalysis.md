# Mobile App Decompilation & Reverse Engineering Analysis

## Executive Summary
This document logs the reverse engineering and analysis of the official Android application for **CrashMania Casino** (package name: `com.rocketdreams.crashmaniacasino`), developed by **Rocket Dreams Inc.**

By downloading and extracting the split-APK bundle, we bypassed the region restrictions and successfully analyzed its architecture, verifying the scripting backend, metadata version, and native library structures.

---

## 1. APK Acquisition & Decompression
Since the app is a sweepstakes casino, downloads are heavily geoblocked to the US and Canada on official mirrors (returning 403 Forbidden to foreign IPs). 

We bypassed this by querying a direct CDN link:
`https://d.apkpure.net/b/APK/com.rocketdreams.crashmaniacasino?version=latest`

This downloaded **`CrashMania Casino_1.1.0_APKPure.xapk`** (124 MB), which we successfully unpacked:

```text
[XAPK Unpacked Contents]
├── com.rocketdreams.crashmaniacasino.apk  <- Main APK (Resource manifest & Metadata)
├── config.arm64_v8a.apk                   <- Architecture Split (Native libraries)
├── icon.png
└── manifest.json
```

---

## 2. Scripting Backend & Metadata Extraction
After extracting the split APKs, we verified the compiled codebase structure:
1. **Scripting Backend:** The app is compiled using **Unity IL2CPP** (ARM64 architecture). It is **not** a Mono build (meaning the logic is compiled into machine instructions).
2. **Native Libraries (in `config.arm64_v8a.apk`):**
   - File: `lib/arm64-v8a/libil2cpp.so` (Standard Android ELF binary).
3. **Metadata (in `com.rocketdreams.crashmaniacasino.apk`):**
   - File: `assets/bin/Data/Managed/Metadata/global-metadata.dat`.
   - **Metadata Version:** Verified as **Version 39** (Unity 6 / 2023.3+).

---

## 3. Tooling & Decompilation Constraints
Because the Android app also utilizes **IL2CPP Metadata Version 39**, standard reverse engineering tools face key limitations:
- **`Il2CppDumper` (v6.7.46):** Fails with `System.NotSupportedException: ERROR: Metadata file supplied is not a supported version[39]` due to structural parsing checks.
- **`r2unity` (Radare2 Plugin):** Successfully parsed the metadata version 39 format. It confirmed that `global-metadata.dat` is unencrypted and fully readable. 

However, since native function pointers are stripped and optimized in compiled ARM64 ELF binaries, automated C# method body recovery is unavailable. Reconstructing the logic involves mapping the signatures to our React lobby API schemas.

---

## 4. Reconstructive Capabilities
With `libil2cpp.so` and `global-metadata.dat` extracted, we have:
1. **Complete Type Blueprints:** Exposes all class structures, method signatures, properties, and parameters.
2. **Asset Correlation:** Allows matching Unity scene prefabs and asset bundles directly with C# scripts to eliminate compiled reference errors in the Unity Editor.
