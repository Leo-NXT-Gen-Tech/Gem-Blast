using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager Instance;

    public const int MaxLevel = 30;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -----------------------------
    // CURRENT LEVEL
    // -----------------------------

    public int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    public void SetCurrentLevel(int level)
    {
        level = Mathf.Clamp(level, 1, MaxLevel);

        PlayerPrefs.SetInt("CurrentLevel", level);
        PlayerPrefs.Save();
    }

    // -----------------------------
    // UNLOCKED LEVEL
    // -----------------------------

    public int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt("UnlockedLevel", 1);
    }

    public void UnlockNextLevel()
    {
        int currentLevel = GetCurrentLevel();
        int unlockedLevel = GetUnlockedLevel();

        if (currentLevel + 1 > unlockedLevel &&
            currentLevel < MaxLevel)
        {
            PlayerPrefs.SetInt(
                "UnlockedLevel",
                currentLevel + 1
            );

            PlayerPrefs.Save();
        }
    }

    public bool IsLevelUnlocked(int level)
    {
        return level <= GetUnlockedLevel();
    }

    // -----------------------------
    // LEVEL MOVES
    // -----------------------------

    public int GetMoves(int level)
    {
        switch (level)
        {
            case 1: return 30;
            case 2: return 30;
            case 3: return 30;
            case 4: return 30;
            case 5: return 29;

            case 6: return 28;
            case 7: return 28;
            case 8: return 28;
            case 9: return 27;
            case 10: return 27;

            case 11: return 26;
            case 12: return 26;
            case 13: return 26;
            case 14: return 25;
            case 15: return 25;

            case 16: return 25;
            case 17: return 24;
            case 18: return 24;
            case 19: return 24;
            case 20: return 23;

            case 21: return 23;
            case 22: return 23;
            case 23: return 22;
            case 24: return 22;
            case 25: return 22;

            case 26: return 21;
            case 27: return 21;
            case 28: return 20;
            case 29: return 20;
            case 30: return 20;
        }

        return 30;
    }

    // -----------------------------
    // RED GEM GOAL
    // -----------------------------

    public int GetRedGoal(int level)
    {
        switch (level)
        {
            case 1: return 20;
            case 2: return 20;
            case 3: return 22;
            case 4: return 22;
            case 5: return 25;

            case 6: return 25;
            case 7: return 27;
            case 8: return 27;
            case 9: return 30;
            case 10: return 30;

            case 11: return 30;
            case 12: return 32;
            case 13: return 32;
            case 14: return 35;
            case 15: return 35;

            case 16: return 35;
            case 17: return 37;
            case 18: return 38;
            case 19: return 40;
            case 20: return 40;

            case 21: return 40;
            case 22: return 42;
            case 23: return 42;
            case 24: return 45;
            case 25: return 45;

            case 26: return 45;
            case 27: return 48;
            case 28: return 48;
            case 29: return 50;
            case 30: return 50;
        }

        return 20;
    }
}