using UnityEngine;
using System.Collections;

public class SplashGemLoop : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector2 moveOffset = new Vector2(0, 80f);
    [SerializeField] private float moveDuration = 1.2f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 25f;

    [Header("Timing")]
    [SerializeField] private float delay = 0f;

    [Header("Sound")]
    [SerializeField] private AudioSource bgMusicSource;
    [SerializeField] private AudioClip bgMusic;
    [SerializeField] private AudioClip playButtonClickSound;

    private RectTransform rect;
    private Vector2 originalPosition;
    private bool isPlaying = true;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPosition = rect.anchoredPosition;
    }

    private void Start()
    {
        // Start Background Music
        if (bgMusicSource != null && bgMusic != null)
        {
            bgMusicSource.clip = bgMusic;
            bgMusicSource.loop = true;
            bgMusicSource.Play();
        }

        StartCoroutine(AnimateGem());
    }

    private IEnumerator AnimateGem()
    {
        yield return new WaitForSeconds(delay);

        while (isPlaying)
        {
            Vector2 outsidePosition = originalPosition + moveOffset;

            yield return MoveGem(
                originalPosition,
                outsidePosition
            );

            if (!isPlaying)
                yield break;

            yield return MoveGem(
                outsidePosition,
                originalPosition
            );
        }
    }

    private IEnumerator MoveGem(Vector2 from, Vector2 to)
    {
        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;

            float t = time / moveDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            rect.anchoredPosition = Vector2.Lerp(
                from,
                to,
                t
            );

            rect.Rotate(
                0f,
                0f,
                rotationSpeed * Time.deltaTime
            );

            yield return null;
        }

        rect.anchoredPosition = to;
    }

    // PLAY BUTTON માટે આ function લગાવવો
    public void PlayButtonPressed()
    {
        // Play button click sound
        if (bgMusicSource != null && playButtonClickSound != null)
        {
            bgMusicSource.PlayOneShot(playButtonClickSound);
        }

        // Background music તરત બંધ
        if (bgMusicSource != null)
        {
            bgMusicSource.Stop();
        }

        // Gem animation stop
        StopAnimation();
    }

    public void StopAnimation()
    {
        isPlaying = false;
        StopAllCoroutines();
    }
}