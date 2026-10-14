using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RuntimeSceneUI : MonoBehaviour
{
    private static RuntimeSceneUI instance;
    private static Text messageText;
    private static Text menuHighScoreText;
    private static Text pauseButtonText;

    public static void Build(string sceneName)
    {
        if (instance != null)
            Destroy(instance.gameObject);

        GameObject root = new GameObject("Lab4 Runtime UI");
        instance = root.AddComponent<RuntimeSceneUI>();

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        root.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();

        if (sceneName == "MainMenu")
            BuildMainMenu(root.transform);
        else if (sceneName == "LoadingScene")
            BuildLoadingScene(root.transform);
        else if (sceneName.StartsWith("World-"))
            BuildWorldOverlay(root.transform, sceneName);
    }

    private static void BuildMainMenu(Transform parent)
    {
        CreateFullScreenBackground(parent, new Color(0.05f, 0.1f, 0.22f, 1f));
        CreateText(parent, "SUPER MARIO LAB 4", 48, new Vector2(0f, 170f), new Vector2(900f, 90f));

        menuHighScoreText = CreateText(parent,
            "HIGH SCORE: " + LabGameManager.Instance.State.highScore,
            28, new Vector2(0f, 90f), new Vector2(600f, 60f));

        CreateButton(parent, "START GAME", new Vector2(0f, 10f),
            () => LabGameManager.Instance.StartNewGame());

        CreateButton(parent, "RESET HIGH SCORE", new Vector2(0f, -70f),
            () =>
            {
                LabGameManager.Instance.ResetHighScore();
                menuHighScoreText.text = "HIGH SCORE: " + LabGameManager.Instance.State.highScore;
            });

        CreateText(parent, "Lab 4: scenes, persistence, power-ups, feedback and restart",
            18, new Vector2(0f, -170f), new Vector2(1000f, 50f));
    }

    private static void BuildLoadingScene(Transform parent)
    {
        CreateFullScreenBackground(parent, new Color(0.02f, 0.02f, 0.05f, 1f));
        CreateText(parent, "LOADING...", 46, new Vector2(0f, 130f), new Vector2(700f, 80f));
        CreateText(parent, "World 1-1 is ready", 24, new Vector2(0f, 60f), new Vector2(700f, 50f));

        CreateButton(parent, "ENTER WORLD 1-1", new Vector2(0f, -25f),
            () => LabGameManager.Instance.EnterWorldOne());

        CreateButton(parent, "BACK TO MAIN MENU", new Vector2(0f, -105f),
            () => LabGameManager.Instance.GoToMainMenu());
    }

    private static void BuildWorldOverlay(Transform parent, string sceneName)
    {
        CreateText(parent, sceneName.ToUpperInvariant(), 24,
            new Vector2(0f, -35f), new Vector2(420f, 50f));

        pauseButtonText = CreateButton(parent, "PAUSE (P)", new Vector2(-130f, -40f),
            () => LabGameManager.Instance.TogglePause(),
            new Vector2(220f, 55f), new Vector2(1f, 1f));

        if (sceneName == "World-1-1")
        {
            CreateButton(parent, "NEXT WORLD", new Vector2(-130f, -110f),
                () => LabGameManager.Instance.GoToNextWorld(),
                new Vector2(220f, 55f), new Vector2(1f, 1f));
        }

        CreateButton(parent, "MAIN MENU", new Vector2(-130f, -180f),
            () => LabGameManager.Instance.GoToMainMenu(),
            new Vector2(220f, 55f), new Vector2(1f, 1f));

        messageText = CreateText(parent, "", 26, new Vector2(0f, -70f), new Vector2(850f, 70f));

        LabGameManager.Instance.PauseChanged += OnPauseChanged;
    }

    private void OnDestroy()
    {
        if (LabGameManager.Instance != null)
            LabGameManager.Instance.PauseChanged -= OnPauseChanged;
    }

    private static void OnPauseChanged(bool paused)
    {
        if (pauseButtonText != null)
            pauseButtonText.text = paused ? "RESUME (P)" : "PAUSE (P)";

        ShowMessage(paused ? "PAUSED - MUSIC STOPPED" : "RESUMED - MUSIC PLAYING");
    }

    public static void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        messageText.text = message;
        if (instance != null)
            instance.StartCoroutine(instance.ClearMessage());
    }

    private IEnumerator ClearMessage()
    {
        yield return new WaitForSecondsRealtime(2f);
        if (messageText != null)
            messageText.text = "";
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static void CreateFullScreenBackground(Transform parent, Color color)
    {
        GameObject background = new GameObject("Background");
        background.transform.SetParent(parent, false);
        Image image = background.AddComponent<Image>();
        image.color = color;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Text CreateText(Transform parent, string content, int size,
        Vector2 position, Vector2 dimensions)
    {
        GameObject textObject = new GameObject("Text - " + content);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;
        return text;
    }

    private static Text CreateButton(Transform parent, string label, Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        return CreateButton(parent, label, position, action,
            new Vector2(360f, 60f), new Vector2(0.5f, 0.5f));
    }

    private static Text CreateButton(Transform parent, string label, Vector2 position,
        UnityEngine.Events.UnityAction action, Vector2 dimensions, Vector2 anchor)
    {
        GameObject buttonObject = new GameObject("Button - " + label);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.12f, 0.35f, 0.75f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        RectTransform rect = image.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;

        Text text = CreateText(buttonObject.transform, label, 21, Vector2.zero, dimensions);
        text.raycastTarget = false;
        return text;
    }
}
