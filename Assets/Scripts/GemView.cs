using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GemView : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private Image gemImage;

    private GemType gemType;

    private int row;
    private int column;

    private BoardManager boardManager;

    // =========================================================
    // DRAG SETTINGS
    // =========================================================

    [Header("Drag Settings")]

    [SerializeField]
    private float dragThreshold = 50f;

    [SerializeField]
    private float pickupScale = 1.15f;

    [SerializeField]
    private float maxTilt = 8f;

    [SerializeField]
    private float tiltAmount = 0.03f;

    // =========================================================
    // DRAG SOUND
    // =========================================================

    [Header("Drag Sound")]

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip dragSound;

    // =========================================================
    // DRAG VARIABLES
    // =========================================================

    private Vector2 dragStartScreenPosition;

    private Vector2 originalAnchoredPosition;

    private Vector3 originalScale;

    private Quaternion originalRotation;

    private bool isDragging = false;

    // =========================================================
    // RECT TRANSFORM
    // =========================================================

    private RectTransform rectTransform;

    private RectTransform parentRect;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        if (gemImage == null)
        {
            gemImage =
                GetComponentInChildren<Image>(true);
        }

        if (rectTransform != null)
        {
            parentRect =
                rectTransform.parent as RectTransform;
        }

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }
    }

    // =========================================================
    // SET GEM
    // =========================================================

    public void SetGem(
        GemType type,
        int gemRow,
        int gemColumn
    )
    {
        gemType = type;

        row = gemRow;
        column = gemColumn;
    }

    // =========================================================
    // SET BOARD MANAGER
    // =========================================================

    public void SetBoardManager(
        BoardManager manager
    )
    {
        boardManager = manager;
    }

    // =========================================================
    // SET GEM SPRITE
    // =========================================================

    public void SetGemSprite(
        Sprite sprite
    )
    {
        if (gemImage == null)
        {
            gemImage =
                GetComponentInChildren<Image>(true);
        }

        if (gemImage == null)
        {
            Debug.LogError(
                "Gem Image not found on " +
                gameObject.name
            );

            return;
        }

        gemImage.sprite = sprite;

        // Important:
        // Do not tint the actual gem sprite.
        gemImage.color = Color.white;
    }

    // =========================================================
    // SET COLOR
    // =========================================================

    public void SetColor(
        GemType type
    )
    {
        // Kept for compatibility.
        // Actual gem appearance is controlled
        // by the sprite/prefab.

        gemType = type;

        if (gemImage != null)
        {
            gemImage.color = Color.white;
        }
    }

    // =========================================================
    // BEGIN DRAG
    // =========================================================

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        if (boardManager == null)
            return;

        dragStartScreenPosition =
            eventData.position;

        if (rectTransform != null)
        {
            originalAnchoredPosition =
                rectTransform.anchoredPosition;

            originalScale =
                rectTransform.localScale;

            originalRotation =
                rectTransform.localRotation;
        }

        isDragging = true;

        transform.SetAsLastSibling();

        // =====================================================
        // PICKUP EFFECT
        // =====================================================

        if (rectTransform != null)
        {
            rectTransform.localScale =
                originalScale * pickupScale;
        }

        // =====================================================
        // DRAG SOUND
        // =====================================================

        if (
            audioSource != null &&
            dragSound != null
        )
        {
            audioSource.PlayOneShot(
                dragSound
            );
        }
    }

    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(
        PointerEventData eventData
    )
    {
        if (!isDragging)
            return;

        if (rectTransform == null)
            return;

        // =====================================================
        // MOVE GEM
        // =====================================================

        if (parentRect != null)
        {
            Vector2 localPointerPosition;

            if (
                RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    parentRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPointerPosition
                )
            )
            {
                rectTransform.anchoredPosition =
                    localPointerPosition;
            }
        }

        // =====================================================
        // TILT EFFECT
        // =====================================================

        Vector2 drag =
            eventData.position -
            dragStartScreenPosition;

        float tilt = 0f;

        if (
            Mathf.Abs(drag.x) >
            Mathf.Abs(drag.y)
        )
        {
            tilt =
                -drag.x * tiltAmount;

            tilt =
                Mathf.Clamp(
                    tilt,
                    -maxTilt,
                    maxTilt
                );
        }

        rectTransform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                tilt
            );
    }

    // =========================================================
    // END DRAG
    // =========================================================

    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        if (!isDragging)
            return;

        isDragging = false;

        Vector2 drag =
            eventData.position -
            dragStartScreenPosition;

        // =====================================================
        // RESET POSITION
        // =====================================================

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition =
                originalAnchoredPosition;

            rectTransform.localScale =
                originalScale;

            rectTransform.localRotation =
                originalRotation;
        }

        // =====================================================
        // TOO SMALL
        // =====================================================

        if (
            drag.magnitude <
            dragThreshold
        )
        {
            return;
        }

        // =====================================================
        // DIRECTION
        // =====================================================

        Vector2 direction;

        if (
            Mathf.Abs(drag.x) >
            Mathf.Abs(drag.y)
        )
        {
            if (drag.x > 0)
            {
                direction =
                    Vector2.right;
            }
            else
            {
                direction =
                    Vector2.left;
            }
        }
        else
        {
            if (drag.y > 0)
            {
                direction =
                    Vector2.up;
            }
            else
            {
                direction =
                    Vector2.down;
            }
        }

        // =====================================================
        // SEND TO BOARD
        // =====================================================

        if (boardManager != null)
        {
            boardManager.OnGemDragged(
                this,
                direction
            );
        }
    }

    // =========================================================
    // GET GEM TYPE
    // =========================================================

    public GemType GetGemType()
    {
        return gemType;
    }

    // =========================================================
    // GET ROW
    // =========================================================

    public int GetRow()
    {
        return row;
    }

    // =========================================================
    // GET COLUMN
    // =========================================================

    public int GetColumn()
    {
        return column;
    }

    // =========================================================
    // SET GRID POSITION
    // =========================================================

    public void SetGridPosition(
        int gemRow,
        int gemColumn
    )
    {
        row = gemRow;
        column = gemColumn;
    }

    // =========================================================
    // SELECT / DESELECT
    // =========================================================

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