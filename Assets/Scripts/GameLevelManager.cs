using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager Instance;

    public const int MaxLevel = 30;

    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    private const string UNLOCKED_LEVEL_KEY = "UnlockedLevel";

    private int currentLevel = 1;
    private int unlockedLevel = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentLevel = PlayerPrefs.GetInt(
            CURRENT_LEVEL_KEY,
            1
        );

        unlockedLevel = PlayerPrefs.GetInt(
            UNLOCKED_LEVEL_KEY,
            1
        );

        currentLevel = Mathf.Clamp(
            currentLevel,
            1,
            MaxLevel
        );

        unlockedLevel = Mathf.Clamp(
            unlockedLevel,
            1,
            MaxLevel
        );
    }

    // =====================================================
    // CURRENT LEVEL
    // =====================================================

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void SetCurrentLevel(int level)
    {
        level = Mathf.Clamp(
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

        Debug.Log(
            "Current Level Set To: " +
            currentLevel
        );
    }

    // =====================================================
    // UNLOCKED LEVEL
    // =====================================================

    public int GetUnlockedLevel()
    {
        return unlockedLevel;
    }

    public void UnlockNextLevel()
    {
        int nextLevel = currentLevel + 1;

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

            Debug.Log(
                "Unlocked Level: " +
                unlockedLevel
            );
        }
    }

    public void UnlockLevel(int level)
    {
        if (level < 1 || level > MaxLevel)
            return;

        if (level > unlockedLevel)
        {
            unlockedLevel = level;

            PlayerPrefs.SetInt(
                UNLOCKED_LEVEL_KEY,
                unlockedLevel
            );

            PlayerPrefs.Save();

            Debug.Log(
                "Unlocked Level: " +
                unlockedLevel
            );
        }
    }

    public bool IsLevelUnlocked(int level)
    {
        return level >= 1 &&
               level <= unlockedLevel;
    }

    // =====================================================
    // LEVEL DATA
    // =====================================================

    public LevelData GetLevelData(int level)
    {
        level = Mathf.Clamp(
            level,
            1,
            MaxLevel
        );

        string path =
            "Levels/Level_" + level;

        LevelData data =
            Resources.Load<LevelData>(path);

        if (data == null)
        {
            Debug.LogError(
                "LevelData NOT FOUND: " +
                path
            );
        }
        else
        {
            Debug.Log(
                "Loaded LevelData: " +
                path
            );
        }

        return data;
    }

    // =====================================================
    // MOVES
    // =====================================================

    public int GetMoves(int level)
    {
        LevelData data =
            GetLevelData(level);

        if (data != null)
            return data.moves;

        return 30;
    }

    // =====================================================
    // RED GOAL
    // =====================================================

    public int GetRedGoal(int level)
    {
        LevelData data =
            GetLevelData(level);

        if (data != null)
            return data.redGoal;

        return 20;
    }

    // =====================================================
    // RESET PROGRESS
    // =====================================================

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

        Debug.Log(
            "Level Progress Reset To Level 1"
        );
    }
}