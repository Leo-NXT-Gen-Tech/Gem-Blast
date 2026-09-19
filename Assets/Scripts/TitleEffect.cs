using UnityEngine;
using UnityEngine.UI;

public class TitleEffect : MonoBehaviour
{
    [Header("Title Image")]
    [SerializeField] private Image title;

    [Header("Animation Settings")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float minScale = 0.95f;
    [SerializeField] private float maxScale = 1.05f;

    [Header("Fade Settings")]
    [SerializeField] private float minAlpha = 0.65f;
    [SerializeField] private float maxAlpha = 1f;

    private void Update()
    {
        if (title == null)
            return;

        // Smooth continuous animation
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        // Zoom effect
        float scale = Mathf.Lerp(minScale, maxScale, t);

        title.transform.localScale = Vector3.one * scale;

        // Fade effect
        Color color = title.color;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        title.color = color;
    }
}