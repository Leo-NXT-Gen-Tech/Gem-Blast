using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FailPanel : MonoBehaviour
{
    public static FailPanel Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Fail Panel UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text goalProgressText;
    [SerializeField] private TMP_Text movesText;

    [Header("Fail Emoji Animation")]
    [SerializeField] private RectTransform failEmoji;
    [SerializeField] private float emojiScale = 1.12f;
    [SerializeField] private float emojiRotation = 8f;
    [SerializeField] private float animationDuration = 0.22f;
    [SerializeField] private int animationCycles = 3;

    [Header("Fail Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelFailedSound;

    [Header("Button Click Sounds")]
    [SerializeField] private AudioClip retryClickSound;
    [SerializeField] private AudioClip mainMenuClickSound;

    [Header("Fail Fade Animation")]
    [SerializeField] private CanvasGroup failTitleCanvasGroup;
    [SerializeField] private CanvasGroup goalNotCompletedCanvasGroup;
    [SerializeField] private float fadeDelay = 0.7f;
    [SerializeField] private float fadeDuration = 1.2f;

    private Coroutine emojiCoroutine;
    private Coroutine fadeCoroutine;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        Instance = this;

        if (panel == null)
            panel = gameObject;

        // Keep script GameObject active.
        // Only actual panel is disabled.
        if (panel != gameObject)
            panel.SetActive(false);

        ResetFailEmojiAnimation();
        ResetFailFade();
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // =========================================================
    // SHOW FAIL PANEL
    // =========================================================

    public void Show(
        int score,

        int redGemsCollected,
        int targetRedGems,

        int blueGemsCollected,
        int targetBlueGems,

        int greenGemsCollected,
        int targetGreenGems,

        int pinkGemsCollected,
        int targetPinkGems,

        int purpleGemsCollected,
        int targetPurpleGems,

        int orangeGemsCollected,
        int targetOrangeGems,

        int movesUsed)
    {
        if (panel == null)
            panel = gameObject;

        // =====================================================
        // SCORE
        // =====================================================

        if (scoreText != null)
            scoreText.text = "SCORE " + score;

        // =====================================================
        // GOAL PROGRESS
        // =====================================================

        if (goalProgressText != null)
        {
            StringBuilder goals = new StringBuilder();

            AddGoalLine(
                goals,
                redGemsCollected,
                targetRedGems,
                "RED"
            );

            AddGoalLine(
                goals,
                blueGemsCollected,
                targetBlueGems,
                "BLUE"
            );

            AddGoalLine(
                goals,
                greenGemsCollected,
                targetGreenGems,
                "GREEN"
            );

            AddGoalLine(
                goals,
                pinkGemsCollected,
                targetPinkGems,
                "PINK"
            );

            AddGoalLine(
                goals,
                purpleGemsCollected,
                targetPurpleGems,
                "PURPLE"
            );

            AddGoalLine(
                goals,
                orangeGemsCollected,
                targetOrangeGems,
                "ORANGE"
            );

            if (goals.Length == 0)
                goalProgressText.text = "GOAL COMPLETED!";
            else
                goalProgressText.text = goals.ToString();
        }

        // =====================================================
        // MOVES USED
        // =====================================================

        if (movesText != null)
            movesText.text = "MOVES USED: " + movesUsed;

        // =====================================================
        // SHOW PANEL
        // =====================================================

        panel.SetActive(true);

        ResetFailEmojiAnimation();
        ResetFailFade();

        // =====================================================
        // FAIL SOUND
        // =====================================================

        if (
            audioSource != null &&
            levelFailedSound != null
        )
        {
            audioSource.PlayOneShot(
                levelFailedSound
            );
        }

        // =====================================================
        // ANIMATIONS
        // =====================================================

        StartFailEmojiAnimation();
        StartFailFadeAnimation();
    }

    // =========================================================
    // ADD GOAL LINE
    // =========================================================

    private void AddGoalLine(
        StringBuilder goals,
        int collected,
        int target,
        string colorName)
    {
        // Goal 0 = don't show
        if (target <= 0)
            return;

        if (goals.Length > 0)
            goals.Append("\n");

        goals.Append(collected);
        goals.Append(" / ");
        goals.Append(target);
        goals.Append(" ");
        goals.Append(colorName);
        goals.Append(" GEMS");
    }

    // =========================================================
    // FAIL EMOJI
    // =========================================================

    private void ResetFailEmojiAnimation()
    {
        if (failEmoji == null)
            return;

        failEmoji.localScale = Vector3.one;
        failEmoji.localRotation = Quaternion.identity;
    }

    private void StartFailEmojiAnimation()
    {
        if (failEmoji == null)
            return;

        if (!isActiveAndEnabled)
        {
            Debug.LogWarning(
                "FailPanel GameObject is inactive. Emoji animation cannot start."
            );

            return;
        }

        if (emojiCoroutine != null)
        {
            StopCoroutine(
                emojiCoroutine
            );
        }

        emojiCoroutine =
            StartCoroutine(
                AnimateFailEmoji()
            );
    }

    private IEnumerator AnimateFailEmoji()
    {
        float duration =
            Mathf.Max(
                0.05f,
                animationDuration
            );

        int cycles =
            Mathf.Max(
                1,
                animationCycles
            );

        Vector3 baseScale = Vector3.one;

        failEmoji.localScale = baseScale;
        failEmoji.localRotation = Quaternion.identity;

        for (
            int cycle = 0;
            cycle < cycles;
            cycle++
        )
        {
            float timer = 0f;

            // SCALE UP + ROTATE RIGHT
            while (timer < duration)
            {
                if (failEmoji == null)
                    yield break;

                timer += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        timer / duration
                    );

                float eased =
                    1f -
                    Mathf.Pow(
                        1f - t,
                        3f
                    );

                failEmoji.localScale =
                    Vector3.Lerp(
                        baseScale,
                        baseScale * emojiScale,
                        eased
                    );

                float angle =
                    Mathf.Lerp(
                        0f,
                        emojiRotation,
                        eased
                    );

                failEmoji.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        angle
                    );

                yield return null;
            }

            // ROTATE LEFT
            timer = 0f;

            while (timer < duration)
            {
                if (failEmoji == null)
                    yield break;

                timer += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        timer / duration
                    );

                float eased =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );

                failEmoji.localScale =
                    Vector3.Lerp(
                        baseScale * emojiScale,
                        baseScale,
                        eased
                    );

                float angle =
                    Mathf.Lerp(
                        emojiRotation,
                        -emojiRotation,
                        eased
                    );

                failEmoji.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        angle
                    );

                yield return null;
            }

            // RETURN NORMAL
            timer = 0f;

            while (timer < duration)
            {
                if (failEmoji == null)
                    yield break;

                timer += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        timer / duration
                    );

                float eased =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );

                failEmoji.localScale = baseScale;

                float angle =
                    Mathf.Lerp(
                        -emojiRotation,
                        0f,
                        eased
                    );

                failEmoji.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        angle
                    );

                yield return null;
            }
        }

        ResetFailEmojiAnimation();
        emojiCoroutine = null;
    }

    // =========================================================
    // FAIL FADE
    // =========================================================

    private void ResetFailFade()
    {
        if (failTitleCanvasGroup != null)
        {
            failTitleCanvasGroup.alpha = 1f;
            failTitleCanvasGroup.interactable = true;
            failTitleCanvasGroup.blocksRaycasts = true;
        }

        if (goalNotCompletedCanvasGroup != null)
        {
            goalNotCompletedCanvasGroup.alpha = 1f;
            goalNotCompletedCanvasGroup.interactable = true;
            goalNotCompletedCanvasGroup.blocksRaycasts = true;
        }
    }

    private void StartFailFadeAnimation()
    {
        if (!isActiveAndEnabled)
            return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(
                fadeCoroutine
            );
        }

        fadeCoroutine =
            StartCoroutine(
                FadeFailTexts()
            );
    }

    private IEnumerator FadeFailTexts()
    {
        yield return new WaitForSecondsRealtime(
            fadeDelay
        );

        while (true)
        {
            float timer = 0f;

            // FADE OUT
            while (timer < fadeDuration)
            {
                if (!isActiveAndEnabled)
                    yield break;

                timer += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        timer / fadeDuration
                    );

                float alpha =
                    Mathf.SmoothStep(
                        1f,
                        0f,
                        t
                    );

                if (failTitleCanvasGroup != null)
                    failTitleCanvasGroup.alpha = alpha;

                if (goalNotCompletedCanvasGroup != null)
                    goalNotCompletedCanvasGroup.alpha = alpha;

                yield return null;
            }

            // FADE IN
            timer = 0f;

            while (timer < fadeDuration)
            {
                if (!isActiveAndEnabled)
                    yield break;

                timer += Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        timer / fadeDuration
                    );

                float alpha =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );

                if (failTitleCanvasGroup != null)
                    failTitleCanvasGroup.alpha = alpha;

                if (goalNotCompletedCanvasGroup != null)
                    goalNotCompletedCanvasGroup.alpha = alpha;

                yield return null;
            }
        }
    }

    // =========================================================
    // BUTTON CLICK SOUND
    // =========================================================

    private void PlayClickSound(
        AudioClip clip)
    {
        if (
            audioSource != null &&
            clip != null
        )
        {
            audioSource.PlayOneShot(
                clip
            );
        }
    }

    // =========================================================
    // RETRY
    // =========================================================

    public void RetryLevel()
    {
        Time.timeScale = 1f;

        PlayClickSound(
            retryClickSound
        );

        Debug.Log(
            "RETRY CURRENT LEVEL"
        );

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    // =========================================================
    // MAIN MENU
    // =========================================================

    public void MainMenu()
    {
        Time.timeScale = 1f;

        PlayClickSound(
            mainMenuClickSound
        );

        Debug.Log(
            "RETURN TO MAIN MENU"
        );

        SceneManager.LoadScene(0);
    }
}