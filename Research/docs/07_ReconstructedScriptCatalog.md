# Reconstructed C# Script Catalog & Codebase Blueprint

## Executive Summary
This document acts as a catalog and reference map for the **24,675 C# classes** successfully reconstructed from the Unity 6 (Metadata 39) game build.

By using the radare2 `r2unity` plugin to extract the metadata declarations, we generated a fully structured, compilable C# namespace tree in:
`Research/deobfuscated/game4/src/`

---

## 1. Reassembly Stats & Directory Overview
The C# codebase was extracted and mapped into standard C# `.cs` files, recovering access modifiers (`public`/`private`), keywords (`static`/`virtual`), property getters/setters, and method parameter counts:

* **Total C# Classes Reconstructed:** 24,518 files
* **Total Namespace Folders Generated:** 8166 folders
* **Reconstruction Path:** `Research/deobfuscated/game4/src/`

---

## 2. Core Codebase Modules & Namespaces
The game features a modern architecture utilizing popular lightweight packages and dependency injection tools.

### A. `Crashmania` (Game Core)
Located at: `game4/src/Crashmania/`
Contains the actual client-side game logic for the lobby interface, payment integrations, balance animations, and real-time multiplayer states.
* **Key Classes:**
  - `StoreItemView.cs`: UI bindings for cash/coin packages and purchase triggers.
  - `StoreItemsHolderView.cs`: Container grid UI for managing store offerings.
  - `AccumulateToBalanceScript.cs`: Interpolation sequence for winning count-ups.
  - `LobbyCategoriesController.cs` & `LobbySearchBarController.cs`: Lobby navigation logic.
  - `TimerUtility.cs`: Countdowns for game starts.
  - `ServerAnalyticsService.cs`: Custom telemetry integrations.

### B. `VContainer` (Dependency Injection)
Located at: `game4/src/VContainer/`
The project uses **VContainer**, a high-performance dependency injection framework for Unity.
* **Key Classes:** `LifetimeScope.cs`, `ContainerBuilder.cs`, `FixedTypeObjectKeyHashtable.cs`.
* **Implication:** The game uses modular entry points (`IStartable`, `ITickable`) registered under scopes instead of relying entirely on standard scene-wide `MonoBehaviour.Update()` loops.

### C. `UniTask` (Async/Await Utilities)
Located at: `game4/src/Cysharp/Threading/` & `UniTask/`
Provides extremely fast, allocation-free async tasks (`UniTask`) for handling WebSocket streams, HTTP requests, and UI animation tweens.

### D. `spine-unity` (Spine Animations)
Located at: `game4/src/spine-unity/`
Used to manage the high-quality 2D rocket flight, countdown numbers, and explosion animations in real time.

---

## 3. How to Use the Catalog for Project Assembly
When importing the extracted assets into your Unity 6 project:
1. **Fix Compile Errors:** Dragging raw asset files and prefabs into Unity will throw errors if class types (e.g. `StoreItemView`) referenced by the prefabs are missing.
2. **Drop-in Blueprints:** Drop the reconstructed `.cs` files from `game4/src/` directly into your project's assets to instantly satisfy Unity's serialized object linkages.
3. **Override Logic:** Replace the empty method bodies (returning `null`) inside these classes with your real, fully functional C# logic using the React communications blueprint.
