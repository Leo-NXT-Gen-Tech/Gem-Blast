using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Stars")]
    [SerializeField] private GameObject star1;
    [SerializeField] private GameObject star2;
    [SerializeField] private GameObject star3;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Goal")]
    [SerializeField] private TMP_Text goalCompletedText;
    [SerializeField] private TMP_Text goalProgressText;

    [Header("Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        // Panel OFF when game starts
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        // Button listeners
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
    }

    // =========================================================
    // SHOW LEVEL COMPLETE
    // =========================================================

    public void ShowLevelComplete(
        int score,
        int movesRemaining,
        int redGemsCollected,
        int targetRedGems)
    {
        if (levelCompletePanel == null)
            return;

        levelCompletePanel.SetActive(true);

        // Score
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        // Goal completed
        if (goalCompletedText != null)
        {
            goalCompletedText.text = "✓ GOAL COMPLETED!";
        }

        // Goal progress
        if (goalProgressText != null)
        {
            goalProgressText.text =
                redGemsCollected + " / " +
                targetRedGems +
                " RED GEMS";
        }

        // Calculate stars
        int stars = CalculateStars(movesRemaining);

        ShowStars(stars);

        Debug.Log(
            "LEVEL COMPLETE | Score: " +
            score +
            " | Moves Left: " +
            movesRemaining +
            " | Stars: " +
            stars
        );
    }

    // =========================================================
    // STAR SYSTEM
    // =========================================================

    private int CalculateStars(int movesRemaining)
    {
        /*
         * Starting moves = 30
         *
         * 15 or more remaining = 3 Stars
         * 8 - 14 remaining     = 2 Stars
         * 1 - 7 remaining      = 1 Star
         */

        if (movesRemaining >= 15)
            return 3;

        if (movesRemaining >= 8)
            return 2;

        return 1;
    }

    private void ShowStars(int starCount)
    {
        if (star1 != null)
            star1.SetActive(starCount >= 1);

        if (star2 != null)
            star2.SetActive(starCount >= 2);

        if (star3 != null)
            star3.SetActive(starCount >= 3);
    }

    // =========================================================
    // NEXT LEVEL
    // =========================================================

    public void OnNextLevelClicked()
    {
        if (GameLevelManager.Instance == null)
            return;

        GameLevelManager.Instance.UnlockNextLevel();

        int nextLevel =
            GameLevelManager.Instance.GetCurrentLevel() + 1;

        if (nextLevel > GameLevelManager.MaxLevel)
        {
            Debug.Log("ALL 30 LEVELS COMPLETED!");
            return;
        }

        GameLevelManager.Instance.SetCurrentLevel(nextLevel);

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // RETRY
    // =========================================================

    private void OnRetryClicked()
    {
        Debug.Log("RETRY BUTTON CLICKED");

        // Reload current scene
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // HIDE PANEL
    // =========================================================

    public void HidePanel()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }
}