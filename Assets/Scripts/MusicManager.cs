using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip backgroundMusic;

    private const string MusicEnabledKey =
        "MusicEnabled";

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

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (backgroundMusic != null)
        {
            audioSource.clip =
                backgroundMusic;
        }
    }

    private void Start()
    {
        if (IsMusicEnabled())
            PlayMusic();
        else
            StopMusic();
    }

    public void PlayMusic()
    {
        if (audioSource == null)
            return;

        if (backgroundMusic != null)
        {
            audioSource.clip =
                backgroundMusic;
        }

        if (audioSource.clip == null)
            return;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
    }

    public void SetMusic(bool enabled)
    {
        PlayerPrefs.SetInt(
            MusicEnabledKey,
            enabled ? 1 : 0
        );

        PlayerPrefs.Save();

        if (enabled)
            PlayMusic();
        else
            StopMusic();
    }

    public bool IsMusicEnabled()
    {
        return PlayerPrefs.GetInt(
            MusicEnabledKey,
            1
        ) == 1;
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }
}