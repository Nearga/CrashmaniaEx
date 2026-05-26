# CrashmaniaEx Project Overview

This project is a comprehensive research and reconstruction effort of the "Crashmania" platform, focusing on both the web lobby and the Unity-based "Crash" game.

## Project Structure

- **Research/**: Contains all research data, raw assets, and deobfuscated code.
    - **raw/**: Original assets downloaded from the live site and game servers.
    - **deobfuscated/**: Results of deobfuscation and decompilation efforts.
        - **game/**: Decompiled Unity project (AssetRipper export).
            - **Assets/**: Extracted Unity assets (Textures, Prefabs, Scenes, etc.).
            - **unity/ExportedProject/**: The reconstructed Unity project structure.
                - **Assets/Scripts/**: Decompiled C# scripts (Note: Many are currently "dummy" classes due to decompilation limitations).
    - **scripts/**: Utility scripts for downloading assets, fetching API data, and deobfuscating code.
    - **tools/**: Third-party tools used for research (e.g., AssetRipper).

## Key Components

### 1. Web Lobby (Research/01_LobbyAnalysis.md)
- **Tech Stack**: React 19, Vite, PixiJS 8.
- **Architecture**: Single Page Application (SPA) with client-side rendering.
- **API**: Communicates with `https://api.crashmania.com/api/`.

### 2. Unity Crash Game (Research/deobfuscated/game/)
- **Technology**: Unity (WebGL/Mobile).
- **Scenes**: `Boot`, `Login`, `Lobby`, `Game`, `Ftue`.
- **Key Scripts**: Located in `Assets/Scripts/Crashmania/`.
    - `CrashGameLoader.cs`: Responsible for loading the core game.
    - `AppManager.cs`: Main application lifecycle management.
    - `LobbyManager.cs`: Manages the in-game lobby state.
- **Assets**:
    - **Textures**: `GameThumbnails.png`, `Main.png`, `Avatars.png`.
    - **Prefabs**: `LobbyGameCategoryView`, `GameMultiplierHolder`.

### 3. API & Endpoints (Research/05_APIEndpoints.md)
- **Base URLs**:
    - Main API: `https://api.crashmania.com/api/`
    - Crash Game API: `https://crash.crashmania.com/api`
- **Features**: Authentication, Lobby/Catalog, Player Stats, Store/Payments, Bonuses/Rewards.

## Development & Research Workflow

### Downloading Assets
Use scripts in `Research/scripts/`:
- `01_download_assets.sh`: Downloads lobby assets.
- `05_fetch_game_info.js`: Fetches metadata for games.

### Deobfuscation
- Use `Research/scripts/03_deobfuscate.sh` to process JS bundles.
- Use `AssetRipper` (in `Research/tools/`) to unpack Unity data.

### Reconstructing Scripts
The decompiled scripts in `Research/deobfuscated/game/unity/ExportedProject/Assets/Scripts/Crashmania/` are currently mostly placeholders. Reconstruction requires manual analysis of the deobfuscated JS bundles or original assemblies if available.

## Design System (Research/02_ColorPalette.md, Research/03_Typography.md)
- **Primary Font**: Murecho.
- **Colors**: Dark blue-grey background (`#282b38`), Purple brand color (`#8a3dea`).
