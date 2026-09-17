using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Music Toggle")]
    [SerializeField] private Toggle musicToggle;

    [Header("Sound Toggle")]
    [SerializeField] private Toggle soundToggle;

    [Header("UI Sounds")]
    [SerializeField] private AudioSource uiAudioSource;

    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip toggleSound;
    [SerializeField] private AudioClip closeSound;

    private const string MusicKey = "MusicEnabled";
    private const string SoundKey = "SoundEnabled";

    private bool loadingSettings = false;

    private void Awake()
    {
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
        }

        // Settings panel start ma hidden
        settingsPanel.SetActive(false);
    }

    private void Start()
    {
        LoadSettings();
    }

    // =========================================================
    // OPEN SETTINGS
    // =========================================================

    public void OpenSettings()
    {
        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(true);

        LoadSettings();

        PlayUISound(openSound);
    }

    // =========================================================
    // CLOSE SETTINGS
    // =========================================================

    public void CloseSettings()
    {
        PlayUISound(closeSound);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // =========================================================
    // MUSIC ON / OFF
    // ONLY BACKGROUND MUSIC
    // =========================================================

    public void SetMusic(bool enabled)
    {
        if (loadingSettings)
            return;

        PlayerPrefs.SetInt(
            MusicKey,
            enabled ? 1 : 0
        );

        PlayerPrefs.Save();

        // ONLY MUSIC MANAGER
        if (MusicManager.Instance != null)
        {
            if (enabled)
            {
                MusicManager.Instance.PlayMusic();
            }
            else
            {
                MusicManager.Instance.StopMusic();
            }
        }

        // Toggle click sound
        PlayUISound(toggleSound);

        RefreshHandles();
    }

    // =========================================================
    // SOUND ON / OFF
    // ONLY GAME SFX
    // MUSIC NEVER TOUCHED
    // =========================================================

    public void SetSound(bool enabled)
    {
        if (loadingSettings)
            return;

        PlayerPrefs.SetInt(
            SoundKey,
            enabled ? 1 : 0
        );

        PlayerPrefs.Save();

        // ONLY SOUND MANAGER
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSound(enabled);
        }

        // Toggle click sound
        PlayUISound(toggleSound);

        RefreshHandles();
    }

    // =========================================================
    // LOAD SAVED SETTINGS
    // =========================================================

    private void LoadSettings()
    {
        loadingSettings = true;

        bool musicEnabled =
            PlayerPrefs.GetInt(
                MusicKey,
                1
            ) == 1;

        bool soundEnabled =
            PlayerPrefs.GetInt(
                SoundKey,
                1
            ) == 1;

        // -----------------------------------------------------
        // MUSIC TOGGLE
        // -----------------------------------------------------

        if (musicToggle != null)
        {
            musicToggle.SetIsOnWithoutNotify(
                musicEnabled
            );
        }

        // -----------------------------------------------------
        // SOUND TOGGLE
        // -----------------------------------------------------

        if (soundToggle != null)
        {
            soundToggle.SetIsOnWithoutNotify(
                soundEnabled
            );
        }

        // -----------------------------------------------------
        // APPLY MUSIC
        // -----------------------------------------------------

        if (MusicManager.Instance != null)
        {
            if (musicEnabled)
            {
                MusicManager.Instance.PlayMusic();
            }
            else
            {
                MusicManager.Instance.StopMusic();
            }
        }

        // -----------------------------------------------------
        // APPLY SOUND
        // -----------------------------------------------------

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSound(
                soundEnabled
            );
        }

        loadingSettings = false;

        // Update handles
        RefreshHandles();
    }

    // =========================================================
    // REFRESH SWITCH HANDLES
    // ON  = LEFT
    // OFF = RIGHT
    // =========================================================

    private void RefreshHandles()
    {
        // MUSIC HANDLE
        if (musicToggle != null)
        {
            SwitchToggle switchToggle =
                musicToggle.GetComponent<SwitchToggle>();

            if (switchToggle != null)
            {
                switchToggle.Refresh();
            }
        }

        // SOUND HANDLE
        if (soundToggle != null)
        {
            SwitchToggle switchToggle =
                soundToggle.GetComponent<SwitchToggle>();

            if (switchToggle != null)
            {
                switchToggle.Refresh();
            }
        }
    }

    // =========================================================
    // UI SOUND
    // =========================================================

    private void PlayUISound(AudioClip clip)
    {
        if (uiAudioSource == null)
            return;

        if (clip == null)
            return;

        // Sound OFF hoy to UI click sound pan OFF
        bool soundEnabled =
            PlayerPrefs.GetInt(
                SoundKey,
                1
            ) == 1;

        if (!soundEnabled)
            return;

        uiAudioSource.PlayOneShot(clip);
    }
}