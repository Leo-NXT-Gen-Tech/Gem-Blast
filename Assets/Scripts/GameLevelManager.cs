using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager Instance;

    public const int MaxLevel = 30;

    private const string CURRENT_LEVEL_KEY =
        "CurrentLevel";

    private const string UNLOCKED_LEVEL_KEY =
        "UnlockedLevel";

    private int currentLevel = 1;
    private int unlockedLevel = 1;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        currentLevel =
            PlayerPrefs.GetInt(
                CURRENT_LEVEL_KEY,
                1
            );

        unlockedLevel =
            PlayerPrefs.GetInt(
                UNLOCKED_LEVEL_KEY,
                1
            );

        currentLevel =
            Mathf.Clamp(
                currentLevel,
                1,
                MaxLevel
            );

        unlockedLevel =
            Mathf.Clamp(
                unlockedLevel,
                1,
                MaxLevel
            );
    }

    // =========================================
    // CURRENT LEVEL
    // =========================================

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void SetCurrentLevel(int level)
    {
        level =
            Mathf.Clamp(
                level,
                1,
                MaxLevel
            );

        currentLevel = level;

        PlayerPrefs.SetInt(
            CURRENT_LEVEL_KEY,
            currentLevel
        );

        PlayerPrefs.Save();
    }

    // =========================================
    // UNLOCK
    // =========================================

    public int GetUnlockedLevel()
    {
        return unlockedLevel;
    }

    public void UnlockNextLevel()
    {
        int nextLevel =
            currentLevel + 1;

        if (nextLevel > MaxLevel)
            return;

        if (nextLevel > unlockedLevel)
        {
            unlockedLevel = nextLevel;

            PlayerPrefs.SetInt(
                UNLOCKED_LEVEL_KEY,
                unlockedLevel
            );

            PlayerPrefs.Save();
        }
    }

    public bool IsLevelUnlocked(int level)
    {
        return level <= unlockedLevel;
    }

    // =========================================
    // LEVEL DATA
    // =========================================

    public LevelData GetLevelData(int level)
    {
        level =
            Mathf.Clamp(
                level,
                1,
                MaxLevel
            );

        LevelData data =
            Resources.Load<LevelData>(
                "Levels/Level_" + level
            );

        if (data == null)
        {
            Debug.LogWarning(
                "LevelData not found for Level " +
                level
            );
        }

        return data;
    }

    // =========================================
    // MOVES
    // =========================================

    public int GetMoves(int level)
    {
        LevelData data =
            GetLevelData(level);

        if (data != null)
            return data.moves;

        return 30;
    }

    // =========================================
    // RED GOAL
    // =========================================

    public int GetRedGoal(int level)
    {
        LevelData data =
            GetLevelData(level);

        if (data != null)
            return data.redGoal;

        return 20;
    }

    // =========================================
    // RESET PROGRESS
    // =========================================

    public void ResetProgress()
    {
        currentLevel = 1;
        unlockedLevel = 1;

        PlayerPrefs.SetInt(
            CURRENT_LEVEL_KEY,
            1
        );

        PlayerPrefs.SetInt(
            UNLOCKED_LEVEL_KEY,
            1
        );

        PlayerPrefs.Save();
    }
}