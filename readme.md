## DAY 1 

1. Download raw assets and available code "https://game.crashmania.com/lobby" to /Research/raw
2. Run deobfuscation script: ./deobfuscate.sh

3. Find and download unity game "https://game.crashmania.com/game/1002" 
4. Unpack it with AssetRipper (./AssetRipper.GUI.Free)

5. Install Unity 6000.3.10f1

6. While decompilers like Il2CppDumper and Cpp2IL are completely stuck on Unity 6 (Metadata 39), there is a specialized reverse-engineering tool named r2unity (built for the radare2 framework) which has native support for parsing Metadata Version 39.
By compiling r2unity as a native plugin directly inside the radare2 framework, we completely bypassed the Unity 6/Metadata 39 version blocks. 

7. Downloaded and decompiled xAPK. Tried to install it on locall device. Override the license, run the app, stuck on "update required" screen

8. Tried to make screenshots automatically for layout - failed - will get back to it later, if necessary

9. Created Research/docs with a proper description of existing index and the game: 
Research/docs/01_LobbyAnalysis.md, 
Research/docs/02_ColorPalette.md, 
Research/docs/03_Typography.md, 
Research/docs/04_UIComponents.md, 
Research/docs/05_APIEndpoints.md, 
Research/docs/06_MobileAppAnalysis.md, 
Research/docs/07_ReconstructedScriptCatalog.md, 
Research/docs/08_UnityGameFlow.md,  
Research/docs/09_WebSocketRealtimeMath.md

10. Cloned my previous test task i'm proud of - LastOneOut. Summarized strong sides in Sample/Unity-last-one-out/new features.md

11. Created a spec for the new project: 
/Project/spec_master.md, 
/Project/spec_mobile_lobby.md, 
/Project/spec_backend.md, 
/Project/spec_game.md, 
/Project/spec_web_lobby.md 


## DAY 2

12. Run Crashmania on phone. Override license. Override the "update required" screen, pass the registration (bugged), now on the lobby
Screenshots: Research/app_patched/screenshots/Screenshots 

13. WIP on Lobby. Lots of supporting infrastructure (specs, docs, carousel, prefabs, scene hierarchy rules)

## DAY 3

14. Lobby is finished, buttons are interactive, sroller. Similar 95% to original

15. 