using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    public enum PowerUpType
    {
        None,
        Hammer,
        Bomb
    }

    // =========================================================
    // BUTTONS
    // =========================================================

    [Header("Power Up Buttons")]
    [SerializeField] private Button hammerButton;
    [SerializeField] private Button shuffleButton;
    [SerializeField] private Button bombButton;
    [SerializeField] private Button movesButton;

    // =========================================================
    // POWER UP COUNTS
    // =========================================================

    [Header("Power Up Counts")]
    [SerializeField] private int hammerCount = 3;
    [SerializeField] private int shuffleCount = 3;
    [SerializeField] private int bombCount = 3;
    [SerializeField] private int movesCount = 3;

    // =========================================================
    // COUNT TEXT
    // =========================================================

    [Header("Count Texts")]
    [SerializeField] private TMP_Text hammerCountText;
    [SerializeField] private TMP_Text shuffleCountText;
    [SerializeField] private TMP_Text bombCountText;
    [SerializeField] private TMP_Text movesCountText;

    // =========================================================
    // LOCK ICONS
    // =========================================================

    [Header("Lock Icons")]
    [SerializeField] private GameObject hammerLockIcon;
    [SerializeField] private GameObject shuffleLockIcon;
    [SerializeField] private GameObject bombLockIcon;
    [SerializeField] private GameObject movesLockIcon;

    // =========================================================
    // LEVEL 5 POPUP
    // =========================================================

    [Header("LEVEL 5 Popup")]
    [SerializeField] private GameObject level5Popup;

    [SerializeField] private float popupShowTime = 2f;

    // =========================================================
    // SOUND
    // =========================================================

    [Header("Power Up Click Sound")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip powerUpClickSound;

    // =========================================================
    // SETTINGS
    // =========================================================

    private const int POWER_UP_UNLOCK_LEVEL = 5;
    private const int USES_PER_LEVEL = 3;

    // =========================================================
    // VARIABLES
    // =========================================================

    private PowerUpType selectedPowerUp = PowerUpType.None;

    private int lastLevel = -1;

    private float popupTimer = 0f;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (level5Popup != null)
        {
            level5Popup.SetActive(false);
        }

        LoadCurrentLevel();

        UpdateCountUI();

        UpdatePowerUpAvailability();
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        LoadCurrentLevel();

        UpdateCountUI();

        UpdatePowerUpAvailability();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        CheckLevelChange();

        // LEVEL 5 popup timer
        if (level5Popup != null && level5Popup.activeSelf)
        {
            popupTimer -= Time.unscaledDeltaTime;

            if (popupTimer <= 0f)
            {
                level5Popup.SetActive(false);
            }
        }
    }

    // =========================================================
    // CHECK LEVEL CHANGE
    // =========================================================

    private void CheckLevelChange()
    {
        if (GameLevelManager.Instance == null)
            return;

        int currentLevel = GameLevelManager.Instance.GetCurrentLevel();

        if (currentLevel != lastLevel)
        {
            LoadCurrentLevel();

            UpdateCountUI();

            UpdatePowerUpAvailability();
        }
    }

    // =========================================================
    // LOAD CURRENT LEVEL
    // =========================================================

    private void LoadCurrentLevel()
    {
        if (GameLevelManager.Instance == null)
            return;

        int currentLevel = GameLevelManager.Instance.GetCurrentLevel();

        lastLevel = currentLevel;

        // Every new level gets 3 power-up uses
        hammerCount = USES_PER_LEVEL;
        shuffleCount = USES_PER_LEVEL;
        bombCount = USES_PER_LEVEL;
        movesCount = USES_PER_LEVEL;

        selectedPowerUp = PowerUpType.None;
    }

    // =========================================================
    // CHECK UNLOCK
    // =========================================================

    private bool ArePowerUpsUnlocked()
    {
        if (GameLevelManager.Instance == null)
            return false;

        int currentLevel = GameLevelManager.Instance.GetCurrentLevel();

        return currentLevel >= POWER_UP_UNLOCK_LEVEL;
    }

    // =========================================================
    // LEVEL 5 POPUP
    // =========================================================

    private void ShowLevel5Popup()
    {
        if (level5Popup == null)
        {
            Debug.LogWarning("Level 5 Popup is not assigned!");
            return;
        }

        level5Popup.SetActive(true);

        popupTimer = popupShowTime;
    }

    // =========================================================
    // UPDATE POWER UP AVAILABILITY
    // =========================================================

    private void UpdatePowerUpAvailability()
    {
        bool unlocked = ArePowerUpsUnlocked();

        // -----------------------------------------------------
        // LOCK ICONS
        // -----------------------------------------------------

        if (hammerLockIcon != null)
        {
            hammerLockIcon.SetActive(!unlocked);
        }

        if (shuffleLockIcon != null)
        {
            shuffleLockIcon.SetActive(!unlocked);
        }

        if (bombLockIcon != null)
        {
            bombLockIcon.SetActive(!unlocked);
        }

        if (movesLockIcon != null)
        {
            movesLockIcon.SetActive(!unlocked);
        }

        // -----------------------------------------------------
        // BUTTON INTERACTABLE
        // -----------------------------------------------------
        //
        // Locked buttons MUST remain interactable,
        // otherwise LEVEL 5 popup cannot open.
        //

        if (hammerButton != null)
        {
            hammerButton.interactable =
                unlocked ? hammerCount > 0 : true;
        }

        if (shuffleButton != null)
        {
            shuffleButton.interactable =
                unlocked ? shuffleCount > 0 : true;
        }

        if (bombButton != null)
        {
            bombButton.interactable =
                unlocked ? bombCount > 0 : true;
        }

        if (movesButton != null)
        {
            movesButton.interactable =
                unlocked ? movesCount > 0 : true;
        }
    }

    // =========================================================
    // SOUND
    // =========================================================

    private void PlayPowerUpClickSound()
    {
        if (audioSource != null && powerUpClickSound != null)
        {
            audioSource.PlayOneShot(powerUpClickSound);
        }
    }

    // =========================================================
    // HAMMER
    // =========================================================

    public void UseHammer()
    {
        // Locked
        if (!ArePowerUpsUnlocked())
        {
            ShowLevel5Popup();
            return;
        }

        // No uses
        if (hammerCount <= 0)
            return;

        // Cancel if already selected
        if (selectedPowerUp == PowerUpType.Hammer)
        {
            CancelPowerUp();
            return;
        }

        PlayPowerUpClickSound();

        selectedPowerUp = PowerUpType.Hammer;
    }

    // =========================================================
    // BOMB
    // =========================================================

    public void UseBomb()
    {
        // Locked
        if (!ArePowerUpsUnlocked())
        {
            ShowLevel5Popup();
            return;
        }

        // No uses
        if (bombCount <= 0)
            return;

        // Cancel if already selected
        if (selectedPowerUp == PowerUpType.Bomb)
        {
            CancelPowerUp();
            return;
        }

        PlayPowerUpClickSound();

        selectedPowerUp = PowerUpType.Bomb;
    }

    // =========================================================
    // SHUFFLE
    // =========================================================

    public void UseShuffle()
    {
        // Locked
        if (!ArePowerUpsUnlocked())
        {
            ShowLevel5Popup();
            return;
        }

        // No uses
        if (shuffleCount <= 0)
            return;

        BoardManager board = FindFirstObjectByType<BoardManager>();

        if (board == null)
        {
            Debug.LogWarning("BoardManager not found!");
            return;
        }

        if (board.ShuffleBoard())
        {
            PlayPowerUpClickSound();

            shuffleCount--;

            UpdateCountUI();

            UpdatePowerUpAvailability();
        }
    }

    // =========================================================
    // EXTRA MOVE
    // =========================================================

    public void UseExtraMove()
    {
        // Locked
        if (!ArePowerUpsUnlocked())
        {
            ShowLevel5Popup();
            return;
        }

        // No uses
        if (movesCount <= 0)
            return;

        BoardManager board = FindFirstObjectByType<BoardManager>();

        if (board == null)
        {
            Debug.LogWarning("BoardManager not found!");
            return;
        }

        if (board.AddExtraMove())
        {
            PlayPowerUpClickSound();

            movesCount--;

            UpdateCountUI();

            UpdatePowerUpAvailability();
        }
    }

    // =========================================================
    // GEM CLICK
    // =========================================================

    public void OnGemClicked(GemView gem)
    {
        if (gem == null)
            return;

        // Should never process power-ups before level 5
        if (!ArePowerUpsUnlocked())
            return;

        BoardManager board = FindFirstObjectByType<BoardManager>();

        if (board == null)
        {
            Debug.LogWarning("BoardManager not found!");
            return;
        }

        // -----------------------------------------------------
        // HAMMER
        // -----------------------------------------------------

        if (selectedPowerUp == PowerUpType.Hammer)
        {
            if (hammerCount > 0 && board.UseHammer(gem))
            {
                PlayPowerUpClickSound();

                hammerCount--;

                selectedPowerUp = PowerUpType.None;

                UpdateCountUI();

                UpdatePowerUpAvailability();
            }

            return;
        }

        // -----------------------------------------------------
        // BOMB
        // -----------------------------------------------------

        if (selectedPowerUp == PowerUpType.Bomb)
        {
            if (bombCount > 0 && board.UseBomb(gem))
            {
                PlayPowerUpClickSound();

                bombCount--;

                selectedPowerUp = PowerUpType.None;

                UpdateCountUI();

                UpdatePowerUpAvailability();
            }

            return;
        }
    }

    // =========================================================
    // CANCEL POWER UP
    // =========================================================

    public void CancelPowerUp()
    {
        selectedPowerUp = PowerUpType.None;
    }

    // =========================================================
    // UPDATE COUNT UI
    // =========================================================

    private void UpdateCountUI()
    {
        if (hammerCountText != null)
        {
            hammerCountText.text = hammerCount.ToString();
        }

        if (shuffleCountText != null)
        {
            shuffleCountText.text = shuffleCount.ToString();
        }

        if (bombCountText != null)
        {
            bombCountText.text = bombCount.ToString();
        }

        if (movesCountText != null)
        {
            movesCountText.text = movesCount.ToString();
        }
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public int GetHammerCount()
    {
        return hammerCount;
    }

    public int GetShuffleCount()
    {
        return shuffleCount;
    }

    public int GetBombCount()
    {
        return bombCount;
    }

    public int GetMovesCount()
    {
        return movesCount;
    }

    public bool IsUnlocked()
    {
        return ArePowerUpsUnlocked();
    }
}