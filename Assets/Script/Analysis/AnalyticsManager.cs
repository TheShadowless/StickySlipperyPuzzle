using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    private static bool isBootstrapped;

    private float sessionStartTime;
    private int deathCount = 0;
    private int winCount = 0;
    private int bulletUsed = 0;
    private int bulletSwitchCount = 0;
    private bool analyticsInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (isBootstrapped || Instance != null)
            return;

        GameObject analyticsObject = new GameObject("AnalyticsManager");
        analyticsObject.AddComponent<AnalyticsManager>();
        isBootstrapped = true;
    }

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            isBootstrapped = true;
            DontDestroyOnLoad(gameObject);

            await UnityServices.InitializeAsync();
#pragma warning disable CS0618
            AnalyticsService.Instance.StartDataCollection();
#pragma warning restore CS0618

            analyticsInitialized = true;
            sessionStartTime = Time.time;
            Debug.Log("Analytics Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetSessionTime()
    {
        return Time.time - sessionStartTime;
    }

    private bool EnsureAnalyticsReady(string eventName)
    {
        if (analyticsInitialized)
            return true;

        Debug.LogWarning($"Analytics not ready yet. Skipped event: {eventName}");
        return false;
    }

    public void OnLevelStart()
    {
        if (!EnsureAnalyticsReady("level_start"))
            return;

        CustomEvent evt = new CustomEvent("level_start");
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log("Level Start: " + SceneManager.GetActiveScene().name);
    }

    public void OnPlayerDeath()
    {
        if (!EnsureAnalyticsReady("level_fail"))
            return;

        deathCount++;

        CustomEvent evt = new CustomEvent("level_fail");
        evt.Add("death_count", deathCount);
        evt.Add("bullet_used", bulletUsed);
        evt.Add("session_time", GetSessionTime());
        evt.Add("scene_name", SceneManager.GetActiveScene().name);

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log($"Player Death | deathCount={deathCount}");
    }

    public void OnLevelFail()
    {
        OnPlayerDeath();
    }

    public void OnLevelComplete()
    {
        if (!EnsureAnalyticsReady("level_complete"))
            return;

        winCount++;

        CustomEvent evt = new CustomEvent("level_complete");
        evt.Add("win_count", winCount);
        evt.Add("death_count", deathCount);
        evt.Add("bullet_used", bulletUsed);
        evt.Add("session_time", GetSessionTime());
        evt.Add("scene_name", SceneManager.GetActiveScene().name);

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log($"Level Complete | winCount={winCount}");
    }

    public void OnBulletUsed(int bulletIndex, string bulletName, float speed)
    {
        if (!EnsureAnalyticsReady("bullet_used"))
            return;

        bulletUsed++;

        CustomEvent evt = new CustomEvent("bullet_used");
        evt.Add("bullet_total", bulletUsed);
        evt.Add("bullet_index", bulletIndex);
        evt.Add("bullet_name", bulletName);
        evt.Add("bullet_speed", speed);
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log($"Bullet Used: {bulletName} | total={bulletUsed}");
    }

    public void OnBulletUsed(string bulletName)
    {
        if (!EnsureAnalyticsReady("bullet_used"))
            return;

        bulletUsed++;

        CustomEvent evt = new CustomEvent("bullet_used");
        evt.Add("bullet_total", bulletUsed);
        evt.Add("bullet_index", -1);
        evt.Add("bullet_name", bulletName);
        evt.Add("bullet_speed", 0f);
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log($"Bullet Used: {bulletName} | total={bulletUsed}");
    }

    public void OnBulletSwitch(int bulletIndex, string bulletName)
    {
        if (!EnsureAnalyticsReady("bullet_switch"))
            return;

        bulletSwitchCount++;

        CustomEvent evt = new CustomEvent("bullet_switch");
        evt.Add("switch_count", bulletSwitchCount);
        evt.Add("bullet_index", bulletIndex);
        evt.Add("bullet_name", bulletName);
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log($"Bullet Switched: {bulletName} | switchCount={bulletSwitchCount}");
    }

    public void OnShootBlocked(string reason)
    {
        if (!EnsureAnalyticsReady("shoot_blocked"))
            return;

        CustomEvent evt = new CustomEvent("shoot_blocked");
        evt.Add("reason", reason);
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log($"Shoot Blocked: {reason}");
    }
}
