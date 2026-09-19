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
        StartCoroutine(AnimateGem());
    }

    private IEnumerator AnimateGem()
    {
        yield return new WaitForSeconds(delay);

        while (isPlaying)
        {
            // બહાર જવું
            Vector2 outsidePosition = originalPosition + moveOffset;

            yield return MoveGem(
                originalPosition,
                outsidePosition
            );

            if (!isPlaying)
                yield break;

            // પાછું આવવું
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

            // Smooth movement
            t = Mathf.SmoothStep(0f, 1f, t);

            rect.anchoredPosition = Vector2.Lerp(
                from,
                to,
                t
            );

            // Continuous rotation
            rect.Rotate(
                0f,
                0f,
                rotationSpeed * Time.deltaTime
            );

            yield return null;
        }

        rect.anchoredPosition = to;
    }

    public void StopAnimation()
    {
        isPlaying = false;
        StopAllCoroutines();
    }
}