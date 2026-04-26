using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GameUiOverlayView : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas canvasComponent;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("HUD")]
    [SerializeField] private RectTransform hudRoot;
    [SerializeField] private Button cheatButton;
    [SerializeField] private Button pauseButton;

    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Text pauseTitle;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRetryButton;
    [SerializeField] private Button pauseBackButton;
    [SerializeField] private Text musicLabel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Text sfxLabel;
    [SerializeField] private Slider sfxSlider;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultTitle;
    [SerializeField] private Text resultSubtitle;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button resultRetryButton;
    [SerializeField] private Button resultBackButton;

    public Canvas CanvasComponent => canvasComponent;
    public CanvasScaler CanvasScaler => canvasScaler;
    public RectTransform HudRoot => hudRoot;
    public GameObject PausePanel => pausePanel;
    public GameObject ResultPanel => resultPanel;
    public Text ResultTitle => resultTitle;
    public Text ResultSubtitle => resultSubtitle;
    public Button CheatButton => cheatButton;
    public Button PauseButton => pauseButton;
    public Button ResumeButton => resumeButton;
    public Button PauseRetryButton => pauseRetryButton;
    public Button PauseBackButton => pauseBackButton;
    public Slider MusicSlider => musicSlider;
    public Slider SfxSlider => sfxSlider;
    public Button NextLevelButton => nextLevelButton;
    public Button ResultRetryButton => resultRetryButton;
    public Button ResultBackButton => resultBackButton;

    public void BuildMissingUi()
    {
        EnsureCanvasComponents();

        RectTransform root = transform as RectTransform;
        if (root == null)
            return;

        hudRoot = EnsureRect("HudRoot", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        cheatButton = EnsureButton("CheatButton", hudRoot, "SKIP", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(86f, -42f), new Vector2(120f, 48f), new Color(0.86f, 0.18f, 0.22f));
        pauseButton = EnsureButton("PauseButton", hudRoot, "PAUSE", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-92f, -42f), new Vector2(120f, 48f), new Color(0.97f, 0.72f, 0.43f));

        RectTransform pauseRoot = EnsurePanelRoot("PausePanel", root, new Vector2(520f, 430f), new Color(0.98f, 0.90f, 0.77f, 0.96f));
        pausePanel = pauseRoot.gameObject;
        pauseTitle = EnsureText("PauseTitle", pauseRoot, "PAUSED", new Vector2(0f, 180f), new Vector2(360f, 60f), 34, new Color(0.72f, 0.19f, 0.23f));
        resumeButton = EnsureButton("ResumeButton", pauseRoot, "RESUME", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(220f, 60f), new Color(0.98f, 0.74f, 0.45f));
        pauseRetryButton = EnsureButton("PauseRetryButton", pauseRoot, "RETRY", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -5f), new Vector2(220f, 60f), new Color(0.98f, 0.52f, 0.42f));
        pauseBackButton = EnsureButton("PauseBackButton", pauseRoot, "BACK TO MENU", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(220f, 60f), new Color(0.96f, 0.39f, 0.43f));
        musicLabel = EnsureText("MusicLabel", pauseRoot, "Music", new Vector2(0f, 145f), new Vector2(220f, 32f), 22, new Color(0.72f, 0.19f, 0.23f));
        musicSlider = EnsureSlider("MusicSlider", pauseRoot, new Vector2(0f, 115f), new Vector2(260f, 24f));
        sfxLabel = EnsureText("SfxLabel", pauseRoot, "SFX", new Vector2(0f, 25f), new Vector2(220f, 32f), 22, new Color(0.72f, 0.19f, 0.23f));
        sfxSlider = EnsureSlider("SfxSlider", pauseRoot, new Vector2(0f, -35f), new Vector2(260f, 24f));

        RectTransform resultRoot = EnsurePanelRoot("ResultPanel", root, new Vector2(520f, 430f), new Color(0.98f, 0.92f, 0.80f, 0.96f));
        resultPanel = resultRoot.gameObject;
        resultTitle = EnsureText("ResultTitle", resultRoot, "RESULT", new Vector2(0f, 140f), new Vector2(360f, 60f), 34, new Color(0.72f, 0.19f, 0.23f));
        resultSubtitle = EnsureText("ResultSubtitle", resultRoot, "Message", new Vector2(0f, 70f), new Vector2(380f, 70f), 24, new Color(0.53f, 0.28f, 0.26f));
        nextLevelButton = EnsureButton("NextLevelButton", resultRoot, "NEXT LEVEL", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(220f, 60f), new Color(0.98f, 0.74f, 0.45f));
        resultRetryButton = EnsureButton("ResultRetryButton", resultRoot, "RETRY", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -85f), new Vector2(220f, 60f), new Color(0.98f, 0.52f, 0.42f));
        resultBackButton = EnsureButton("ResultBackButton", resultRoot, "BACK TO MENU", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(220f, 60f), new Color(0.96f, 0.39f, 0.43f));

        if (!Application.isPlaying)
        {
            pausePanel.SetActive(false);
            resultPanel.SetActive(false);
        }
    }

    public void BindActions(GameSessionManager sessionManager)
    {
        BindButton(cheatButton, sessionManager.SkipLevelForTesting);
        BindButton(pauseButton, sessionManager.TogglePause);
        BindButton(resumeButton, sessionManager.TogglePause);
        BindButton(pauseRetryButton, sessionManager.RetryLevel);
        BindButton(pauseBackButton, sessionManager.ReturnToMenu);
        BindButton(nextLevelButton, sessionManager.LoadNextLevel);
        BindButton(resultRetryButton, sessionManager.RetryLevel);
        BindButton(resultBackButton, sessionManager.ReturnToMenu);

        BindSlider(musicSlider, GameProgress.GetMusicVolume(), value =>
        {
            GameProgress.SetMusicVolume(value);
            sessionManager.ApplyAudioVolumes();
        });

        BindSlider(sfxSlider, GameProgress.GetSfxVolume(), value =>
        {
            GameProgress.SetSfxVolume(value);
            sessionManager.ApplyAudioVolumes();
        });
    }

    private void EnsureCanvasComponents()
    {
        canvasComponent = GetOrAddComponent<Canvas>(gameObject);
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComponent.sortingOrder = 999;

        canvasScaler = GetOrAddComponent<CanvasScaler>(gameObject);
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1280f, 720f);

        graphicRaycaster = GetOrAddComponent<GraphicRaycaster>(gameObject);
    }

    private RectTransform EnsurePanelRoot(string name, RectTransform parent, Vector2 size, Color color)
    {
        RectTransform panel = EnsureRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        Image image = GetOrAddComponent<Image>(panel.gameObject);
        image.sprite = GetWhiteSprite();
        image.color = color;
        return panel;
    }

    private RectTransform EnsureRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        Transform existing = parent.Find(name);
        GameObject target = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        target.layer = 5;
        if (target.transform.parent != parent)
            target.transform.SetParent(parent, false);

        RectTransform rect = target.GetComponent<RectTransform>();
        bool isNew = existing == null;

        if (isNew)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);

            if (anchorMin == new Vector2(0f, 1f))
                rect.pivot = new Vector2(0f, 1f);
            else if (anchorMin == new Vector2(1f, 1f))
                rect.pivot = new Vector2(1f, 1f);

            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        return rect;
    }

    private Button EnsureButton(string name, RectTransform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        RectTransform rect = EnsureRect(name, parent, anchorMin, anchorMax, anchoredPosition, size);
        Image image = GetOrAddComponent<Image>(rect.gameObject);
        image.sprite = GetWhiteSprite();
        image.color = color;

        Button button = GetOrAddComponent<Button>(rect.gameObject);
        button.targetGraphic = image;

        Text text = EnsureText("Label", rect, label, Vector2.zero, size, 24, Color.white);
        text.transform.SetAsLastSibling();
        return button;
    }

    private Text EnsureText(string name, RectTransform parent, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, Color color)
    {
        RectTransform rect = EnsureRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size);
        Text text = GetOrAddComponent<Text>(rect.gameObject);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    private Slider EnsureSlider(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = EnsureRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size);
        Slider slider = GetOrAddComponent<Slider>(rect.gameObject);
        slider.minValue = 0f;
        slider.maxValue = 1f;

        RectTransform background = EnsureRect("Background", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        Image backgroundImage = GetOrAddComponent<Image>(background.gameObject);
        backgroundImage.sprite = GetWhiteSprite();
        backgroundImage.color = new Color(0.95f, 0.74f, 0.45f);

        RectTransform fillArea = EnsureRect("Fill Area", rect, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), Vector2.zero, Vector2.zero);
        fillArea.offsetMin = new Vector2(10f, 0f);
        fillArea.offsetMax = new Vector2(-10f, 0f);

        RectTransform fill = EnsureRect("Fill", fillArea, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        Image fillImage = GetOrAddComponent<Image>(fill.gameObject);
        fillImage.sprite = GetWhiteSprite();
        fillImage.color = new Color(0.89f, 0.25f, 0.31f);

        RectTransform handle = EnsureRect("Handle", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f));
        Image handleImage = GetOrAddComponent<Image>(handle.gameObject);
        handleImage.sprite = GetWhiteSprite();
        handleImage.color = new Color(1f, 1f, 0.96f);

        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void BindSlider(Slider slider, float value, UnityEngine.Events.UnityAction<float> action)
    {
        if (slider == null)
            return;

        slider.onValueChanged.RemoveAllListeners();
        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(action);
    }

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
            component = target.AddComponent<T>();
        return component;
    }

    private Sprite GetWhiteSprite()
    {
        return Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
    }
}
