using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SwitchToggle : MonoBehaviour
{
    public enum SettingType
    {
        Music,
        Sound
    }

    [Header("Setting Type")]
    [SerializeField] private SettingType settingType;

    [Header("Toggle")]
    [SerializeField] private Toggle toggle;

    [Header("Handle")]
    [SerializeField] private RectTransform handle;

    [Header("Handle Positions")]
    [Tooltip("ON = LEFT")]
    [SerializeField] private Vector2 leftOnPosition = new Vector2(-35f, 0f);

    [Tooltip("OFF = RIGHT")]
    [SerializeField] private Vector2 rightOffPosition = new Vector2(35f, 0f);

    [Header("Editor Preview")]
    [SerializeField] private bool previewOn = true;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.18f;

    private Coroutine moveCoroutine;

    private void Awake()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (toggle == null)
        {
            Debug.LogError(
                "SwitchToggle: Toggle component missing on " +
                gameObject.name
            );
            return;
        }

        if (handle == null)
        {
            Debug.LogError(
                "SwitchToggle: Handle not assigned on " +
                gameObject.name
            );
            return;
        }

        // IMPORTANT:
        // Only this script handles toggle change.
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
        toggle.onValueChanged.AddListener(OnToggleChanged);

        Refresh();
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (handle == null)
            return;

        if (!Application.isPlaying)
        {
            handle.anchoredPosition =
                previewOn
                    ? leftOnPosition
                    : rightOffPosition;
        }
    }

#endif

    private void OnToggleChanged(bool isOn)
    {
        if (handle == null)
            return;

        // ON = LEFT
        // OFF = RIGHT
        Vector2 target =
            isOn
                ? leftOnPosition
                : rightOffPosition;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(
            AnimateHandle(target)
        );

        SettingsPanel settingsPanel =
            FindFirstObjectByType<SettingsPanel>(
                FindObjectsInactive.Include
            );

        if (settingsPanel == null)
            return;

        // MUSIC
        if (settingType == SettingType.Music)
        {
            settingsPanel.SetMusic(isOn);
        }

        // SOUND
        else if (settingType == SettingType.Sound)
        {
            settingsPanel.SetSound(isOn);
        }
    }

    private IEnumerator AnimateHandle(Vector2 target)
    {
        Vector2 start = handle.anchoredPosition;

        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = time / slideDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            handle.anchoredPosition =
                Vector2.Lerp(start, target, t);

            yield return null;
        }

        handle.anchoredPosition = target;

        moveCoroutine = null;
    }

    public void Refresh()
    {
        if (toggle == null || handle == null)
            return;

        handle.anchoredPosition =
            toggle.isOn
                ? leftOnPosition
                : rightOffPosition;
    }

    private void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(
                OnToggleChanged
            );
        }
    }
}