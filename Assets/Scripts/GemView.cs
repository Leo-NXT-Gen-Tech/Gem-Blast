using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GemView : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
{
    [SerializeField] private Image gemImage;

    private GemType gemType;

    private int row;
    private int column;

    private BoardManager boardManager;

    [Header("Drag Settings")]
    [SerializeField] private float dragThreshold = 50f;
    [SerializeField] private float pickupScale = 1.15f;
    [SerializeField] private float maxTilt = 8f;
    [SerializeField] private float tiltAmount = 0.03f;

    [Header("Drag Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dragSound;

    private Vector2 dragStartScreenPosition;
    private Vector2 originalAnchoredPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isDragging = false;

    private RectTransform rectTransform;
    private RectTransform parentRect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (gemImage == null)
            gemImage = GetComponentInChildren<Image>(true);

        if (rectTransform != null)
            parentRect = rectTransform.parent as RectTransform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void SetGem(GemType type, int gemRow, int gemColumn)
    {
        gemType = type;
        row = gemRow;
        column = gemColumn;
    }

    public void SetBoardManager(BoardManager manager)
    {
        boardManager = manager;
    }

    public void SetGemSprite(Sprite sprite)
    {
        if (gemImage == null)
            gemImage = GetComponentInChildren<Image>(true);

        if (gemImage == null)
        {
            Debug.LogError("Gem Image not found on " + gameObject.name);
            return;
        }

        gemImage.sprite = sprite;
        gemImage.color = Color.white;
    }

    public void SetColor(GemType type)
    {
        gemType = type;

        if (gemImage != null)
            gemImage.color = Color.white;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (boardManager == null)
            return;

        dragStartScreenPosition = eventData.position;

        if (rectTransform != null)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalScale = rectTransform.localScale;
            originalRotation = rectTransform.localRotation;
        }

        isDragging = true;
        transform.SetAsLastSibling();

        if (rectTransform != null)
            rectTransform.localScale = originalScale * pickupScale;

        if (audioSource != null && dragSound != null)
            audioSource.PlayOneShot(dragSound);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (rectTransform == null)
            return;

        if (parentRect != null)
        {
            Vector2 localPointerPosition;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPointerPosition))
            {
                rectTransform.anchoredPosition = localPointerPosition;
            }
        }

        Vector2 drag = eventData.position - dragStartScreenPosition;
        float tilt = 0f;

        if (Mathf.Abs(drag.x) > Mathf.Abs(drag.y))
        {
            tilt = -drag.x * tiltAmount;
            tilt = Mathf.Clamp(tilt, -maxTilt, maxTilt);
        }

        rectTransform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        Vector2 drag = eventData.position - dragStartScreenPosition;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
            rectTransform.localScale = originalScale;
            rectTransform.localRotation = originalRotation;
        }

        if (drag.magnitude < dragThreshold)
            return;

        Vector2 direction;

        if (Mathf.Abs(drag.x) > Mathf.Abs(drag.y))
            direction = drag.x > 0 ? Vector2.right : Vector2.left;
        else
            direction = drag.y > 0 ? Vector2.up : Vector2.down;

        if (boardManager != null)
            boardManager.OnGemDragged(this, direction);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (PowerUpManager.Instance == null)
            return;

        PowerUpManager.Instance.OnGemClicked(this);
    }

    public GemType GetGemType()
    {
        return gemType;
    }

    public int GetRow()
    {
        return row;
    }

    public int GetColumn()
    {
        return column;
    }

    public void SetGridPosition(int gemRow, int gemColumn)
    {
        row = gemRow;
        column = gemColumn;
    }

    public void Select()
    {
    }

    public void Deselect()
    {
    }

    public bool IsSelected()
    {
        return false;
    }
}
