using UnityEngine;
using UnityEngine.UI;

public class HintButtonGlow : MonoBehaviour
{
    [SerializeField] private Shadow glow;

    private float timer;

    void Start()
    {
        if (glow == null)
            glow = GetComponent<Shadow>();

        glow.effectColor = new Color(1f, 0.85f, 0.1f, 0f);
        glow.effectDistance = new Vector2(0f, 0f);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Smooth pulse
        float pulse = (Mathf.Sin(timer * 2.5f) + 1f) / 2f;

        Color c = new Color(
            1f,
            0.85f,
            0.1f,
            Mathf.Lerp(0.05f, 0.35f, pulse)
        );

        glow.effectColor = c;

        float size = Mathf.Lerp(2f, 6f, pulse);
        glow.effectDistance = new Vector2(size, size);
    }
}