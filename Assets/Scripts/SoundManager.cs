using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private const string SoundEnabledKey = "SoundEnabled";

    private bool soundEnabled = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        soundEnabled =
            PlayerPrefs.GetInt(
                SoundEnabledKey,
                1
            ) == 1;
    }

    private void Start()
    {
        ApplySoundToAllSources();
    }

    private void Update()
    {
        // New AudioSources can be created when changing scene
        // or spawning gems/buttons.
        ApplySoundToAllSources();
    }

    public void SetSound(bool enabled)
    {
        soundEnabled = enabled;

        PlayerPrefs.SetInt(
            SoundEnabledKey,
            enabled ? 1 : 0
        );

        PlayerPrefs.Save();

        ApplySoundToAllSources();
    }

    public bool IsSoundEnabled()
    {
        return soundEnabled;
    }

    private void ApplySoundToAllSources()
    {
        AudioSource[] sources =
            FindObjectsByType<AudioSource>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        AudioSource musicSource = null;

        if (MusicManager.Instance != null)
        {
            musicSource =
                MusicManager.Instance.GetAudioSource();
        }

        foreach (AudioSource source in sources)
        {
            if (source == null)
                continue;

            // NEVER mute background music
            if (source == musicSource)
                continue;

            // Everything else = SFX
            source.mute = !soundEnabled;
        }
    }
}