using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    private static bool isBootstrapped;

    private float sessionStartTime;
    private int deathCount = 0;
    private int winCount = 0;
    private int bulletUsed = 0;
    private int bulletSwitchCount = 0;
    private bool analyticsReady = false;
    private readonly Queue<Action> pendingEvents = new Queue<Action>();

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        isBootstrapped = true;
        DontDestroyOnLoad(gameObject);
        sessionStartTime = Time.time;

        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            analyticsReady = true;
            FlushPendingEvents();
            Debug.Log("Analytics Initialized");
        }
        catch (Exception ex)
        {
            analyticsReady = false;
            Debug.LogWarning($"Analytics initialization failed: {ex.Message}");
        }
    }

    public float GetSessionTime()
    {
        return Time.time - sessionStartTime;
    }

    private void RecordEvent(CustomEvent evt)
    {
        void Record()
        {
            AnalyticsService.Instance.RecordEvent(evt);
            AnalyticsService.Instance.Flush();
        }

        if (analyticsReady)
        {
            Record();
            return;
        }

        pendingEvents.Enqueue(Record);
    }

    private void FlushPendingEvents()
    {
        while (pendingEvents.Count > 0)
        {
            pendingEvents.Dequeue().Invoke();
        }
    }

    public void OnLevelStart()
    {
        CustomEvent evt = new CustomEvent("level_start");
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        RecordEvent(evt);

        Debug.Log("Level Start: " + SceneManager.GetActiveScene().name);
    }

    public void OnPlayerDeath()
    {
        deathCount++;

        CustomEvent evt = new CustomEvent("level_fail");
        evt.Add("death_count", deathCount);
        evt.Add("bullet_used", bulletUsed);
        evt.Add("session_time", GetSessionTime());
        evt.Add("scene_name", SceneManager.GetActiveScene().name);

        RecordEvent(evt);

        Debug.Log($"Player Death | deathCount={deathCount}");
    }

    public void OnLevelFail()
    {
        OnPlayerDeath();
    }

    public void OnLevelComplete()
    {
        winCount++;

        CustomEvent evt = new CustomEvent("level_complete");
        evt.Add("win_count", winCount);
        evt.Add("death_count", deathCount);
        evt.Add("bullet_used", bulletUsed);
        evt.Add("session_time", GetSessionTime());
        evt.Add("scene_name", SceneManager.GetActiveScene().name);

        RecordEvent(evt);

        Debug.Log($"Level Complete | winCount={winCount}");
    }

    public void OnBulletUsed(int bulletIndex, string bulletName, float speed)
    {
        bulletUsed++;

        CustomEvent evt = new CustomEvent("bullet_used");
        evt.Add("bullet_total", bulletUsed);
        evt.Add("bullet_index", bulletIndex);
        evt.Add("bullet_name", bulletName);
        evt.Add("bullet_speed", speed);
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        RecordEvent(evt);

        Debug.Log($"Bullet Used: {bulletName} | total={bulletUsed}");
    }

    public void OnBulletUsed(string bulletName)
    {
        bulletUsed++;

        CustomEvent evt = new CustomEvent("bullet_used");
        evt.Add("bullet_total", bulletUsed);
        evt.Add("bullet_index", -1);
        evt.Add("bullet_name", bulletName);
        evt.Add("bullet_speed", 0f);
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        RecordEvent(evt);

        Debug.Log($"Bullet Used: {bulletName} | total={bulletUsed}");
    }

    public void OnBulletSwitch(int bulletIndex, string bulletName)
    {
        bulletSwitchCount++;

        CustomEvent evt = new CustomEvent("bullet_switch");
        evt.Add("switch_count", bulletSwitchCount);
        evt.Add("bullet_index", bulletIndex);
        evt.Add("bullet_name", bulletName);
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        RecordEvent(evt);

        Debug.Log($"Bullet Switched: {bulletName} | switchCount={bulletSwitchCount}");
    }

    public void OnShootBlocked(string reason)
    {
        CustomEvent evt = new CustomEvent("shoot_blocked");
        evt.Add("reason", reason);
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        RecordEvent(evt);

        Debug.Log($"Shoot Blocked: {reason}");
    }
}
