using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelCompleteUI : MonoBehaviour
{
    // =========================================================
    // PANEL
    // =========================================================

    [Header("Panel")]
    [SerializeField] private GameObject levelCompletePanel;

    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text goalCompletedText;
    [SerializeField] private TMP_Text goalProgressText;

    // =========================================================
    // STARS
    // =========================================================

    [Header("Stars")]
    [SerializeField] private GameObject[] stars;

    // =========================================================
    // NEXT LEVEL BUTTON
    // =========================================================

    [Header("Next Level Button")]
    [SerializeField] private Button nextLevelButton;

    // =========================================================
    // AUDIO
    // =========================================================

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip nextLevelClickSound;

    // =========================================================
    // INTERNAL
    // =========================================================

    private bool isShowing = false;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (levelCompletePanel == null)
        {
            levelCompletePanel = gameObject;
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveListener(
                OnNextLevelClicked
            );

            nextLevelButton.onClick.AddListener(
                OnNextLevelClicked
            );
        }
    }

    // =========================================================
    // START
    // IMPORTANT:
    // DO NOT HIDE PANEL HERE
    // =========================================================

    private void Start()
    {
        // Intentionally empty.
        // Do not call HidePanel() here.
        //
        // BoardManager handles the initial hidden state.
        // When the level is completed, ShowLevelComplete()
        // will activate and display this panel.
    }

    // =========================================================
    // SHOW LEVEL COMPLETE
    // 14 ARGUMENTS
    // =========================================================

    public void ShowLevelComplete(
        int score,
        int moves,

        int redCollected,
        int redTarget,

        int blueCollected,
        int blueTarget,

        int greenCollected,
        int greenTarget,

        int pinkCollected,
        int pinkTarget,

        int purpleCollected,
        int purpleTarget,

        int orangeCollected,
        int orangeTarget)
    {
        if (levelCompletePanel == null)
        {
            levelCompletePanel = gameObject;
        }

        // =====================================================
        // ACTIVATE ALL PARENTS
        // =====================================================

        Transform current =
            levelCompletePanel.transform;

        while (current != null)
        {
            current.gameObject.SetActive(true);

            current = current.parent;
        }

        // =====================================================
        // ACTIVATE PANEL
        // =====================================================

        levelCompletePanel.SetActive(true);

        isShowing = true;

        // =====================================================
        // RECT TRANSFORM RESET
        // =====================================================

        RectTransform rect =
            levelCompletePanel.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.localScale =
                Vector3.one;

            rect.localRotation =
                Quaternion.identity;
        }

        // =====================================================
        // CANVAS GROUP
        // =====================================================

        CanvasGroup canvasGroup =
            levelCompletePanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                levelCompletePanel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // =====================================================
        // BRING TO FRONT
        // =====================================================

        levelCompletePanel.transform.SetAsLastSibling();

        // =====================================================
        // SCORE
        // =====================================================

        if (scoreText != null)
        {
            scoreText.text =
                "SCORE: " +
                score;
        }

        // =====================================================
        // GOAL COUNTS
        // =====================================================

        int completedGoals = 0;
        int totalGoals = 0;

        CheckGoal(
            redCollected,
            redTarget,
            ref completedGoals,
            ref totalGoals
        );

        CheckGoal(
            blueCollected,
            blueTarget,
            ref completedGoals,
            ref totalGoals
        );

        CheckGoal(
            greenCollected,
            greenTarget,
            ref completedGoals,
            ref totalGoals
        );

        CheckGoal(
            pinkCollected,
            pinkTarget,
            ref completedGoals,
            ref totalGoals
        );

        CheckGoal(
            purpleCollected,
            purpleTarget,
            ref completedGoals,
            ref totalGoals
        );

        CheckGoal(
            orangeCollected,
            orangeTarget,
            ref completedGoals,
            ref totalGoals
        );

        // =====================================================
        // GOALS COMPLETED TEXT
        // =====================================================

        if (goalCompletedText != null)
        {
            goalCompletedText.text =
                "GOALS COMPLETED: " +
                completedGoals +
                " / " +
                totalGoals;
        }

        // =====================================================
        // GOAL PROGRESS
        // =====================================================

        if (goalProgressText != null)
        {
            string progress = "";

            AddGoalProgress(
                ref progress,
                redCollected,
                redTarget,
                "RED"
            );

            AddGoalProgress(
                ref progress,
                blueCollected,
                blueTarget,
                "BLUE"
            );

            AddGoalProgress(
                ref progress,
                greenCollected,
                greenTarget,
                "GREEN"
            );

            AddGoalProgress(
                ref progress,
                pinkCollected,
                pinkTarget,
                "PINK"
            );

            AddGoalProgress(
                ref progress,
                purpleCollected,
                purpleTarget,
                "PURPLE"
            );

            AddGoalProgress(
                ref progress,
                orangeCollected,
                orangeTarget,
                "ORANGE"
            );

            goalProgressText.text =
                progress;
        }

        // =====================================================
        // STARS
        // =====================================================

        UpdateStars(
            score,
            moves
        );

        // =====================================================
        // NEXT LEVEL BUTTON
        // =====================================================

        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        // =====================================================
        // DEBUG
        // =====================================================

        Debug.Log(
            "===================================="
        );

        Debug.Log(
            "LEVEL COMPLETE PANEL OPENED!"
        );

        Debug.Log(
            "SCORE = " +
            score
        );

        Debug.Log(
            "MOVES LEFT = " +
            moves
        );

        Debug.Log(
            "GOALS = " +
            completedGoals +
            "/" +
            totalGoals
        );

        Debug.Log(
            "===================================="
        );
    }

    // =========================================================
    // CHECK GOAL
    // =========================================================

    private void CheckGoal(
        int collected,
        int target,
        ref int completed,
        ref int total)
    {
        if (target <= 0)
            return;

        total++;

        if (collected >= target)
        {
            completed++;
        }
    }

    // =========================================================
    // ADD GOAL PROGRESS
    // =========================================================

    private void AddGoalProgress(
        ref string text,
        int collected,
        int target,
        string colorName)
    {
        if (target <= 0)
            return;

        if (!string.IsNullOrEmpty(text))
        {
            text += "\n";
        }

        text +=
            colorName +
            ": " +
            collected +
            " / " +
            target;
    }

    // =========================================================
    // UPDATE STARS
    // =========================================================

    private void UpdateStars(
        int score,
        int moves)
    {
        if (stars == null ||
            stars.Length == 0)
        {
            return;
        }

        // Turn all stars OFF
        for (
            int i = 0;
            i < stars.Length;
            i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(false);
            }
        }

        // =====================================================
        // STAR CALCULATION
        // =====================================================

        int starCount = 1;

        if (score >= 500)
        {
            starCount = 2;
        }

        if (score >= 1000)
        {
            starCount = 3;
        }

        if (moves >= 10 &&
            score >= 300)
        {
            starCount =
                Mathf.Max(
                    starCount,
                    2
                );
        }

        starCount =
            Mathf.Clamp(
                starCount,
                0,
                stars.Length
            );

        // =====================================================
        // TURN STARS ON
        // =====================================================

        for (
            int i = 0;
            i < starCount;
            i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(true);
            }
        }
    }

    // =========================================================
    // HIDE PANEL
    // =========================================================

    public void HidePanel()
    {
        if (levelCompletePanel == null)
        {
            levelCompletePanel = gameObject;
        }

        if (levelCompletePanel == null)
            return;

        isShowing = false;

        // =====================================================
        // CANVAS GROUP
        // =====================================================

        CanvasGroup canvasGroup =
            levelCompletePanel.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // =====================================================
        // STARS OFF
        // =====================================================

        if (stars != null)
        {
            for (
                int i = 0;
                i < stars.Length;
                i++)
            {
                if (stars[i] != null)
                {
                    stars[i].SetActive(false);
                }
            }
        }

        // =====================================================
        // BUTTON DISABLE
        // =====================================================

        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = false;
        }

        // =====================================================
        // PANEL OFF
        // =====================================================

        levelCompletePanel.SetActive(false);

        Debug.Log(
            "LEVEL COMPLETE PANEL HIDDEN"
        );
    }

    // =========================================================
    // NEXT LEVEL
    // =========================================================

    public void OnNextLevelClicked()
    {
        if (!isShowing)
            return;

        // =====================================================
        // SOUND
        // =====================================================

        if (audioSource != null &&
            nextLevelClickSound != null)
        {
            audioSource.PlayOneShot(
                nextLevelClickSound
            );
        }

        // =====================================================
        // GAME LEVEL MANAGER
        // =====================================================

        if (GameLevelManager.Instance == null)
        {
            Debug.LogError(
                "GameLevelManager Instance not found!"
            );

            return;
        }

        Time.timeScale = 1f;

        int currentLevel =
            GameLevelManager.Instance.GetCurrentLevel();

        int nextLevel =
            currentLevel + 1;

        // =====================================================
        // MAX LEVEL
        // =====================================================

        if (nextLevel >
            GameLevelManager.MaxLevel)
        {
            Debug.Log(
                "ALL LEVELS COMPLETED!"
            );

            HidePanel();

            return;
        }

        // =====================================================
        // UNLOCK NEXT LEVEL
        // =====================================================

        GameLevelManager.Instance.UnlockLevel(
            nextLevel
        );

        GameLevelManager.Instance.SetCurrentLevel(
            nextLevel
        );

        PlayerPrefs.Save();

        // =====================================================
        // HIDE PANEL
        // =====================================================

        HidePanel();

        // =====================================================
        // LOAD GAME SCENE
        // =====================================================

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // GET PANEL OBJECT
    // =========================================================

    public GameObject GetPanelObject()
    {
        if (levelCompletePanel == null)
        {
            levelCompletePanel = gameObject;
        }

        return levelCompletePanel;
    }

    // =========================================================
    // IS SHOWING
    // =========================================================

    public bool IsShowing()
    {
        return isShowing;
    }
}