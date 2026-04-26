using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance;

    private static bool isBootstrapped;
    private const string OverlayPrefabPath = "Prefabs/GameUiOverlay";

    private GameUiOverlayView overlayView;

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
            currentPlayer = FindAnyObjectByType<PlayerController>();

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
        if (!IsGameplayScene(currentSceneName) || overlayView == null || overlayView.ResultPanel == null || overlayView.PausePanel == null)
            return;

        if (overlayView.ResultPanel.activeSelf)
            return;

        isPaused = !isPaused;
        overlayView.PausePanel.SetActive(isPaused);

        if (overlayView.HudRoot != null)
            overlayView.HudRoot.gameObject.SetActive(!isPaused);

        if (isPaused)
            Time.timeScale = 0f;
        else
            ResumeTime();
    }

    private IEnumerator AttachToPlayerRoutine()
    {
        yield return null;

        currentPlayer = FindAnyObjectByType<PlayerController>();
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
        GameUiOverlayView overlayPrefab = Resources.Load<GameUiOverlayView>(OverlayPrefabPath);
        if (overlayPrefab == null)
        {
            Debug.LogError($"Missing UI overlay prefab at Resources/{OverlayPrefabPath}");
            return;
        }

        overlayView = Instantiate(overlayPrefab);
        overlayView.name = overlayPrefab.name;
        DontDestroyOnLoad(overlayView.gameObject);
        overlayView.BuildMissingUi();
        overlayView.BindActions(this);
    }

    private void DestroyOverlay()
    {
        if (overlayView == null)
            return;

        Destroy(overlayView.gameObject);
        overlayView = null;
    }

    private void ShowResult(string title, string subtitle, bool showNext)
    {
        if (overlayView == null || overlayView.ResultPanel == null || overlayView.ResultTitle == null || overlayView.ResultSubtitle == null)
            return;

        overlayView.ResultTitle.text = title;
        overlayView.ResultSubtitle.text = subtitle;
        overlayView.ResultPanel.SetActive(true);
        overlayView.HudRoot?.gameObject.SetActive(false);
        overlayView.PausePanel?.SetActive(false);
        if (overlayView.NextLevelButton != null)
            overlayView.NextLevelButton.gameObject.SetActive(showNext);
        Time.timeScale = 0f;
    }

    private void ShowGameplayHud()
    {
        overlayView?.ResultPanel?.SetActive(false);
        overlayView?.PausePanel?.SetActive(false);
        overlayView?.HudRoot?.gameObject.SetActive(true);
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

    public void ApplyAudioVolumes()
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        float musicVolume = GameProgress.GetMusicVolume();
        float sfxVolume = GameProgress.GetSfxVolume();

        foreach (AudioSource source in sources)
        {
            source.volume = source.loop ? musicVolume : sfxVolume;
        }
    }
}
