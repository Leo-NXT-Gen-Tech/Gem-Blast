using UnityEngine;
using TMPro;
using System.Text;

public class GoalTextController : MonoBehaviour
{
    [Header("Goal Text")]
    [SerializeField] private TMP_Text goalText;

    [Header("Text Format")]
    [SerializeField] private string prefix = "GOAL: COLLECT ";

    private void Start()
    {
        UpdateGoalText();
    }

    public void UpdateGoalText()
    {
        if (goalText == null)
        {
            Debug.LogWarning("Goal Text is not assigned!");
            return;
        }

        if (GameLevelManager.Instance == null)
        {
            Debug.LogWarning("GameLevelManager not found!");
            return;
        }

        int currentLevel = GameLevelManager.Instance.GetCurrentLevel();
        LevelData levelData = GameLevelManager.Instance.GetLevelData(currentLevel);

        if (levelData == null)
        {
            goalText.text = prefix + "GEMS";
            return;
        }

        StringBuilder goals = new StringBuilder();

        AddGoal(goals, levelData.redGoal, "RED");
        AddGoal(goals, levelData.blueGoal, "BLUE");
        AddGoal(goals, levelData.greenGoal, "GREEN");
        AddGoal(goals, levelData.pinkGoal, "PINK");
        AddGoal(goals, levelData.purpleGoal, "PURPLE");
        AddGoal(goals, levelData.orangeGoal, "ORANGE");

        if (goals.Length == 0)
        {
            goalText.text = prefix + "GEMS";
        }
        else
        {
            goalText.text = prefix + goals.ToString() + " GEMS";
        }
    }

    private void AddGoal(StringBuilder goals, int amount, string colorName)
    {
        if (amount <= 0)
            return;

        if (goals.Length > 0)
            goals.Append(" & ");

        goals.Append(colorName);
    }
}
