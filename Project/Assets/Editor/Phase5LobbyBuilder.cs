#if UNITY_EDITOR
using System.IO;
using Crashmania.UI.Components;
using Crashmania.UI.Lobby;
using Crashmania.UI.Shell;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Crashmania.Editor
{
    public static class Phase5LobbyBuilder
    {
        private const string PrefabDir = "Assets/Resources/UI/Prefabs";

        [MenuItem("Crashmania/Build Phase 5 Lobby Assets")]
        public static void Run()
        {
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory("Assets/Resources/UI/Promotions/Lobby");

            ImportSprites();
            BuildCategoryChipPrefab();
            SavePrefab(BuildGameCard("GameCard", false), $"{PrefabDir}/GameCard.prefab");
            SavePrefab(BuildGameCard("GameCardTop10", true), $"{PrefabDir}/GameCardTop10.prefab");
            BuildPromoBannerPrefab();
            BuildGamesCarouselPrefab();
            BuildHeaderOverlayPrefab();
            BuildTabBarOverlayPrefab();
            BuildLobbyScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase5LobbyBuilder] Phase 5 lobby prefabs and Lobby.unity hierarchy rebuilt.");
        }

        private static void ImportSprites()
        {
            foreach (var path in new[]
            {
                "Assets/Resources/UI/Promotions/Lobby/mission.png",
                "Assets/Resources/UI/Promotions/Lobby/lobby-bg.png",
                "Assets/Resources/UI/Promotions/Lobby/front-image.png",
                "Assets/Resources/UI/Promotions/Lobby/gift.png",
                "Assets/Resources/UI/Promotions/Lobby/gift-sweep.png",
                "Assets/Resources/UI/Games/Top10/1.png",
                "Assets/Resources/UI/Games/Top10/2.png",
                "Assets/Resources/UI/Games/Top10/3.png",
                "Assets/Resources/UI/Games/Homepage/astro_go.png",
                "Assets/Resources/UI/Games/Homepage/tiltx.png"
            })
            {
                SpriteAt(path);
            }
        }

        private static GameObject UI(string name, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private static RectTransform RT(GameObject go)
        {
            return go.GetComponent<RectTransform>();
        }

        private static void Full(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void Rect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchored, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchored;
            rt.sizeDelta = size;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        private static TMP_Text Text(GameObject go, string value, int size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            return text;
        }

        private static Image Img(GameObject go, Color color)
        {
            var image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Button Btn(GameObject go)
        {
            var button = go.GetComponent<Button>() ?? go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.94f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            button.colors = colors;
            return button;
        }

        private static void Field(Object target, string name, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(name).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite SpriteAt(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static GameObject MakeLabel(
            Transform parent,
            string name,
            string value,
            int size,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 pos,
            Vector2 sizeDelta,
            TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var go = UI(name, parent);
            Rect(RT(go), anchorMin, anchorMax, pivot, pos, sizeDelta);
            Text(go, value, size, color, align);
            return go;
        }

        private static void BuildCategoryChipPrefab()
        {
            var root = UI("CategoryChip");
            Rect(RT(root), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 76));
            Img(root, Hex("#111117"));
            Btn(root);
            root.AddComponent<LayoutElement>().preferredWidth = 300;

            var label = MakeLabel(root.transform, "Label", "ALL", 34, Color.white, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var view = root.AddComponent<CategoryChipView>();
            Field(view, "button", root.GetComponent<Button>());
            Field(view, "background", root.GetComponent<Image>());
            Field(view, "label", label.GetComponent<TMP_Text>());
            SavePrefab(root, $"{PrefabDir}/CategoryChip.prefab");
        }

        private static GameObject BuildGameCard(string name, bool top10)
        {
            var root = UI(name);
            Rect(RT(root), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, top10 ? new Vector2(342, 392) : new Vector2(326, 372));
            Img(root, Color.black);
            root.AddComponent<SkewRect>();
            Btn(root);

            var le = root.AddComponent<LayoutElement>();
            le.preferredWidth = top10 ? 342 : 326;
            le.preferredHeight = top10 ? 392 : 372;

            var thumb = UI("Thumbnail", root.transform);
            Rect(RT(thumb), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -18), top10 ? new Vector2(300, 300) : new Vector2(292, 268));
            var thumbImage = Img(thumb, Hex("#22242E"));
            thumbImage.preserveAspect = true;

            var online = UI("Online", root.transform);
            Rect(RT(online), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -16), new Vector2(116, 44));
            Img(online, Hex("#050507"));
            MakeLabel(online.transform, "Text", "44", 24, Hex("#33FF66"), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var title = MakeLabel(root.transform, "Name", "GAME", 24, Color.white, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(-24, 44));
            var rank = MakeLabel(root.transform, "RankText", top10 ? "1" : string.Empty, 92, Hex("#FFD400"), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(14, 10), new Vector2(92, 112), TextAlignmentOptions.Left);
            if (!top10) rank.SetActive(false);

            var view = root.AddComponent<GameCardView>();
            Field(view, "button", root.GetComponent<Button>());
            Field(view, "thumbnail", thumbImage);
            Field(view, "nameText", title.GetComponent<TMP_Text>());
            Field(view, "onlineText", online.transform.Find("Text").GetComponent<TMP_Text>());
            Field(view, "rankText", rank.GetComponent<TMP_Text>());
            return root;
        }

        private static void BuildPromoBannerPrefab()
        {
            var root = UI("PromoBanner");
            Rect(RT(root), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1070, 560));
            var image = Img(root, Hex("#221B3D"));
            image.sprite = SpriteAt("Assets/Resources/UI/Promotions/Lobby/mission.png");
            image.preserveAspect = true;
            root.AddComponent<LayoutElement>().preferredHeight = 560;

            var title = MakeLabel(root.transform, "Title", "DAILY MISSION", 44, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -26), new Vector2(-60, 80));
            var view = root.AddComponent<PromoBannerView>();
            Field(view, "image", image);
            Field(view, "titleText", title.GetComponent<TMP_Text>());
            SavePrefab(root, $"{PrefabDir}/PromoBanner.prefab");
        }

        private static void BuildGamesCarouselPrefab()
        {
            var root = UI("GamesCarousel");
            Rect(RT(root), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1170, 560));
            var le = root.AddComponent<LayoutElement>();
            le.preferredHeight = 560;
            le.minHeight = 560;

            var header = UI("Header", root.transform);
            Rect(RT(header), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 108));
            var title = MakeLabel(header.transform, "Title", "LUCKY WEEK", 42, Color.white, new Vector2(0, 0), new Vector2(0.72f, 1), new Vector2(0, 0.5f), new Vector2(44, 0), Vector2.zero, TextAlignmentOptions.Left);

            var viewAll = UI("ViewAllButton", header.transform);
            Rect(RT(viewAll), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-34, 0), new Vector2(194, 62));
            Img(viewAll, Hex("#FFD400"));
            Btn(viewAll);
            MakeLabel(viewAll.transform, "Label", "VIEW ALL", 24, Color.black, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var prev = UI("PreviousButton", header.transform);
            Rect(RT(prev), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-270, 0), new Vector2(58, 58));
            Img(prev, Hex("#12131B"));
            Btn(prev);
            MakeLabel(prev.transform, "Label", "<", 34, Color.white, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var next = UI("NextButton", header.transform);
            Rect(RT(next), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-206, 0), new Vector2(58, 58));
            Img(next, Hex("#12131B"));
            Btn(next);
            MakeLabel(next.transform, "Label", ">", 34, Color.white, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var scroll = UI("ScrollRect", root.transform);
            Rect(RT(scroll), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -64), new Vector2(0, -128));
            var sr = scroll.AddComponent<ScrollRect>();
            sr.horizontal = true;
            sr.vertical = false;
            sr.inertia = true;
            sr.scrollSensitivity = 48;

            var viewport = UI("Viewport", scroll.transform);
            Full(RT(viewport));
            Img(viewport, new Color(0, 0, 0, 0));
            viewport.AddComponent<RectMask2D>();

            var content = UI("Content", viewport.transform);
            Rect(RT(content), new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), Vector2.zero, new Vector2(1700, 0));
            var hlg = content.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 38;
            hlg.padding = new RectOffset(44, 44, 0, 0);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.viewport = RT(viewport);
            sr.content = RT(content);

            var view = root.AddComponent<GamesCarouselView>();
            Field(view, "titleText", title.GetComponent<TMP_Text>());
            Field(view, "viewAllButton", viewAll.GetComponent<Button>());
            Field(view, "previousButton", prev.GetComponent<Button>());
            Field(view, "nextButton", next.GetComponent<Button>());
            Field(view, "content", RT(content));
            SavePrefab(root, $"{PrefabDir}/GamesCarousel.prefab");
        }

        private static void BuildHeaderOverlayPrefab()
        {
            var root = UI("HeaderOverlay");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            root.AddComponent<CanvasScaler>();
            CanvasResolutionPolicy.ApplyTo(root.GetComponent<CanvasScaler>());
            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<HeaderView>();
            Rect(RT(root), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var safe = UI("Safe Area", root.transform);
            Full(RT(safe));
            var bar = UI("Header Bar", safe.transform);
            Rect(RT(bar), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 210));
            Img(bar, Hex("#111014"));

            var yellow = UI("Gold Strip", bar.transform);
            Rect(RT(yellow), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 76));
            Img(yellow, Hex("#FFD400"));
            MakeLabel(yellow.transform, "Logo", "CRASHMANIA", 34, Color.black, new Vector2(0, 0), new Vector2(0.35f, 1), new Vector2(0, 0.5f), new Vector2(42, 0), Vector2.zero, TextAlignmentOptions.Left);
            MakeLabel(yellow.transform, "Menu", "MENU", 24, Color.black, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-42, 0), new Vector2(160, 0));

            var cc = UI("CC Balance", bar.transform);
            Rect(RT(cc), new Vector2(0, 0), new Vector2(0.56f, 0), new Vector2(0, 0), new Vector2(36, 34), new Vector2(-52, 84));
            Img(cc, Hex("#FFD400"));
            MakeLabel(cc.transform, "CC Label", "CC", 28, Color.black, new Vector2(0, 0), new Vector2(0.32f, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var ccValue = MakeLabel(cc.transform, "CC Value", "250,000", 34, Color.black, new Vector2(0.32f, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            ccValue.AddComponent<AccumulateToBalance>();

            var sc = UI("SC Balance", bar.transform);
            Rect(RT(sc), new Vector2(0.58f, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 34), new Vector2(-42, 84));
            Img(sc, Hex("#1C1D27"));
            MakeLabel(sc.transform, "SC Label", "SC", 28, Hex("#FFD400"), new Vector2(0, 0), new Vector2(0.32f, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var scValue = MakeLabel(sc.transform, "SC Value", "0", 34, Color.white, new Vector2(0.32f, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            scValue.AddComponent<AccumulateToBalance>();
            SavePrefab(root, $"{PrefabDir}/HeaderOverlay.prefab");
        }

        private static void BuildTabBarOverlayPrefab()
        {
            var root = UI("TabBarOverlay");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            root.AddComponent<CanvasScaler>();
            CanvasResolutionPolicy.ApplyTo(root.GetComponent<CanvasScaler>());
            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<TabBarView>();
            Rect(RT(root), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var safe = UI("Safe Area", root.transform);
            Full(RT(safe));
            var bar = UI("Tab Bar", safe.transform);
            Rect(RT(bar), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 196));
            Img(bar, Hex("#08080B"));

            var names = new[] { "STORE Tab", "GIFTS Tab", "HOME Tab", "REDEEM Tab", "ACCOUNT Tab" };
            var labels = new[] { "STORE", "GIFTS", "LOBBY", "REDEEM", "ACCOUNT" };
            for (var i = 0; i < names.Length; i++)
            {
                var tab = UI(names[i], bar.transform);
                Rect(RT(tab), new Vector2(i / 5f, 0), new Vector2((i + 1) / 5f, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                Img(tab, new Color(0, 0, 0, 0));
                Btn(tab);

                var icon = UI("Icon", tab.transform);
                Rect(RT(icon), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, i == 2 ? -10 : -36), i == 2 ? new Vector2(110, 110) : new Vector2(72, 72));
                Img(icon, i == 2 ? Hex("#FFD400") : Hex("#5E6374"));
                MakeLabel(tab.transform, "Label", labels[i], i == 2 ? 27 : 24, i == 2 ? Color.white : Hex("#9BA0B0"), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(0, 48));
            }

            SavePrefab(root, $"{PrefabDir}/TabBarOverlay.prefab");
        }

        private static void BuildLobbyScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                Object.DestroyImmediate(root);
            }

            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var cam = cameraGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("#292C38");
            cam.orthographic = true;
            cameraGO.AddComponent<AudioListener>();
            cameraGO.transform.position = new Vector3(0, 0, -10);

            var lightGO = new GameObject("Directional Light");
            lightGO.AddComponent<Light>().type = LightType.Directional;
            lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasGO = UI("LobbyCanvas");
            var lobbyCanvas = canvasGO.AddComponent<Canvas>();
            lobbyCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            CanvasResolutionPolicy.ApplyTo(scaler);
            canvasGO.AddComponent<GraphicRaycaster>();
            var canvasBg = Img(canvasGO, Hex("#292C38"));
            canvasBg.raycastTarget = false;
            var lobbyView = canvasGO.AddComponent<LobbyView>();

            var scroll = UI("ScrollRect", canvasGO.transform);
            Full(RT(scroll));
            var mainScroll = scroll.AddComponent<ScrollRect>();
            mainScroll.horizontal = false;
            mainScroll.vertical = true;
            mainScroll.movementType = ScrollRect.MovementType.Clamped;
            mainScroll.inertia = true;
            mainScroll.scrollSensitivity = 70;

            var viewport = UI("Viewport", scroll.transform);
            Full(RT(viewport));
            Img(viewport, new Color(0, 0, 0, 0));
            viewport.AddComponent<RectMask2D>();

            var content = UI("Content", viewport.transform);
            Rect(RT(content), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 3000));
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 0;
            vlg.padding = new RectOffset(0, 0, 220, 220);
            var fit = content.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            mainScroll.viewport = RT(viewport);
            mainScroll.content = RT(content);

            var promo = UI("PromoSection", content.transform);
            promo.AddComponent<LayoutElement>().preferredHeight = 860;
            Img(promo, Hex("#302A44"));
            var bgImg = UI("PromoBackground", promo.transform);
            Full(RT(bgImg));
            var lobbyBg = Img(bgImg, Hex("#302A44"));
            lobbyBg.sprite = SpriteAt("Assets/Resources/UI/Promotions/Lobby/lobby-bg.png");
            lobbyBg.preserveAspect = false;
            MakeLabel(promo.transform, "MissionProgress", "1/3 SPINS     08:12:45", 38, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -34), new Vector2(-80, 72));

            var mainPromo = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/PromoBanner.prefab"), promo.transform);
            mainPromo.name = "MainPromo";
            Rect(RT(mainPromo), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -8), new Vector2(1070, 560));

            var badgeL = UI("SideBadgeLeft", promo.transform);
            Rect(RT(badgeL), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, -22), new Vector2(132, 232));
            Img(badgeL, Hex("#FFD400"));
            MakeLabel(badgeL.transform, "Label", "GIFT", 28, Color.black, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var badgeR = UI("SideBadgeRight", promo.transform);
            Rect(RT(badgeR), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, -22), new Vector2(132, 232));
            Img(badgeR, Hex("#FF335F"));
            MakeLabel(badgeR.transform, "Label", "OFFER", 26, Color.white, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            MakeLabel(promo.transform, "BonusCopy", "DAILY REWARD / SWEEPSTAKES OFFERS", 30, Hex("#FFD400"), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 34), new Vector2(-70, 70));

            var recent = UI("RecentMultipliers", content.transform);
            recent.AddComponent<LayoutElement>().preferredHeight = 106;
            Img(recent, Hex("#151620"));
            var recentText = MakeLabel(recent.transform, "Text", "RECENT MULTIPLIERS", 28, Color.white, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Left);

            var rail = UI("CategoryRail", content.transform);
            rail.AddComponent<LayoutElement>().preferredHeight = 154;
            Img(rail, Hex("#0E0F16"));
            var search = UI("SearchInput", rail.transform);
            Rect(RT(search), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(38, 0), new Vector2(148, 82));
            Img(search, Hex("#20222D"));
            var input = search.AddComponent<TMP_InputField>();
            var textArea = UI("Text Area", search.transform);
            Full(RT(textArea));
            var placeholder = MakeLabel(textArea.transform, "Placeholder", "SEARCH", 20, Hex("#9EA4B4"), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var inputText = MakeLabel(textArea.transform, "Text", string.Empty, 20, Color.white, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            input.placeholder = placeholder.GetComponent<TMP_Text>();
            input.textComponent = inputText.GetComponent<TMP_Text>();

            var chipScroll = UI("ScrollRect", rail.transform);
            Rect(RT(chipScroll), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(100, 0), new Vector2(-200, 0));
            var chipSR = chipScroll.AddComponent<ScrollRect>();
            chipSR.horizontal = true;
            chipSR.vertical = false;
            var chipViewport = UI("Viewport", chipScroll.transform);
            Full(RT(chipViewport));
            Img(chipViewport, new Color(0, 0, 0, 0));
            chipViewport.AddComponent<RectMask2D>();
            var chipContent = UI("Content", chipViewport.transform);
            Rect(RT(chipContent), new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), Vector2.zero, new Vector2(1500, 0));
            var chipHlg = chipContent.AddComponent<HorizontalLayoutGroup>();
            chipHlg.childAlignment = TextAnchor.MiddleLeft;
            chipHlg.spacing = 22;
            chipHlg.padding = new RectOffset(16, 16, 0, 0);
            chipHlg.childForceExpandWidth = false;
            chipHlg.childForceExpandHeight = false;
            chipContent.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            chipSR.viewport = RT(chipViewport);
            chipSR.content = RT(chipContent);

            var carousels = UI("CarouselSections", content.transform);
            var cle = carousels.AddComponent<LayoutElement>();
            cle.minHeight = 1680;
            cle.preferredHeight = 1680;
            Img(carousels, Hex("#292C38"));
            var carVlg = carousels.AddComponent<VerticalLayoutGroup>();
            carVlg.childForceExpandWidth = true;
            carVlg.childForceExpandHeight = false;
            carVlg.spacing = 14;
            carVlg.padding = new RectOffset(0, 0, 26, 60);
            carousels.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Field(lobbyView, "promoBanner", mainPromo.GetComponent<PromoBannerView>());
            Field(lobbyView, "categoryChipPrefab", AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/CategoryChip.prefab").GetComponent<CategoryChipView>());
            Field(lobbyView, "carouselPrefab", AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/GamesCarousel.prefab").GetComponent<GamesCarouselView>());
            Field(lobbyView, "gameCardPrefab", AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/GameCard.prefab").GetComponent<GameCardView>());
            Field(lobbyView, "topGameCardPrefab", AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/GameCardTop10.prefab").GetComponent<GameCardView>());
            Field(lobbyView, "categoryContent", RT(chipContent));
            Field(lobbyView, "carouselContent", RT(carousels));
            Field(lobbyView, "searchInput", input);
            Field(lobbyView, "recentMultipliersText", recentText.GetComponent<TMP_Text>());

            EditorSceneManager.SaveScene(scene);
        }
    }
}
#endif
