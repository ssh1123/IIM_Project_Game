#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WindVillage.EditorTools
{
    public static class WindVillageSceneBuilder
    {
        private const string RootPath = "Assets/WindVillage";
        private const string ScenePath = RootPath + "/Scenes/WindVillage_MainMap.unity";
        private const string MapImagePath = RootPath + "/Art/WindVillage_Map_Reference.png";

        [MenuItem("Tools/Wind Village/Build Main Map Scene")]
        public static void BuildScene()
        {
            EnsureFolders();
            ConfigureMapTexture();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateEventSystem();
            Canvas canvas = CreateCanvas();
            RectTransform mapRoot = CreatePanel("MapRoot", canvas.transform, new Color(0.92f, 0.88f, 0.74f, 1f));
            Stretch(mapRoot);

            CreateMapBackground(mapRoot);
            CreateHeader(mapRoot);

            GameObject controllerObject = new GameObject("WindVillageMapController");
            controllerObject.transform.SetParent(canvas.transform, false);
            WindVillageMapController controller = controllerObject.AddComponent<WindVillageMapController>();

            List<MapLocation> locations = new List<MapLocation>();
            locations.Add(CreateLocation(mapRoot, controller, "recycling", "回收站", "回收舊腳踏車、整理零件，發展成村內共享單車與修繕據點。", new Vector2(-570f, 260f), new Vector2(310f, 180f), new Color(0.64f, 0.78f, 0.65f, 0.90f)));
            locations.Add(CreateLocation(mapRoot, controller, "bamboo", "竹子工坊", "運用竹材、竹編技藝與共同工具，發展技能共享與地方體驗。", new Vector2(560f, 260f), new Vector2(340f, 190f), new Color(0.59f, 0.78f, 0.48f, 0.90f)));
            locations.Add(CreateLocation(mapRoot, controller, "temple", "廟宇", "村民信仰與聚會中心，可作為節慶活動、故事蒐集與社群連結的節點。", new Vector2(-560f, -270f), new Vector2(330f, 190f), new Color(0.90f, 0.63f, 0.45f, 0.90f)));
            locations.Add(CreateLocation(mapRoot, controller, "activity", "活動中心", "村民交換技能、共用設備並推動風待共同品牌的主要據點。", new Vector2(0f, -250f), new Vector2(390f, 210f), new Color(0.82f, 0.72f, 0.47f, 0.92f)));
            locations.Add(CreateLocation(mapRoot, controller, "empty_houses", "空屋巷", "盤點閒置空屋，規劃短期展覽、青年進駐或社區共用空間。", new Vector2(565f, -270f), new Vector2(340f, 190f), new Color(0.65f, 0.72f, 0.82f, 0.90f)));

            RectTransform player = CreatePlayerMarker(mapRoot);
            GameObject infoPanel;
            TMP_Text titleText;
            TMP_Text descriptionText;
            Button enterButton;
            Button closeButton;
            CreateInfoPanel(canvas.transform, out infoPanel, out titleText, out descriptionText, out enterButton, out closeButton);

            SerializedObject so = new SerializedObject(controller);
            SerializedProperty locationsProp = so.FindProperty("locations");
            locationsProp.arraySize = locations.Count;
            for (int i = 0; i < locations.Count; i++)
            {
                SerializedProperty element = locationsProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("id").stringValue = locations[i].id;
                element.FindPropertyRelative("displayName").stringValue = locations[i].displayName;
                element.FindPropertyRelative("description").stringValue = locations[i].description;
                element.FindPropertyRelative("target").objectReferenceValue = locations[i].target;
            }
            so.FindProperty("infoPanel").objectReferenceValue = infoPanel;
            so.FindProperty("titleText").objectReferenceValue = titleText;
            so.FindProperty("descriptionText").objectReferenceValue = descriptionText;
            so.FindProperty("enterButton").objectReferenceValue = enterButton;
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
            so.FindProperty("playerMarker").objectReferenceValue = player;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("Wind Village map scene created: " + ScenePath);
        }

        private static MapLocation CreateLocation(RectTransform parent, WindVillageMapController controller, string id, string title, string description, Vector2 position, Vector2 size, Color color)
        {
            GameObject targetObject = new GameObject(title + "_Target", typeof(RectTransform));
            RectTransform target = targetObject.GetComponent<RectTransform>();
            target.SetParent(parent, false);
            target.anchorMin = target.anchorMax = new Vector2(0.5f, 0.5f);
            target.anchoredPosition = position;
            target.sizeDelta = Vector2.zero;

            Button button = CreateButton(title + "Button", parent, title, position, size, color);
            LocationButton locationButton = button.gameObject.AddComponent<LocationButton>();
            locationButton.Configure(id, controller);
            UnityEventTools.AddPersistentListener(button.onClick, locationButton.Click);

            return new MapLocation { id = id, displayName = title, description = description, target = target };
        }

        private static void CreateMapBackground(RectTransform parent)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MapImagePath);
            GameObject bg = new GameObject("MapReferenceBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = bg.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            Image image = bg.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = new Color(1f, 1f, 1f, 0.28f);
            image.raycastTarget = false;
        }

        private static void CreateHeader(RectTransform parent)
        {
            RectTransform header = CreatePanel("Header", parent, new Color(0.18f, 0.24f, 0.20f, 0.92f));
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0f, 82f);
            header.anchoredPosition = Vector2.zero;
            TMP_Text text = CreateText("Title", header, "風待村｜主要地圖", 38, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
        }

        private static RectTransform CreatePlayerMarker(RectTransform parent)
        {
            GameObject marker = new GameObject("PlayerMarker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = marker.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(42f, 42f);
            rect.anchoredPosition = new Vector2(0f, -250f);
            Image image = marker.GetComponent<Image>();
            image.color = new Color(0.95f, 0.23f, 0.18f, 1f);
            image.raycastTarget = false;
            marker.transform.SetAsLastSibling();
            return rect;
        }

        private static void CreateInfoPanel(Transform parent, out GameObject panelObject, out TMP_Text titleText, out TMP_Text descriptionText, out Button enterButton, out Button closeButton)
        {
            RectTransform panel = CreatePanel("LocationInfoPanel", parent, new Color(0.08f, 0.10f, 0.09f, 0.96f));
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(680f, 330f);
            panel.anchoredPosition = Vector2.zero;
            panelObject = panel.gameObject;

            titleText = CreateText("LocationTitle", panel, "地點名稱", 38, TextAlignmentOptions.Center);
            titleText.rectTransform.anchorMin = new Vector2(0.08f, 0.72f);
            titleText.rectTransform.anchorMax = new Vector2(0.92f, 0.94f);
            titleText.rectTransform.offsetMin = titleText.rectTransform.offsetMax = Vector2.zero;

            descriptionText = CreateText("LocationDescription", panel, "地點說明", 25, TextAlignmentOptions.TopLeft);
            descriptionText.rectTransform.anchorMin = new Vector2(0.09f, 0.27f);
            descriptionText.rectTransform.anchorMax = new Vector2(0.91f, 0.70f);
            descriptionText.rectTransform.offsetMin = descriptionText.rectTransform.offsetMax = Vector2.zero;

            enterButton = CreateButton("EnterButton", panel, "進入地點", new Vector2(125f, -120f), new Vector2(210f, 65f), new Color(0.35f, 0.63f, 0.43f, 1f));
            closeButton = CreateButton("CloseButton", panel, "關閉", new Vector2(-125f, -120f), new Vector2(170f, 65f), new Color(0.55f, 0.55f, 0.55f, 1f));
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = color;
            Button button = go.GetComponent<Button>();
            TMP_Text text = CreateText("Label", rect, label, 28, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
            return button;
        }

        private static TMP_Text CreateText(string name, Transform parent, string content, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.enableWordWrapping = true;
            return text;
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return rect;
        }

        private static Canvas CreateCanvas()
        {
            GameObject go = new GameObject("WindVillageCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ConfigureMapTexture()
        {
            TextureImporter importer = AssetImporter.GetAtPath(MapImagePath) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(RootPath + "/Scenes"))
                AssetDatabase.CreateFolder(RootPath, "Scenes");
        }
    }
}
#endif
