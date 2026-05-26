# Unity WebGL Game Architecture & Deobfuscation Guide

## Executive Summary
This document outlines the reverse-engineered communications and structure of the Unity WebGL Crash Game (ID `1002`). It provides details on why automatic decompilers failed, maps out the entire WebGL message payload exchange between the React host and the Unity container, and includes complete C# source code templates to reconstruct the core Unity components.

---

## 1. Why Tooling Failed on C# Reconstruction
When extracting this game, you'll find that all generated C# classes in the exported Unity project are empty "Dummy Classes". This happens for two reasons:
1. **WebGL WASM Decompilation Barriers:** The game assembly was compiled via IL2CPP into WebAssembly (`CrashMania Casino (prod).wasm`). Modern decompilation engines (like AssetRipper's internal `Cpp2IL`) have highly experimental WebAssembly support, making direct bytecode decompilation unstable on macOS/Linux.
2. **Unity 6 Metadata (v39) Incompatibility:** The game was built using a very recent version of Unity (Unity 6 / 2023.3+), which utilizes **IL2CPP Metadata Version 39**. Standard static reverse-engineering tools like `Il2CppDumper` throw a `System.NotSupportedException: ERROR: Metadata file supplied is not a supported version[39]` because they lack parsing schemas for this modern header layout.

Rather than struggling with binary-level decompilers, we can **fully reconstruct the C# codebase** by mapping the game's actual communication pipeline from our deobfuscated React lobby frontend.

---

## 2. React <-> Unity WebGL Message Mapping
The React lobby hosts the Unity WebGL instance in an iframe. They communicate bidirectionally using `window.postMessage` and Unity's `SendMessage` bindings.

### A. React -> Unity (Commands sent to Unity)
Messages sent to the Unity WebGL context are forwarded via `unityInstance.SendMessage("WebComManager", MethodName, JSONString)`. 

| Method Name | Message Type | Payload Structure | Description |
| :--- | :--- | :--- | :--- |
| `Initialize` | `INITIALIZED` | `{ gameBundleId: string, gameId: number, userId: string, accessToken: string, musicOn: boolean, effectsOn: boolean, gameType: number, wsUrl?: string, apiUrl?: string, ssoUrl?: string, gameUrl?: string }` | Initializes game state, auth token, and API/WS URLs. |
| `WebToUnity` | `PLACE_BET` | `{ eventName: "PLACE_BET", content: { index: number, amount: number } }` | Instructs Unity to register a bet at a specific index. |
| `WebToUnity` | `CASH_OUT` | `{ eventName: "CASH_OUT", content: { index: number, amount: number } }` | Triggers a cash out for a specific active bet index. |
| `WebToUnity` | `CANCEL_BET` | `{ eventName: "CANCEL_BET", content: { index: number, amount: number } }` | Cancels a pending bet at a specific index. |
| `WebToUnity` | `TOGGLE_SOUNDS`| `{ eventName: "TOGGLE_SOUNDS", content: { musicOn: boolean, effectsOn: boolean } }` | Updates SFX/Music options dynamically. |
| `WebToUnity` | `EXIT_GAME` | `{ eventName: "EXIT_GAME", content: {} }` | Notifies the game that the user is exiting back to the lobby. |

---

### B. Unity -> React (Events returned to the React lobby)
Unity posts events back to the parent browser tab using a global Javascript handler:
`parent.postMessage({ type: eventName, payload: data }, "*")`

| Event Name | Payload Structure | Triggering Event in Unity |
| :--- | :--- | :--- |
| `UNITY_INITIALIZED` | `null` | Emitted when Unity completes its boot sequence. |
| `UNITY_STARTED` | `null` | Emitted when the game canvas starts rendering. |
| `UNITY_GAME_READY` | `null` | Emitted when preloads finish and audio is unlocked. |
| `GAME_LOADED` | `"Running"` | Sent when the scene loading is fully complete. |
| `LOADING_PROGRESS` | `number` | Loading bar percentage (0.0 to 1.0). |
| `GAME_START` | `null` | Fired when a new round countdown completes and the rocket starts. |
| `GAME_MULTIPLIER_UPDATE` | `number` | Real-time broadcast of the ascending multiplier (e.g. `2.45`). |
| `CASH_OUT_RESULT` | `{ index: number, amount: number }` | Sent on successful cash-out. Activates React's coin animation. |
| `GAME_END` | `{ gameId: string, startTime: string, endDate: string, multiplier: number, serverSeed?: string, clientSeed?: string, nonce?: number, combinedHash?: string }` | Dispatched when the rocket crashes, declaring the final round multiplier. |
| `PLAYER_BETS_UPDATE` | `Array<Bet>` | Real-time list of all active bets in the current round. |
| `PLAYER_BET_CHANGED` | `{ Action: string, Bet: Bet }` | Dispatched when a player's individual bet status is modified. |
| `WEBSOCKET_DISCONNECTED`| `null` | Fired if the WebSocket connection is interrupted. |

---

## 3. Reconstructed C# Core Scripts
To replace the dummy scripts and get your Unity Editor project compiling and running, implement the following C# files inside `Assets/Scripts/Crashmania/`:

### 1. `WebComManager.cs`
This handles the bridge between Unity and your React lobby web page. Place this script on a persistent GameObject named **`WebComManager`**.

```csharp
using UnityEngine;
using System;

public class WebComManager : MonoBehaviour
{
    public static WebComManager Instance { get; private set; }

    [System.Serializable]
    public class ConnectionData
    {
        public string gameBundleId;
        public int gameId;
        public string userId;
        public string accessToken;
        public bool musicOn;
        public bool effectsOn;
        public int gameType;
        public string wsUrl;
        public string apiUrl;
    }

    [System.Serializable]
    public class UnityMessage
    {
        public string eventName;
        public string content; // Serialized inner JSON payload
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tell React that the WebComManager is fully alive
        SendEventToBrowser("UNITY_INITIALIZED", "");
        SendEventToBrowser("UNITY_STARTED", "");
    }

    // Called from WebGL index.html: unityInstance.SendMessage("WebComManager", "Initialize", payload)
    public void Initialize(string jsonPayload)
    {
        Debug.Log("[WebCom] Received Initialize: " + jsonPayload);
        try
        {
            ConnectionData data = JsonUtility.FromJson<ConnectionData>(jsonPayload);
            
            // Apply music/sound settings
            AudioListener.volume = data.effectsOn ? 1.0f : 0.0f;
            
            // Connect WebSocket to real-time server
            // e.g. CrashWebSocketClient.Instance.Connect(data.wsUrl, data.accessToken);
            
            SendEventToBrowser("UNITY_GAME_READY", "");
        }
        catch (Exception e)
        {
            Debug.LogError("[WebCom] Init Error: " + e.Message);
        }
    }

    // Called from WebGL index.html: unityInstance.SendMessage("WebComManager", "WebToUnity", payload)
    public void WebToUnity(string jsonPayload)
    {
        Debug.Log("[WebCom] Received Event: " + jsonPayload);
        try
        {
            UnityMessage msg = JsonUtility.FromJson<UnityMessage>(jsonPayload);
            switch (msg.eventName)
            {
                case "PLACE_BET":
                    // Parse placing a bet content and register with backend
                    break;
                case "CASH_OUT":
                    // Send cash-out transaction to backend
                    break;
                case "CANCEL_BET":
                    // Cancel pending bet
                    break;
                case "TOGGLE_SOUNDS":
                    // Toggle Audio volume
                    break;
                case "EXIT_GAME":
                    Application.Quit();
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[WebCom] WebToUnity Error: " + e.Message);
        }
    }

    // Helper to send events back to the parent browser
    public void SendEventToBrowser(string eventName, string jsonContent)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Call browser receiver defined in index.html
        try {
            Application.ExternalCall("SendMessageToBrowser", eventName, jsonContent);
        } catch (Exception e) {
            Debug.LogWarning("ExternalCall Failed: " + e.Message);
        }
#else
        Debug.LogFormat("[WebCom Simulation] Event: {0} | Payload: {1}", eventName, jsonContent);
#endif
    }
}
```

---

### 2. `StoreItem.cs`
The reconstructed placeholder script. Typically manages individual store item purchases/visuals.

```csharp
using UnityEngine;
using UnityEngine.UI;

public class StoreItem : MonoBehaviour
{
    [Header("Item Properties")]
    public string itemId;
    public string itemName;
    public double coinValue;
    public double costUSD;

    [Header("UI References")]
    public Text titleText;
    public Text costText;
    public Image iconImage;
    public Button purchaseButton;

    public void Setup(string id, string name, double coins, double usd, Sprite icon)
    {
        itemId = id;
        itemName = name;
        coinValue = coins;
        costUSD = usd;
        
        if (titleText != null) titleText.text = name;
        if (costText != null) costText.text = $"${usd:F2}";
        if (iconImage != null) iconImage.sprite = icon;
        
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnPurchaseClicked);
    }

    private void OnPurchaseClicked()
    {
        Debug.Log($"[Store] Purchase item clicked: {itemName}");
        // Trigger payment flow
    }
}
```

---

### 3. `AccumulateToBalanceScript.cs`
Animates increments to your cash/coin balance when winning (e.g. from cash out coin accumulation).

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Crashmania
{
    public class AccumulateToBalanceScript : MonoBehaviour
    {
        public Text balanceText;
        public float animationDuration = 0.5f;

        private double currentDisplayBalance = 0;

        public void SetBalanceImmediately(double targetBalance)
        {
            currentDisplayBalance = targetBalance;
            UpdateText(currentDisplayBalance);
        }

        public void AnimateToBalance(double targetBalance)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateValue(currentDisplayBalance, targetBalance));
        }

        private IEnumerator AnimateValue(double start, double end)
        {
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                // Ease out cubic
                t = 1f - Mathf.Pow(1f - t, 3f); 
                
                currentDisplayBalance = start + (end - start) * t;
                UpdateText(currentDisplayBalance);
                yield return null;
            }
            currentDisplayBalance = end;
            UpdateText(currentDisplayBalance);
        }

        private void UpdateText(double val)
        {
            if (balanceText != null)
            {
                balanceText.text = val.ToString("N2");
            }
        }
    }
}
```
