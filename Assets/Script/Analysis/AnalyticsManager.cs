using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    private float sessionStartTime;
    private int deathCount = 0;
    private int winCount = 0;
    private int bulletUsed = 0;
    private int bulletSwitchCount = 0;

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();

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

    // -------------------------
    // LEVEL START
    // -------------------------
    public void OnLevelStart()
    {
        CustomEvent evt = new CustomEvent("level_start");
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log("Level Start: " + SceneManager.GetActiveScene().name);
    }

    // -------------------------
    // PLAYER DEATH / FAIL
    // -------------------------
    public void OnPlayerDeath()
    {
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

    // เผื่อโค้ดเก่ายังเรียกชื่อนี้
    public void OnLevelFail()
    {
        OnPlayerDeath();
    }

    // -------------------------
    // LEVEL COMPLETE
    // -------------------------
    public void OnLevelComplete()
    {
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

    // -------------------------
    // BULLET USED
    // -------------------------
    // แบบละเอียด สำหรับสคริปต์ยิง
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

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log($"Bullet Used: {bulletName} | total={bulletUsed}");
    }

    // แบบง่าย เผื่อ PlayerController ยังส่งมาแค่ type เดียว
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

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log($"Bullet Used: {bulletName} | total={bulletUsed}");
    }

    // -------------------------
    // BULLET SWITCH
    // -------------------------
    public void OnBulletSwitch(int bulletIndex, string bulletName)
    {
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

    // -------------------------
    // SHOOT BLOCKED
    // -------------------------
    public void OnShootBlocked(string reason)
    {
        CustomEvent evt = new CustomEvent("shoot_blocked");
        evt.Add("reason", reason);
        evt.Add("scene_name", SceneManager.GetActiveScene().name);
        evt.Add("session_time", GetSessionTime());

        AnalyticsService.Instance.RecordEvent(evt);
        AnalyticsService.Instance.Flush();

        Debug.Log($"Shoot Blocked: {reason}");
    }
}