using UnityEngine;

public static class GameProgress
{
    private const string UnlockedLevelsKey = "progress.unlocked_levels";
    private const string MusicVolumeKey = "settings.music_volume";
    private const string SfxVolumeKey = "settings.sfx_volume";

    public static int GetUnlockedLevels()
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(UnlockedLevelsKey, 1), 1, 3);
    }

    public static void UnlockNextLevel(string sceneName)
    {
        int currentLevel = GetLevelNumber(sceneName);
        if (currentLevel <= 0)
            return;

        int nextUnlocked = Mathf.Clamp(currentLevel + 1, 1, 3);
        if (nextUnlocked > GetUnlockedLevels())
        {
            PlayerPrefs.SetInt(UnlockedLevelsKey, nextUnlocked);
            PlayerPrefs.Save();
        }
    }

    public static bool IsLevelUnlocked(string sceneName)
    {
        int levelNumber = GetLevelNumber(sceneName);
        if (levelNumber <= 0)
            return false;

        return levelNumber <= GetUnlockedLevels();
    }

    public static string GetNextSceneName(string sceneName)
    {
        return sceneName switch
        {
            "Level_1" => "Level_2",
            "Level_2" => "Level_3",
            "Level_3" => "END",
            "Scenesgame" => "END",
            _ => "END"
        };
    }

    public static void SaveCheckpoint(string sceneName, Vector3 position)
    {
        PlayerPrefs.SetFloat(GetCheckpointKey(sceneName, "x"), position.x);
        PlayerPrefs.SetFloat(GetCheckpointKey(sceneName, "y"), position.y);
        PlayerPrefs.SetFloat(GetCheckpointKey(sceneName, "z"), position.z);
        PlayerPrefs.SetInt(GetCheckpointKey(sceneName, "has"), 1);
        PlayerPrefs.Save();
    }

    public static bool TryGetCheckpoint(string sceneName, out Vector3 position)
    {
        if (PlayerPrefs.GetInt(GetCheckpointKey(sceneName, "has"), 0) != 1)
        {
            position = Vector3.zero;
            return false;
        }

        position = new Vector3(
            PlayerPrefs.GetFloat(GetCheckpointKey(sceneName, "x"), 0f),
            PlayerPrefs.GetFloat(GetCheckpointKey(sceneName, "y"), 0f),
            PlayerPrefs.GetFloat(GetCheckpointKey(sceneName, "z"), 0f)
        );

        return true;
    }

    public static void ClearCheckpoint(string sceneName)
    {
        PlayerPrefs.DeleteKey(GetCheckpointKey(sceneName, "x"));
        PlayerPrefs.DeleteKey(GetCheckpointKey(sceneName, "y"));
        PlayerPrefs.DeleteKey(GetCheckpointKey(sceneName, "z"));
        PlayerPrefs.DeleteKey(GetCheckpointKey(sceneName, "has"));
        PlayerPrefs.Save();
    }

    public static float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
    }

    public static void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

    public static float GetSfxVolume()
    {
        return PlayerPrefs.GetFloat(SfxVolumeKey, 0.85f);
    }

    public static void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

    private static int GetLevelNumber(string sceneName)
    {
        return sceneName switch
        {
            "Level_1" => 1,
            "Level_2" => 2,
            "Level_3" => 3,
            _ => -1
        };
    }

    private static string GetCheckpointKey(string sceneName, string suffix)
    {
        return $"checkpoint.{sceneName}.{suffix}";
    }
}
