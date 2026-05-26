1. Download raw assets and available code "https://game.crashmania.com/lobby" to /Research/raw
2. Run deobfuscation script: ./deobfuscate.sh

3. Find and download unity game "https://game.crashmania.com/game/1002" 
4. Unpack it with AssetRipper (./AssetRipper.GUI.Free)

5. Install Unity 6000.3.10f1

6. While decompilers like Il2CppDumper and Cpp2IL are completely stuck on Unity 6 (Metadata 39), there is a specialized reverse-engineering tool named r2unity (built for the radare2 framework) which has native support for parsing Metadata Version 39.
By compiling r2unity as a native plugin directly inside the radare2 framework, we completely bypassed the Unity 6/Metadata 39 version blocks. 

7. Downloaded and decompiled APK

8. Tried to make screenshots automatically for layout - failed - will get back to it later, if necessary

9. Cloned my previous test task i'm proud of - LastOneOut. Summarized strong sides in Sample/Unity-last-one-out/new features.md

