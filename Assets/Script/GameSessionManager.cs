using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance;

    private static bool isBootstrapped;

    private Canvas overlayCanvas;
    private GameObject hudRoot;
    private GameObject pausePanel;
    private GameObject resultPanel;
    private Text resultTitle;
    private Text resultSubtitle;
    private Button nextButton;
    private Button retryButton;
    private Button resumeButton;
    private Slider musicSlider;
    private Slider sfxSlider;

    private PlayerController currentPlayer;
    private string currentSceneName;
    private Vector3 lastSafePosition;
    private bool hasSafePosition;
    private bool isRespawning;
    private bool isPaused;
    private float lastCheckpointSaveTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (isBootstrapped || Instance != null)
            return;

        GameObject managerObject = new GameObject("GameSessionManager");
        managerObject.AddComponent<GameSessionManager>();
        isBootstrapped = true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        isBootstrapped = true;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!IsGameplayScene(currentSceneName))
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (currentPlayer == null)
            currentPlayer = FindObjectOfType<PlayerController>();

        if (currentPlayer == null || isRespawning || isPaused)
            return;

        if (currentPlayer.IsGrounded && Time.unscaledTime - lastCheckpointSaveTime >= 0.5f)
        {
            lastCheckpointSaveTime = Time.unscaledTime;
            lastSafePosition = currentPlayer.transform.position;
            hasSafePosition = true;
            GameProgress.SaveCheckpoint(currentSceneName, lastSafePosition);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        currentPlayer = null;
        hasSafePosition = false;
        isRespawning = false;
        isPaused = false;
        lastCheckpointSaveTime = 0f;
        Time.timeScale = 1f;

        DestroyOverlay();

        if (!IsGameplayScene(scene.name))
            return;

        BuildOverlay();
        StartCoroutine(AttachToPlayerRoutine());
    }

    public void HandlePlayerDeath(PlayerController player)
    {
        if (isRespawning)
            return;

        currentPlayer = player;
        AnalyticsManager.Instance?.OnPlayerDeath();
        StartCoroutine(RespawnRoutine());
    }

    public void HandleLevelComplete(PlayerController player)
    {
        currentPlayer = player;
        GameProgress.UnlockNextLevel(currentSceneName);
        GameProgress.ClearCheckpoint(currentSceneName);
        AnalyticsManager.Instance?.OnLevelComplete();
        ShowResult("LEVEL COMPLETE", "Sticky and slippery mastered.", true);
    }

    public void SkipLevelForTesting()
    {
        if (!IsGameplayScene(currentSceneName))
            return;

        HandleLevelComplete(currentPlayer);
    }

    public void RetryLevel()
    {
        ResumeTime();
        SceneManager.LoadScene(currentSceneName);
    }

    public void LoadNextLevel()
    {
        ResumeTime();
        SceneManager.LoadScene(GameProgress.GetNextSceneName(currentSceneName));
    }

    public void ReturnToMenu()
    {
        ResumeTime();
        SceneManager.LoadScene("MainMenu");
    }

    public void TogglePause()
    {
        if (!IsGameplayScene(currentSceneName) || resultPanel == null || pausePanel == null)
            return;

        if (resultPanel.activeSelf)
            return;

        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);

        if (hudRoot != null)
            hudRoot.SetActive(!isPaused);

        if (isPaused)
            Time.timeScale = 0f;
        else
            ResumeTime();
    }

    private IEnumerator AttachToPlayerRoutine()
    {
        yield return null;

        currentPlayer = FindObjectOfType<PlayerController>();
        if (currentPlayer == null)
            yield break;

        if (GameProgress.TryGetCheckpoint(currentSceneName, out Vector3 savedCheckpoint))
        {
            currentPlayer.transform.position = savedCheckpoint;
            lastSafePosition = savedCheckpoint;
            hasSafePosition = true;
        }
        else
        {
            lastSafePosition = currentPlayer.transform.position;
            hasSafePosition = true;
            GameProgress.SaveCheckpoint(currentSceneName, lastSafePosition);
        }

        ApplyAudioVolumes();
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        ShowResult("GAME OVER", hasSafePosition ? "Respawning from your latest checkpoint..." : "Restarting level...", false);

        if (currentPlayer != null)
            currentPlayer.SetGameplayEnabled(false);

        yield return new WaitForSecondsRealtime(0.75f);

        if (currentPlayer == null)
        {
            RetryLevel();
            yield break;
        }

        Vector3 respawnPoint = hasSafePosition ? lastSafePosition : currentPlayer.StartPosition;
        currentPlayer.Respawn(respawnPoint);
        ShowGameplayHud();
        isRespawning = false;
    }

    private void BuildOverlay()
    {
        GameObject canvasObject = new GameObject("GameOverlayCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(canvasObject);

        overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        RectTransform root = canvasObject.GetComponent<RectTransform>();

        hudRoot = new GameObject("HudRoot", typeof(RectTransform));
        hudRoot.layer = 5;
        hudRoot.transform.SetParent(root, false);

        CreateButton(hudRoot.transform as RectTransform, "CheatButton", "SKIP", new Vector2(86f, -42f), new Vector2(120f, 48f), new Color(0.86f, 0.18f, 0.22f), SkipLevelForTesting);
        CreateButton(hudRoot.transform as RectTransform, "PauseButton", "PAUSE", new Vector2(-92f, -42f), new Vector2(120f, 48f), new Color(0.97f, 0.72f, 0.43f), TogglePause, TextAnchor.UpperRight);

        pausePanel = CreatePanel(root, "PausePanel", new Color(0.98f, 0.9f, 0.77f, 0.96f), false);
        CreateTitle((pausePanel.transform as RectTransform), "PAUSED", new Vector2(0f, 180f));
        resumeButton = CreateButton(pausePanel.transform as RectTransform, "ResumeButton", "RESUME", new Vector2(0f, 70f), new Vector2(220f, 60f), new Color(0.98f, 0.74f, 0.45f), TogglePause);
        retryButton = CreateButton(pausePanel.transform as RectTransform, "RetryButton", "RETRY", new Vector2(0f, -5f), new Vector2(220f, 60f), new Color(0.98f, 0.52f, 0.42f), RetryLevel);
        CreateButton(pausePanel.transform as RectTransform, "BackMenuButton", "BACK TO MENU", new Vector2(0f, -150f), new Vector2(220f, 60f), new Color(0.96f, 0.39f, 0.43f), ReturnToMenu);

        CreateLabel(pausePanel.transform as RectTransform, "MusicLabel", "Music", new Vector2(0f, 145f));
        musicSlider = CreateSlider(pausePanel.transform as RectTransform, "MusicSlider", new Vector2(0f, 115f), GameProgress.GetMusicVolume(), value =>
        {
            GameProgress.SetMusicVolume(value);
            ApplyAudioVolumes();
        });

        CreateLabel(pausePanel.transform as RectTransform, "SfxLabel", "SFX", new Vector2(0f, 25f));
        sfxSlider = CreateSlider(pausePanel.transform as RectTransform, "SfxSlider", new Vector2(0f, -35f), GameProgress.GetSfxVolume(), value =>
        {
            GameProgress.SetSfxVolume(value);
            ApplyAudioVolumes();
        });

        resultPanel = CreatePanel(root, "ResultPanel", new Color(0.98f, 0.92f, 0.8f, 0.96f), false);
        resultTitle = CreateTitle(resultPanel.transform as RectTransform, "RESULT", new Vector2(0f, 140f));
        resultSubtitle = CreateBodyText(resultPanel.transform as RectTransform, "ResultSubtitle", "Message", new Vector2(0f, 70f));
        nextButton = CreateButton(resultPanel.transform as RectTransform, "NextButton", "NEXT LEVEL", new Vector2(0f, -10f), new Vector2(220f, 60f), new Color(0.98f, 0.74f, 0.45f), LoadNextLevel);
        CreateButton(resultPanel.transform as RectTransform, "ResultRetryButton", "RETRY", new Vector2(0f, -85f), new Vector2(220f, 60f), new Color(0.98f, 0.52f, 0.42f), RetryLevel);
        CreateButton(resultPanel.transform as RectTransform, "ResultMenuButton", "BACK TO MENU", new Vector2(0f, -160f), new Vector2(220f, 60f), new Color(0.96f, 0.39f, 0.43f), ReturnToMenu);
    }

    private void DestroyOverlay()
    {
        if (overlayCanvas == null)
            return;

        Destroy(overlayCanvas.gameObject);
        overlayCanvas = null;
        hudRoot = null;
        pausePanel = null;
        resultPanel = null;
        resultTitle = null;
        resultSubtitle = null;
        nextButton = null;
        retryButton = null;
        resumeButton = null;
        musicSlider = null;
        sfxSlider = null;
    }

    private void ShowResult(string title, string subtitle, bool showNext)
    {
        if (resultPanel == null || resultTitle == null || resultSubtitle == null)
            return;

        resultTitle.text = title;
        resultSubtitle.text = subtitle;
        resultPanel.SetActive(true);
        hudRoot?.SetActive(false);
        pausePanel?.SetActive(false);
        if (nextButton != null)
            nextButton.gameObject.SetActive(showNext);
        Time.timeScale = 0f;
    }

    private void ShowGameplayHud()
    {
        resultPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        hudRoot?.SetActive(true);
        ResumeTime();
    }

    private void ResumeTime()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    private bool IsGameplayScene(string sceneName)
    {
        return sceneName == "Level_1"
            || sceneName == "Level_2"
            || sceneName == "Level_3"
            || sceneName == "Scenesgame";
    }

    private void ApplyAudioVolumes()
    {
        AudioSource[] sources = FindObjectsOfType<AudioSource>(true);
        float musicVolume = GameProgress.GetMusicVolume();
        float sfxVolume = GameProgress.GetSfxVolume();

        foreach (AudioSource source in sources)
        {
            source.volume = source.loop ? musicVolume : sfxVolume;
        }
    }

    private GameObject CreatePanel(RectTransform parent, string name, Color color, bool active)
    {
        GameObject panel = CreateImage(parent, name, color, Vector2.zero, new Vector2(520f, 430f), new Vector2(0.5f, 0.5f));
        panel.SetActive(active);
        return panel;
    }

    private Text CreateTitle(RectTransform parent, string content, Vector2 position)
    {
        return CreateText(parent, "Title", content, position, new Vector2(360f, 60f), 34, new Color(0.72f, 0.19f, 0.23f));
    }

    private Text CreateBodyText(RectTransform parent, string name, string content, Vector2 position)
    {
        Text text = CreateText(parent, name, content, position, new Vector2(380f, 70f), 24, new Color(0.53f, 0.28f, 0.26f));
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    private void CreateLabel(RectTransform parent, string name, string content, Vector2 position)
    {
        Text label = CreateText(parent, name, content, position, new Vector2(220f, 32f), 22, new Color(0.72f, 0.19f, 0.23f));
        label.alignment = TextAnchor.MiddleCenter;
    }

    private Button CreateButton(RectTransform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, Color color, UnityEngine.Events.UnityAction action, TextAnchor anchor = TextAnchor.UpperLeft)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.layer = 5;
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor == TextAnchor.UpperRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = anchor == TextAnchor.UpperRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);

        if (anchor == TextAnchor.MiddleCenter)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        Text text = CreateText(rect, $"{name}_Text", label, Vector2.zero, size, 24, Color.white);
        text.alignment = TextAnchor.MiddleCenter;
        return button;
    }

    private Slider CreateSlider(RectTransform parent, string name, Vector2 anchoredPosition, float value, UnityEngine.Events.UnityAction<float> onChanged)
    {
        GameObject sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderObject.layer = 5;
        sliderObject.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = anchoredPosition;
        sliderRect.sizeDelta = new Vector2(260f, 24f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;

        GameObject background = CreateImage(sliderRect, "Background", new Color(0.95f, 0.74f, 0.45f), Vector2.zero, new Vector2(260f, 24f), new Vector2(0.5f, 0.5f));
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.layer = 5;
        fillArea.transform.SetParent(sliderRect, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(10f, 0f);
        fillAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject fill = CreateImage(fillAreaRect, "Fill", new Color(0.89f, 0.25f, 0.31f), Vector2.zero, Vector2.zero, new Vector2(0f, 0.5f));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handle = CreateImage(sliderRect, "Handle", new Color(1f, 1f, 0.96f), Vector2.zero, new Vector2(24f, 24f), new Vector2(0.5f, 0.5f));
        slider.fillRect = fillRect;
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.onValueChanged.AddListener(onChanged);
        return slider;
    }

    private GameObject CreateImage(RectTransform parent, string name, Color color, Vector2 anchoredPosition, Vector2 size, Vector2 anchor)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.layer = 5;
        gameObject.transform.SetParent(parent, false);

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = gameObject.GetComponent<Image>();
        image.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
        image.color = color;
        return gameObject;
    }

    private Text CreateText(RectTransform parent, string name, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.layer = 5;
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }
}
