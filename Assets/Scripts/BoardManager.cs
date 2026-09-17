using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BoardManager : MonoBehaviour
{
    [Header("Default Board Size")]
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;

    [Header("Gem Setup")]
    [SerializeField] private GameObject redGemPrefab;
    [SerializeField] private GameObject blueGemPrefab;
    [SerializeField] private GameObject greenGemPrefab;
    [SerializeField] private GameObject pinkGemPrefab;
    [SerializeField] private GameObject purpleGemPrefab;
    [SerializeField] private GameObject orangeGemPrefab;

    [SerializeField] private Transform boardPanel;

    [Header("Animation")]
    [SerializeField] private float fallDuration = 0.25f;
    [SerializeField] private float swapDuration = 0.20f;

    [Header("Candy Blast Effect")]
    [SerializeField] private int blastPieces = 9;
    [SerializeField] private float blastDuration = 0.32f;
    [SerializeField] private float blastPieceSize = 0.18f;
    [SerializeField] private float blastForce = 110f;
    [SerializeField] private float blastGravity = 260f;
    [SerializeField] private float blastRotationSpeed = 420f;
    [SerializeField] private AudioSource matchAudioSource;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip newGemDropSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private ParticleSystem winnerParticleEffect;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text goalProgressText;
    [SerializeField] private TMP_Text levelText;

    [Header("Game Settings")]
    [SerializeField] private int startingMoves = 30;

    [Header("Goals")]
    [SerializeField] private int targetRedGems = 20;
    [SerializeField] private int targetBlueGems = 0;
    [SerializeField] private int targetGreenGems = 0;
    [SerializeField] private int targetPinkGems = 0;
    [SerializeField] private int targetPurpleGems = 0;
    [SerializeField] private int targetOrangeGems = 0;

    [Header("Result Panels")]
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Level Complete UI")]
    [SerializeField] private LevelCompleteUI levelCompleteUI;

    [Header("Hint Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hintSound;

    // =========================================================
    // HINT SYSTEM
    // =========================================================

    [Header("Hint System")]
    [SerializeField] private float hintScale = 1.12f;
    [SerializeField] private float hintRotation = 6f;
    [SerializeField] private float hintAnimDuration = 0.30f;
    [SerializeField] private int hintCycles = 2;

    private float hintTimer = 0f;

    private Coroutine hintCoroutine;

    private GemView hintGem;
    private GemView hintGem2;

    private Vector3 hintOriginalScale;
    private Vector3 hintOriginalScale2;

    private Quaternion hintOriginalRotation;
    private Quaternion hintOriginalRotation2;

    private bool hintShowing = false;

    // =========================================================
    // HINT MOVE DATA
    // =========================================================

    private struct HintMove
    {
        public int row1;
        public int column1;
        public int row2;
        public int column2;

        public HintMove(
            int r1,
            int c1,
            int r2,
            int c2)
        {
            row1 = r1;
            column1 = c1;
            row2 = r2;
            column2 = c2;
        }
    }

    // =========================================================
    // BOARD DATA
    // =========================================================

    private GemType[,] board;
    private GemView[,] gemViews;
    private bool[,] activeCells;

    private bool isProcessing = false;
    private bool isDestroyed = false;
    private bool levelCompleted = false;
    private bool levelFailed = false;
    private bool isLoadingNextLevel = false;

    private float cellWidth;
    private float cellHeight;
    private float gemSize;

    private int score = 0;
    private int moves;

    // =========================================================
    // COLLECTED GOALS
    // =========================================================

    private int redGemsCollected = 0;
    private int blueGemsCollected = 0;
    private int greenGemsCollected = 0;
    private int pinkGemsCollected = 0;
    private int purpleGemsCollected = 0;
    private int orangeGemsCollected = 0;

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
    if (isDestroyed)
        return;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (isDestroyed)
            return;

        Time.timeScale = 1f;

        // =====================================================
        // AUTO FIND LEVEL COMPLETE UI
        // =====================================================

        AutoFindLevelCompleteUI();

        if (matchAudioSource == null)
        {
            matchAudioSource =
                GetComponent<AudioSource>();

            if (matchAudioSource == null)
            {
                matchAudioSource =
                    gameObject.AddComponent<AudioSource>();
            }
        }

        int currentLevel = 1;

        if (GameLevelManager.Instance != null)
        {
            currentLevel =
                GameLevelManager.Instance.GetCurrentLevel();

            startingMoves =
                GameLevelManager.Instance.GetMoves(
                    currentLevel
                );

            LevelData startLevelData =
                GameLevelManager.Instance.GetLevelData(
                    currentLevel
                );

            if (startLevelData != null)
            {
                targetRedGems =
                    Mathf.Max(
                        0,
                        startLevelData.redGoal
                    );

                targetBlueGems =
                    Mathf.Max(
                        0,
                        startLevelData.blueGoal
                    );

                targetGreenGems =
                    Mathf.Max(
                        0,
                        startLevelData.greenGoal
                    );

                targetPinkGems =
                    Mathf.Max(
                        0,
                        startLevelData.pinkGoal
                    );

                targetPurpleGems =
                    Mathf.Max(
                        0,
                        startLevelData.purpleGoal
                    );

                targetOrangeGems =
                    Mathf.Max(
                        0,
                        startLevelData.orangeGoal
                    );
            }
        }

        Debug.Log(
            "===================================="
        );

        Debug.Log(
            "START LEVEL: " +
            currentLevel
        );

        Debug.Log(
            "MOVES: " +
            startingMoves
        );

        Debug.Log(
            "RED GOAL: " +
            targetRedGems
        );

        Debug.Log(
            "BLUE GOAL: " +
            targetBlueGems
        );

        Debug.Log(
            "GREEN GOAL: " +
            targetGreenGems
        );

        Debug.Log(
            "PINK GOAL: " +
            targetPinkGems
        );

        Debug.Log(
            "PURPLE GOAL: " +
            targetPurpleGems
        );

        Debug.Log(
            "ORANGE GOAL: " +
            targetOrangeGems
        );

        moves = startingMoves;
        score = 0;

        redGemsCollected = 0;
        blueGemsCollected = 0;
        greenGemsCollected = 0;
        pinkGemsCollected = 0;
        purpleGemsCollected = 0;
        orangeGemsCollected = 0;

        levelCompleted = false;
        levelFailed = false;
        isLoadingNextLevel = false;

        hintTimer = 0f;
        hintShowing = false;
        hintGem = null;
        hintCoroutine = null;

        // =====================================================
        // HIDE COMPLETE PANEL AT START
        // =====================================================

        if (levelCompleteUI != null)
        {
            levelCompleteUI.HidePanel();
        }
        else if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        if (winnerParticleEffect != null)
        {
            winnerParticleEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            winnerParticleEffect.gameObject.SetActive(
                false
            );
        }

        bool savedLevelLoaded =
            LoadSavedLevel(
                currentLevel
            );

        if (!savedLevelLoaded)
        {
            InitializeBoard();
            GenerateRandomBoard();
        }

        GenerateVisualBoard();

        UpdateUI();

        PrintBoard();

        Debug.Log(
            "FINAL BOARD SIZE: " +
            rows +
            " x " +
            columns
        );

        Debug.Log(
            "===================================="
        );
    }

    // =========================================================
    // AUTO FIND LEVEL COMPLETE UI
    // =========================================================

    private void AutoFindLevelCompleteUI()
    {
        if (levelCompleteUI == null)
        {
            levelCompleteUI =
                FindFirstObjectByType<LevelCompleteUI>(
                    FindObjectsInactive.Include
                );
        }

        if (
            levelCompletePanel == null &&
            levelCompleteUI != null
        )
        {
            levelCompletePanel =
                levelCompleteUI.GetPanelObject();
        }

        if (levelCompleteUI == null)
        {
            Debug.LogWarning(
                "LevelCompleteUI could not be found automatically."
            );
        }

        if (levelCompletePanel == null)
        {
            Debug.LogWarning(
                "Level Complete Panel could not be found automatically."
            );
        }
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        isDestroyed = true;

        StopAllCoroutines();
    }

    // =========================================================
    // TEST WIN
    // =========================================================

    [ContextMenu("TEST WIN LEVEL")]
    public void TestWinLevel()
    {
        if (isDestroyed)
            return;

        if (levelCompleted)
            return;

        Debug.Log(
            "===================================="
        );

        Debug.Log(
            "TEST WIN LEVEL ACTIVATED"
        );

        // =====================================================
        // COMPLETE ALL ACTIVE GOALS
        // =====================================================

        if (targetRedGems > 0)
        {
            redGemsCollected =
                targetRedGems;
        }

        if (targetBlueGems > 0)
        {
            blueGemsCollected =
                targetBlueGems;
        }

        if (targetGreenGems > 0)
        {
            greenGemsCollected =
                targetGreenGems;
        }

        if (targetPinkGems > 0)
        {
            pinkGemsCollected =
                targetPinkGems;
        }

        if (targetPurpleGems > 0)
        {
            purpleGemsCollected =
                targetPurpleGems;
        }

        if (targetOrangeGems > 0)
        {
            orangeGemsCollected =
                targetOrangeGems;
        }

        UpdateUI();

        // =====================================================
        // OPEN LEVEL COMPLETE
        // =====================================================

        ShowLevelComplete();

        Debug.Log(
            "TEST WIN COMPLETE"
        );

        Debug.Log(
            "===================================="
        );
    }

    // =========================================================
    // INITIALIZE BOARD
    // =========================================================

    private void InitializeBoard()
    {
        rows =
            Mathf.Max(
                1,
                rows
            );

        columns =
            Mathf.Max(
                1,
                columns
            );

        board =
            new GemType[
                rows,
                columns
            ];

        gemViews =
            new GemView[
                rows,
                columns
            ];

        activeCells =
            new bool[
                rows,
                columns
            ];

        for (
            int row = 0;
            row < rows;
            row++
        )
        {
            for (
                int column = 0;
                column < columns;
                column++
            )
            {
                activeCells[
                    row,
                    column
                ] = true;

                board[
                    row,
                    column
                ] = GemType.Red;
            }
        }
    }

    // =========================================================
    // RANDOM GEM
    // =========================================================

    private GemType GetRandomGem()
    {
        return (GemType)Random.Range(
            0,
            6
        );
    }

    // =========================================================
    // GET GEM PREFAB
    // =========================================================

    private GameObject GetGemPrefab(
        GemType type
    )
    {
        switch (type)
        {
            case GemType.Red:
                return redGemPrefab;

            case GemType.Blue:
                return blueGemPrefab;

            case GemType.Green:
                return greenGemPrefab;

            case GemType.Pink:
                return pinkGemPrefab;

            case GemType.Purple:
                return purpleGemPrefab;

            case GemType.Orange:
                return orangeGemPrefab;
        }

        return null;
    }

    // =========================================================
    // GET GEM SPRITE
    // =========================================================

    private Sprite GetGemSprite(
        GemType type
    )
    {
        GameObject prefab =
            GetGemPrefab(
                type
            );

        if (prefab == null)
        {
            Debug.LogError(
                "Prefab not assigned for GemType: " +
                type
            );

            return null;
        }

        Image image =
            prefab.GetComponentInChildren<Image>(
                true
            );

        if (image == null)
        {
            Debug.LogError(
                "Image not found inside prefab: " +
                prefab.name
            );

            return null;
        }

        return image.sprite;
    }

    // =========================================================
    // RANDOM BOARD
    // =========================================================

    private void GenerateRandomBoard()
    {
        for (
            int row = 0;
            row < rows;
            row++
        )
        {
            for (
                int column = 0;
                column < columns;
                column++
            )
            {
                if (
                    !activeCells[
                        row,
                        column
                    ]
                )
                {
                    board[
                        row,
                        column
                    ] =
                        GemType.Red;

                    continue;
                }

                board[
                    row,
                    column
                ] =
                    GetRandomGem();
            }
        }

        Debug.Log(
            rows +
            "x" +
            columns +
            " RANDOM BOARD GENERATED!"
        );
    }

    // =========================================================
    // LOAD LEVEL
    // =========================================================

    private bool LoadSavedLevel(
        int level
    )
    {
        if (GameLevelManager.Instance == null)
        {
            Debug.LogWarning(
                "GameLevelManager not found!"
            );

            return false;
        }

        LevelData levelData =
            GameLevelManager.Instance.GetLevelData(
                level
            );

        if (levelData == null)
        {
            Debug.LogWarning(
                "LevelData not found for Level " +
                level +
                ". Using random board."
            );

            return false;
        }

        rows =
            Mathf.Max(
                1,
                levelData.rows
            );

        columns =
            Mathf.Max(
                1,
                levelData.columns
            );

        InitializeBoard();

        if (
            levelData.gems == null ||
            levelData.activeCells == null
        )
        {
            Debug.LogWarning(
                "LevelData arrays are missing."
            );

            return false;
        }

        int expectedSize =
            rows *
            columns;

        if (
            levelData.gems.Length != expectedSize ||
            levelData.activeCells.Length != expectedSize
        )
        {
            Debug.LogWarning(
                "LevelData array size does not match board size."
            );

            return false;
        }

        for (
            int row = 0;
            row < rows;
            row++
        )
        {
            for (
                int column = 0;
                column < columns;
                column++
            )
            {
                board[
                    row,
                    column
                ] =
                    levelData.GetGem(
                        row,
                        column
                    );

                activeCells[
                    row,
                    column
                ] =
                    levelData.IsValidCell(
                        row,
                        column
                    );
            }
        }

        Debug.Log(
            "LEVEL " +
            level +
            " LOADED FROM LEVEL DATA"
        );

        return true;
    }

    // =========================================================
    // GEM SHORT NAME
    // =========================================================

    private string GetGemShortName(
        GemType type
    )
    {
        switch (type)
        {
            case GemType.Red:
                return "R";

            case GemType.Blue:
                return "B";

            case GemType.Green:
                return "G";

            case GemType.Pink:
                return "P";

            case GemType.Purple:
                return "Pu";

            case GemType.Orange:
                return "O";
        }

        return "?";
    }

    // =========================================================
    // GENERATE VISUAL BOARD
    // =========================================================

    private void GenerateVisualBoard()
    {
        if (boardPanel == null)
        {
            Debug.LogError(
                "Board Panel is not assigned!"
            );

            return;
        }

        for (
            int i = boardPanel.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                boardPanel.GetChild(i).gameObject
            );
        }

        RectTransform panelRect =
            boardPanel.GetComponent<RectTransform>();

        if (panelRect == null)
        {
            Debug.LogError(
                "Board Panel needs RectTransform!"
            );

            return;
        }

        float panelWidth =
            panelRect.rect.width;

        float panelHeight =
            panelRect.rect.height;

        cellWidth =
            panelWidth /
            columns;

        cellHeight =
            panelHeight /
            rows;

        gemSize =
            Mathf.Min(
                cellWidth,
                cellHeight
            ) *
            0.85f;

        int createdCount = 0;

        for (
            int row = 0;
            row < rows;
            row++
        )
        {
            for (
                int column = 0;
                column < columns;
                column++
            )
            {
                if (
                    !activeCells[
                        row,
                        column
                    ]
                )
                {
                    continue;
                }

                GemView gem =
                    CreateGem(
                        row,
                        column,
                        false
                    );

                if (gem != null)
                    createdCount++;
            }
        }

        Debug.Log(
            "VISUAL BOARD CREATED: " +
            createdCount +
            " ACTIVE GEMS"
        );
    }

    // =========================================================
    // CREATE GEM
    // =========================================================

    private GemView CreateGem(
        int row,
        int column,
        bool startAboveBoard
    )
    {
        if (isDestroyed)
            return null;

        if (!IsActiveCell(
                row,
                column
            ))
        {
            return null;
        }

        GemType currentType =
            board[
                row,
                column
            ];

        GameObject selectedPrefab =
            GetGemPrefab(
                currentType
            );

        if (selectedPrefab == null)
        {
            Debug.LogError(
                "Gem prefab is not assigned for: " +
                currentType
            );

            return null;
        }

        GameObject gemObject =
            Instantiate(
                selectedPrefab,
                boardPanel
            );

        RectTransform gemRect =
            gemObject.GetComponent<RectTransform>();

        if (gemRect != null)
        {
            gemRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            gemRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            gemRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            gemRect.sizeDelta =
                new Vector2(
                    gemSize,
                    gemSize
                );

            Vector2 targetPosition =
                GetGridPosition(
                    row,
                    column
                );

            if (startAboveBoard)
            {
                targetPosition.y +=
                    cellHeight * rows;
            }

            gemRect.anchoredPosition =
                targetPosition;
        }

        GemView gemView =
            gemObject.GetComponent<GemView>();

        if (gemView == null)
        {
            Debug.LogError(
                "GemView missing on prefab: " +
                selectedPrefab.name
            );

            Destroy(
                gemObject
            );

            return null;
        }

        gemView.SetGem(
            currentType,
            row,
            column
        );

        Sprite sprite =
            GetGemSprite(
                currentType
            );

        if (sprite != null)
        {
            gemView.SetGemSprite(
                sprite
            );
        }

        gemView.SetBoardManager(
            this
        );

        gemViews[
            row,
            column
        ] =
            gemView;

        return gemView;
    }

    // =========================================================
    // GRID POSITION
    // =========================================================

    private Vector2 GetGridPosition(
        int row,
        int column
    )
    {
        float x =
            (
                column -
                (columns - 1) / 2f
            ) *
            cellWidth;

        float y =
            (
                row -
                (rows - 1) / 2f
            ) *
            cellHeight;

        return new Vector2(
            x,
            y
        );
    }

    // =========================================================
    // GEM DRAG
    // =========================================================

    public void OnGemDragged(
        GemView draggedGem,
        Vector2 direction
    )
    {
        if (isDestroyed)
            return;

        StopHint();

        if (isProcessing)
            return;

        if (levelCompleted)
            return;

        if (levelFailed)
            return;

        if (draggedGem == null)
            return;

        if (moves <= 0)
            return;

        int currentRow =
            draggedGem.GetRow();

        int currentColumn =
            draggedGem.GetColumn();

        int targetRow =
            currentRow;

        int targetColumn =
            currentColumn;

        if (direction == Vector2.left)
        {
            targetColumn--;
        }
        else if (direction == Vector2.right)
        {
            targetColumn++;
        }
        else if (direction == Vector2.up)
        {
            targetRow++;
        }
        else if (direction == Vector2.down)
        {
            targetRow--;
        }

        if (
            !IsActiveCell(
                targetRow,
                targetColumn
            )
        )
        {
            return;
        }

        GemView targetGem =
            gemViews[
                targetRow,
                targetColumn
            ];

        if (targetGem == null)
            return;

        StartCoroutine(
            SwapAndProcess(
                draggedGem,
                targetGem
            )
        );
    }

    // =========================================================
    // STOP HINT
    // =========================================================

    private void StopHint()
    {
    hintTimer = 0f;

    if (hintCoroutine != null)
    {
        StopCoroutine(
            hintCoroutine
        );

        hintCoroutine = null;
    }

    if (hintGem != null)
    {
        Transform hintTransform =
            hintGem.transform;

        hintTransform.localScale =
            hintOriginalScale;

        hintTransform.localRotation =
            hintOriginalRotation;
    }

    if (hintGem2 != null)
    {
        Transform hintTransform2 =
            hintGem2.transform;

        hintTransform2.localScale =
            hintOriginalScale2;

        hintTransform2.localRotation =
            hintOriginalRotation2;
    }

    hintGem = null;
    hintGem2 = null;
    hintShowing = false;
    }

    // =========================================================
    // FIND VALID HINT MOVE
    // =========================================================

    private bool FindValidHintMove(
        out HintMove validMove
    )
    {
        validMove =
            new HintMove();

        if (
            board == null ||
            gemViews == null
        )
        {
            return false;
        }

        for (
            int row = 0;
            row < rows;
            row++
        )
        {
            for (
                int column = 0;
                column < columns;
                column++
            )
            {
                if (!IsActiveCell(
                        row,
                        column
                    ))
                {
                    continue;
                }

                if (
                    gemViews[
                        row,
                        column
                    ] == null
                )
                {
                    continue;
                }

                int rightColumn =
                    column + 1;

                if (
                    rightColumn < columns &&
                    IsActiveCell(
                        row,
                        rightColumn
                    )
                )
                {
                    if (
                        CreatesMatchAfterSwap(
                            row,
                            column,
                            row,
                            rightColumn
                        )
                    )
                    {
                        validMove =
                            new HintMove(
                                row,
                                column,
                                row,
                                rightColumn
                            );

                        return true;
                    }
                }

                int upRow =
                    row + 1;

                if (
                    upRow < rows &&
                    IsActiveCell(
                        upRow,
                        column
                    )
                )
                {
                    if (
                        CreatesMatchAfterSwap(
                            row,
                            column,
                            upRow,
                            column
                        )
                    )
                    {
                        validMove =
                            new HintMove(
                                row,
                                column,
                                upRow,
                                column
                            );

                        return true;
                    }
                }
            }
        }

        return false;
    }

    // =========================================================
    // CHECK SWAP MATCH
    // =========================================================

    private bool CreatesMatchAfterSwap(
        int row1,
        int column1,
        int row2,
        int column2
    )
    {
        if (
            !IsActiveCell(
                row1,
                column1
            ) ||
            !IsActiveCell(
                row2,
                column2
            )
        )
        {
            return false;
        }

        GemType type1 =
            board[
                row1,
                column1
            ];

        GemType type2 =
            board[
                row2,
                column2
            ];

        board[
            row1,
            column1
        ] =
            type2;

        board[
            row2,
            column2
        ] =
            type1;

        List<Vector2Int> matches =
            FindAllMatches();

        bool createsMatch =
            matches != null &&
            matches.Count >= 3;

        board[
            row1,
            column1
        ] =
            type1;

        board[
            row2,
            column2
        ] =
            type2;

        return createsMatch;
    }

    // =========================================================
    // GET GEM VIEW
    // =========================================================

    private GemView GetGemView(
        int row,
        int column
    )
    {
        if (gemViews == null)
            return null;

        if (
            row < 0 ||
            row >= rows ||
            column < 0 ||
            column >= columns
        )
        {
            return null;
        }

        return gemViews[
            row,
            column
        ];
    }

    // =========================================================
    // HINT ANIMATION
    // =========================================================

    private IEnumerator AnimateHintPair(
    GemView gem1,
    GemView gem2,
    Image image1,
    Image image2,
    Color originalColor1,
    Color originalColor2
    )
    {
    float timer = 0f;

    while (
        timer < hintAnimDuration
    )
    {
        if (isDestroyed)
            yield break;

        timer += Time.deltaTime;

        float t =
            Mathf.Clamp01(
                timer /
                hintAnimDuration
            );

        float smooth =
            Mathf.Sin(
                t * Mathf.PI
            );

        float scale =
            Mathf.Lerp(
                1f,
                hintScale,
                smooth
            );

        float rotation =
            smooth *
            hintRotation;

        gem1.transform.localScale =
            hintOriginalScale *
            scale;

        gem2.transform.localScale =
            hintOriginalScale2 *
            scale;

        gem1.transform.localRotation =
            hintOriginalRotation *
            Quaternion.Euler(
                0f,
                0f,
                rotation
            );

        gem2.transform.localRotation =
            hintOriginalRotation2 *
            Quaternion.Euler(
                0f,
                0f,
                -rotation
            );

        bool blinkRed =
            Mathf.Sin(
                timer * 22f
            ) > 0f;

        if (image1 != null)
        {
            image1.color =
                blinkRed
                ? Color.red
                : originalColor1;
        }

        if (image2 != null)
        {
            image2.color =
                blinkRed
                ? Color.red
                : originalColor2;
        }

        yield return null;
    }
   }
    // =========================================================
    // SWAP AND PROCESS
    // =========================================================

    private IEnumerator SwapAndProcess(
        GemView gem1,
        GemView gem2
    )
    {
        if (
            isDestroyed ||
            isProcessing ||
            gem1 == null ||
            gem2 == null
        )
        {
            yield break;
        }

        StopHint();

        isProcessing = true;

        int row1 =
            gem1.GetRow();

        int column1 =
            gem1.GetColumn();

        int row2 =
            gem2.GetRow();

        int column2 =
            gem2.GetColumn();

        if (
            !IsActiveCell(
                row1,
                column1
            ) ||
            !IsActiveCell(
                row2,
                column2
            )
        )
        {
            isProcessing = false;
            yield break;
        }

        GemType type1 =
            board[
                row1,
                column1
            ];

        GemType type2 =
            board[
                row2,
                column2
            ];

        RectTransform rect1 =
            gem1.GetComponent<RectTransform>();

        RectTransform rect2 =
            gem2.GetComponent<RectTransform>();

        Vector2 position1 =
            GetGridPosition(
                row1,
                column1
            );

        Vector2 position2 =
            GetGridPosition(
                row2,
                column2
            );

        moves--;

        UpdateUI();

        board[
            row1,
            column1
        ] =
            type2;

        board[
            row2,
            column2
        ] =
            type1;

        gemViews[
            row1,
            column1
        ] =
            gem2;

        gemViews[
            row2,
            column2
        ] =
            gem1;

        gem1.SetGridPosition(
            row2,
            column2
        );

        gem2.SetGridPosition(
            row1,
            column1
        );

        gem1.SetColor(type2);

        gem1.SetGemSprite(
            GetGemSprite(type2)
        );

        gem2.SetColor(type1);

        gem2.SetGemSprite(
            GetGemSprite(type1)
        );

        yield return StartCoroutine(
            AnimateGemSwap(
                rect1,
                position1,
                position2,
                rect2,
                position2,
                position1
            )
        );

        if (isDestroyed)
            yield break;

        List<Vector2Int> matches =
            FindAllMatches();

        if (matches.Count > 0)
        {
            int points;

            if (matches.Count == 3)
                points = 30;
            else if (matches.Count == 4)
                points = 50;
            else
                points = 100;

            AddScore(points);

            yield return StartCoroutine(
                RemoveAndFill(
                    matches
                )
            );
        }
        else
        {
            board[
                row1,
                column1
            ] =
                type1;

            board[
                row2,
                column2
            ] =
                type2;

            gemViews[
                row1,
                column1
            ] =
                gem1;

            gemViews[
                row2,
                column2
            ] =
                gem2;

            gem1.SetGridPosition(
                row1,
                column1
            );

            gem2.SetGridPosition(
                row2,
                column2
            );

            gem1.SetColor(type1);

            gem1.SetGemSprite(
                GetGemSprite(type1)
            );

            gem2.SetColor(type2);

            gem2.SetGemSprite(
                GetGemSprite(type2)
            );

            yield return StartCoroutine(
                AnimateGemSwap(
                    rect1,
                    position2,
                    position1,
                    rect2,
                    position1,
                    position2
                )
            );
        }

        if (
            !levelCompleted &&
            moves <= 0
        )
        {
            ShowLevelFailed();
        }

        hintTimer = 0f;

        isProcessing = false;
    }

    // =========================================================
    // SWAP ANIMATION
    // =========================================================

    private IEnumerator AnimateGemSwap(
        RectTransform gem1Rect,
        Vector2 gem1Start,
        Vector2 gem1Target,
        RectTransform gem2Rect,
        Vector2 gem2Start,
        Vector2 gem2Target
    )
    {
        if (
            gem1Rect == null ||
            gem2Rect == null
        )
        {
            yield break;
        }

        float timer = 0f;

        while (
            timer <
            swapDuration
        )
        {
            if (isDestroyed)
                yield break;

            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    swapDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            gem1Rect.anchoredPosition =
                Vector2.Lerp(
                    gem1Start,
                    gem1Target,
                    smoothT
                );

            gem2Rect.anchoredPosition =
                Vector2.Lerp(
                    gem2Start,
                    gem2Target,
                    smoothT
                );

            yield return null;
        }

        if (gem1Rect != null)
        {
            gem1Rect.anchoredPosition =
                gem1Target;
        }

        if (gem2Rect != null)
        {
            gem2Rect.anchoredPosition =
                gem2Target;
        }
    }

    // =========================================================
    // FIND MATCHES
    // =========================================================

    private List<Vector2Int> FindAllMatches()
    {
        List<Vector2Int> matches =
            new List<Vector2Int>();

        // =====================================================
        // HORIZONTAL
        // =====================================================

        for (
            int row = 0;
            row < rows;
            row++
        )
        {
            int column = 0;

            while (
                column < columns
            )
            {
                if (
                    !IsActiveCell(
                        row,
                        column
                    )
                )
                {
                    column++;
                    continue;
                }

                GemType type =
                    board[
                        row,
                        column
                    ];

                int startColumn =
                    column;

                int count = 1;

                column++;

                while (
                    column < columns &&
                    IsActiveCell(
                        row,
                        column
                    ) &&
                    board[
                        row,
                        column
                    ] == type
                )
                {
                    count++;
                    column++;
                }

                if (count >= 3)
                {
                    for (
                        int c = startColumn;
                        c < column;
                        c++
                    )
                    {
                        Vector2Int position =
                            new Vector2Int(
                                row,
                                c
                            );

                        if (
                            !matches.Contains(
                                position
                            )
                        )
                        {
                            matches.Add(
                                position
                            );
                        }
                    }
                }
            }
        }

        // =====================================================
        // VERTICAL
        // =====================================================

        for (
            int column = 0;
            column < columns;
            column++
        )
        {
            int row = 0;

            while (
                row < rows
            )
            {
                if (
                    !IsActiveCell(
                        row,
                        column
                    )
                )
                {
                    row++;
                    continue;
                }

                GemType type =
                    board[
                        row,
                        column
                    ];

                int startRow =
                    row;

                int count = 1;

                row++;

                while (
                    row < rows &&
                    IsActiveCell(
                        row,
                        column
                    ) &&
                    board[
                        row,
                        column
                    ] == type
                )
                {
                    count++;
                    row++;
                }

                if (count >= 3)
                {
                    for (
                        int r = startRow;
                        r < row;
                        r++
                    )
                    {
                        Vector2Int position =
                            new Vector2Int(
                                r,
                                column
                            );

                        if (
                            !matches.Contains(
                                position
                            )
                        )
                        {
                            matches.Add(
                                position
                            );
                        }
                    }
                }
            }
        }

        return matches;
    }

    // =========================================================
    // REMOVE AND FILL
    // =========================================================

    private IEnumerator RemoveAndFill(
        List<Vector2Int> matches
    )
    {
        if (isDestroyed)
            yield break;

        int redCountThisMatch = 0;
        int blueCountThisMatch = 0;
        int greenCountThisMatch = 0;
        int pinkCountThisMatch = 0;
        int purpleCountThisMatch = 0;
        int orangeCountThisMatch = 0;

        foreach (
            Vector2Int position
            in matches
        )
        {
            int row =
                position.x;

            int column =
                position.y;

            if (
                !IsActiveCell(
                    row,
                    column
                )
            )
            {
                continue;
            }

            switch (
                board[
                    row,
                    column
                ]
            )
            {
                case GemType.Red:
                    redCountThisMatch++;
                    break;

                case GemType.Blue:
                    blueCountThisMatch++;
                    break;

                case GemType.Green:
                    greenCountThisMatch++;
                    break;

                case GemType.Pink:
                    pinkCountThisMatch++;
                    break;

                case GemType.Purple:
                    purpleCountThisMatch++;
                    break;

                case GemType.Orange:
                    orangeCountThisMatch++;
                    break;
            }
        }

        AddCollectedGems(
            redCountThisMatch,
            blueCountThisMatch,
            greenCountThisMatch,
            pinkCountThisMatch,
            purpleCountThisMatch,
            orangeCountThisMatch
        );

        yield return StartCoroutine(
            BlastMatchedGems(
                matches
            )
        );

        yield return new WaitForSeconds(
            0.05f
        );

        for (
            int column = 0;
            column < columns;
            column++
        )
        {
            if (isDestroyed)
                yield break;

            yield return StartCoroutine(
                CollapseColumn(
                    column
                )
            );
        }

        yield return new WaitForSeconds(
            fallDuration
        );

        if (isDestroyed)
            yield break;

        // =====================================================
        // AUTOMATIC LEVEL COMPLETE
        // =====================================================

        if (AreAllGoalsCompleted())
        {
            ShowLevelComplete();

            yield break;
        }

        List<Vector2Int> newMatches =
            FindAllMatches();

        if (newMatches.Count > 0)
        {
            int chainPoints;

            if (newMatches.Count == 3)
                chainPoints = 30;
            else if (newMatches.Count == 4)
                chainPoints = 50;
            else
                chainPoints = 100;

            AddScore(
                chainPoints
            );

            yield return StartCoroutine(
                RemoveAndFill(
                    newMatches
                )
            );
        }
    }

    // =========================================================
    // ADD COLLECTED GEMS
    // =========================================================

    private void AddCollectedGems(
        int red,
        int blue,
        int green,
        int pink,
        int purple,
        int orange
    )
    {
        redGemsCollected += red;
        blueGemsCollected += blue;
        greenGemsCollected += green;
        pinkGemsCollected += pink;
        purpleGemsCollected += purple;
        orangeGemsCollected += orange;

        if (targetRedGems > 0)
        {
            redGemsCollected =
                Mathf.Min(
                    redGemsCollected,
                    targetRedGems
                );
        }

        if (targetBlueGems > 0)
        {
            blueGemsCollected =
                Mathf.Min(
                    blueGemsCollected,
                    targetBlueGems
                );
        }

        if (targetGreenGems > 0)
        {
            greenGemsCollected =
                Mathf.Min(
                    greenGemsCollected,
                    targetGreenGems
                );
        }

        if (targetPinkGems > 0)
        {
            pinkGemsCollected =
                Mathf.Min(
                    pinkGemsCollected,
                    targetPinkGems
                );
        }

        if (targetPurpleGems > 0)
        {
            purpleGemsCollected =
                Mathf.Min(
                    purpleGemsCollected,
                    targetPurpleGems
                );
        }

        if (targetOrangeGems > 0)
        {
            orangeGemsCollected =
                Mathf.Min(
                    orangeGemsCollected,
                    targetOrangeGems
                );
        }

        UpdateUI();

        Debug.Log(
            "GOALS | " +
            "RED " +
            redGemsCollected +
            "/" +
            targetRedGems +
            " | BLUE " +
            blueGemsCollected +
            "/" +
            targetBlueGems +
            " | GREEN " +
            greenGemsCollected +
            "/" +
            targetGreenGems +
            " | PINK " +
            pinkGemsCollected +
            "/" +
            targetPinkGems +
            " | PURPLE " +
            purpleGemsCollected +
            "/" +
            targetPurpleGems +
            " | ORANGE " +
            orangeGemsCollected +
            "/" +
            targetOrangeGems
        );
    }

    // =========================================================
    // ALL GOALS COMPLETED
    // =========================================================

    private bool AreAllGoalsCompleted()
    {
        bool hasGoal = false;

        if (targetRedGems > 0)
        {
            hasGoal = true;

            if (
                redGemsCollected <
                targetRedGems
            )
            {
                return false;
            }
        }

        if (targetBlueGems > 0)
        {
            hasGoal = true;

            if (
                blueGemsCollected <
                targetBlueGems
            )
            {
                return false;
            }
        }

        if (targetGreenGems > 0)
        {
            hasGoal = true;

            if (
                greenGemsCollected <
                targetGreenGems
            )
            {
                return false;
            }
        }

        if (targetPinkGems > 0)
        {
            hasGoal = true;

            if (
                pinkGemsCollected <
                targetPinkGems
            )
            {
                return false;
            }
        }

        if (targetPurpleGems > 0)
        {
            hasGoal = true;

            if (
                purpleGemsCollected <
                targetPurpleGems
            )
            {
                return false;
            }
        }

        if (targetOrangeGems > 0)
        {
            hasGoal = true;

            if (
                orangeGemsCollected <
                targetOrangeGems
            )
            {
                return false;
            }
        }

        return hasGoal;
    }

    // =========================================================
    // COLLAPSE COLUMN
    // =========================================================

    private IEnumerator CollapseColumn(
        int column
    )
    {
        if (isDestroyed)
            yield break;

        int row = 0;

        while (
            row < rows
        )
        {
            while (
                row < rows &&
                !IsActiveCell(
                    row,
                    column
                )
            )
            {
                row++;
            }

            if (row >= rows)
                break;

            int segmentStart =
                row;

            while (
                row < rows &&
                IsActiveCell(
                    row,
                    column
                )
            )
            {
                row++;
            }

            int segmentEnd =
                row - 1;

            yield return StartCoroutine(
                CollapseSegment(
                    column,
                    segmentStart,
                    segmentEnd
                )
            );
        }
    }

    // =========================================================
    // COLLAPSE SEGMENT
    // =========================================================

    private IEnumerator CollapseSegment(
        int column,
        int segmentStart,
        int segmentEnd
    )
    {
        List<GemType> remainingTypes =
            new List<GemType>();

        List<GemView> remainingViews =
            new List<GemView>();

        for (
            int row = segmentStart;
            row <= segmentEnd;
            row++
        )
        {
            if (
                gemViews[
                    row,
                    column
                ] != null
            )
            {
                remainingTypes.Add(
                    board[
                        row,
                        column
                    ]
                );

                remainingViews.Add(
                    gemViews[
                        row,
                        column
                    ]
                );
            }
        }

        int segmentSize =
            segmentEnd -
            segmentStart +
            1;

        GemView[] newViews =
            new GemView[
                segmentSize
            ];

        GemType[] newTypes =
            new GemType[
                segmentSize
            ];

        int existingCount =
            remainingViews.Count;

        for (
            int i = 0;
            i < existingCount;
            i++
        )
        {
            GemView gem =
                remainingViews[i];

            GemType type =
                remainingTypes[i];

            int targetRow =
                segmentStart + i;

            newViews[i] =
                gem;

            newTypes[i] =
                type;

            if (gem != null)
            {
                gem.SetGridPosition(
                    targetRow,
                    column
                );
            }
        }

        int newGemCount =
            segmentSize -
            existingCount;

        for (
            int i = 0;
            i < newGemCount;
            i++
        )
        {
            int arrayIndex =
                existingCount +
                i;

            int targetRow =
                segmentStart +
                arrayIndex;

            GemType newType =
                GetRandomGem();

            newTypes[
                arrayIndex
            ] =
                newType;

            board[
                targetRow,
                column
            ] =
                newType;

            GemView newGem =
                CreateGem(
                    targetRow,
                    column,
                    true
                );

            newViews[
                arrayIndex
            ] =
                newGem;
        }

        for (
            int i = 0;
            i < segmentSize;
            i++
        )
        {
            int targetRow =
                segmentStart +
                i;

            board[
                targetRow,
                column
            ] =
                newTypes[i];

            gemViews[
                targetRow,
                column
            ] =
                newViews[i];
        }

        if (
            newGemCount > 0 &&
            matchAudioSource != null &&
            newGemDropSound != null
        )
        {
            matchAudioSource.PlayOneShot(
                newGemDropSound
            );
        }

        for (
            int i = 0;
            i < segmentSize;
            i++
        )
        {
            int targetRow =
                segmentStart +
                i;

            GemView gem =
                newViews[i];

            if (gem != null)
            {
                StartCoroutine(
                    MoveGemToPosition(
                        gem,
                        GetGridPosition(
                            targetRow,
                            column
                        )
                    )
                );
            }
        }

        yield return new WaitForSeconds(
            fallDuration
        );
    }

    // =========================================================
    // MOVE GEM
    // =========================================================

    private IEnumerator MoveGemToPosition(
        GemView gem,
        Vector2 targetPosition
    )
    {
        if (isDestroyed)
            yield break;

        if (gem == null)
            yield break;

        RectTransform rect =
            gem.GetComponent<RectTransform>();

        if (rect == null)
            yield break;

        Vector2 startPosition =
            rect.anchoredPosition;

        float timer = 0f;

        while (
            timer <
            fallDuration
        )
        {
            if (isDestroyed)
                yield break;

            if (gem == null)
                yield break;

            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    fallDuration
                );

            rect.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        if (rect != null)
        {
            rect.anchoredPosition =
                targetPosition;
        }
    }

    // =========================================================
    // SCORE
    // =========================================================

    private void AddScore(
        int amount
    )
    {
        score += amount;

        UpdateUI();

        Debug.Log(
            "Score: " +
            score
        );
    }

    // =========================================================
    // BLAST PIECE
    // =========================================================

    private class BlastPiece
    {
        public RectTransform rect;
        public CanvasGroup canvasGroup;
        public Vector2 velocity;
        public float rotationSpeed;
        public float startScale;
    }

    // =========================================================
    // BLAST MATCHED GEMS
    // =========================================================

    private IEnumerator BlastMatchedGems(
        List<Vector2Int> matches
    )
    {
        if (isDestroyed)
            yield break;

        List<BlastPiece> pieces =
            new List<BlastPiece>();

        List<GameObject> originalGems =
            new List<GameObject>();

        foreach (
            Vector2Int position
            in matches
        )
        {
            int row =
                position.x;

            int column =
                position.y;

            if (
                !IsActiveCell(
                    row,
                    column
                )
            )
            {
                continue;
            }

            GemView gem =
                gemViews[
                    row,
                    column
                ];

            if (gem == null)
                continue;

            GameObject gemObject =
                gem.gameObject;

            originalGems.Add(
                gemObject
            );

            Image sourceImage =
                gemObject.GetComponentInChildren<Image>();

            if (
                sourceImage == null ||
                sourceImage.sprite == null
            )
            {
                gemViews[
                    row,
                    column
                ] = null;

                continue;
            }

            RectTransform sourceRect =
                sourceImage.rectTransform;

            for (
                int i = 0;
                i < blastPieces;
                i++
            )
            {
                GameObject pieceObject =
                    new GameObject(
                        "GemBlastPiece",
                        typeof(RectTransform),
                        typeof(Image),
                        typeof(CanvasGroup)
                    );

                pieceObject.transform.SetParent(
                    sourceRect.parent,
                    false
                );

                RectTransform pieceRect =
                    pieceObject.GetComponent<RectTransform>();

                Image pieceImage =
                    pieceObject.GetComponent<Image>();

                CanvasGroup pieceCanvas =
                    pieceObject.GetComponent<CanvasGroup>();

                pieceImage.sprite =
                    sourceImage.sprite;

                pieceImage.preserveAspect =
                    true;

                pieceImage.raycastTarget =
                    false;

                pieceCanvas.alpha =
                    1f;

                pieceRect.position =
                    sourceRect.position;

                pieceRect.sizeDelta =
                    sourceRect.sizeDelta;

                float scale =
                    Random.Range(
                        blastPieceSize * 0.65f,
                        blastPieceSize * 1.15f
                    );

                pieceRect.localScale =
                    Vector3.one *
                    scale;

                pieceRect.SetAsLastSibling();

                Vector2 direction =
                    Random.insideUnitCircle.normalized;

                if (
                    direction.sqrMagnitude <
                    0.01f
                )
                {
                    direction =
                        Vector2.up;
                }

                float force =
                    Random.Range(
                        blastForce * 0.65f,
                        blastForce * 1.25f
                    );

                pieces.Add(
                    new BlastPiece
                    {
                        rect = pieceRect,
                        canvasGroup = pieceCanvas,
                        velocity =
                            direction *
                            force,
                        rotationSpeed =
                            Random.Range(
                                -blastRotationSpeed,
                                blastRotationSpeed
                            ),
                        startScale =
                            scale
                    }
                );
            }

            CanvasGroup originalCanvas =
                gemObject.GetComponent<CanvasGroup>();

            if (originalCanvas == null)
            {
                originalCanvas =
                    gemObject.AddComponent<CanvasGroup>();
            }

            originalCanvas.alpha =
                0f;

            gemViews[
                row,
                column
            ] = null;
        }

        if (
            matchAudioSource != null &&
            matchSound != null
        )
        {
            matchAudioSource.PlayOneShot(
                matchSound
            );
        }

        float elapsed = 0f;

        while (
            elapsed <
            blastDuration
        )
        {
            if (isDestroyed)
                yield break;

            float dt =
                Time.deltaTime;

            elapsed += dt;

            foreach (
                BlastPiece piece
                in pieces
            )
            {
                if (piece.rect == null)
                    continue;

                piece.velocity.y -=
                    blastGravity * dt;

                piece.rect.position +=
                    (Vector3)(
                        piece.velocity *
                        dt
                    );

                piece.rect.Rotate(
                    0f,
                    0f,
                    piece.rotationSpeed *
                    dt
                );

                float progress =
                    Mathf.Clamp01(
                        elapsed /
                        blastDuration
                    );

                if (
                    piece.canvasGroup != null
                )
                {
                    piece.canvasGroup.alpha =
                        1f -
                        progress;
                }

                float scaleMultiplier =
                    Mathf.Lerp(
                        1f,
                        0.25f,
                        progress
                    );

                piece.rect.localScale =
                    Vector3.one *
                    piece.startScale *
                    scaleMultiplier;
            }

            yield return null;
        }

        foreach (
            BlastPiece piece
            in pieces
        )
        {
            if (piece.rect != null)
            {
                Destroy(
                    piece.rect.gameObject
                );
            }
        }

        foreach (
            GameObject gemObject
            in originalGems
        )
        {
            if (gemObject != null)
            {
                Destroy(
                    gemObject
                );
            }
        }
    }

    // =========================================================
    // UPDATE UI
    // =========================================================

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "SCORE: " +
                score;
        }

        if (movesText != null)
        {
            movesText.text =
                "MOVES: " +
                moves;
        }

        if (goalProgressText != null)
        {
            List<string> progress =
                new List<string>();

            if (targetRedGems > 0)
            {
                progress.Add(
                    redGemsCollected +
                    " / " +
                    targetRedGems +
                    " RED GEMS"
                );
            }

            if (targetBlueGems > 0)
            {
                progress.Add(
                    blueGemsCollected +
                    " / " +
                    targetBlueGems +
                    " BLUE GEMS"
                );
            }

            if (targetGreenGems > 0)
            {
                progress.Add(
                    greenGemsCollected +
                    " / " +
                    targetGreenGems +
                    " GREEN GEMS"
                );
            }

            if (targetPinkGems > 0)
            {
                progress.Add(
                    pinkGemsCollected +
                    " / " +
                    targetPinkGems +
                    " PINK GEMS"
                );
            }

            if (targetPurpleGems > 0)
            {
                progress.Add(
                    purpleGemsCollected +
                    " / " +
                    targetPurpleGems +
                    " PURPLE GEMS"
                );
            }

            if (targetOrangeGems > 0)
            {
                progress.Add(
                    orangeGemsCollected +
                    " / " +
                    targetOrangeGems +
                    " ORANGE GEMS"
                );
            }

            goalProgressText.text =
                string.Join(
                    "\n",
                    progress
                );
        }

        if (
            levelText != null &&
            GameLevelManager.Instance != null
        )
        {
            levelText.text =
                "LEVEL " +
                GameLevelManager.Instance.GetCurrentLevel();
        }
    }

    // =========================================================
    // LEVEL COMPLETE
    // =========================================================

    private void ShowLevelComplete()
    {
        if (levelCompleted)
            return;

        levelCompleted = true;

        isProcessing = false;

        StopHint();

        // =====================================================
        // AUTO FIND UI
        // =====================================================

        AutoFindLevelCompleteUI();

        // =====================================================
        // FORCE OPEN PANEL
        // =====================================================

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(
                true
            );

            // Bring it to front
            if (
                levelCompletePanel.transform.parent != null
            )
            {
                levelCompletePanel.transform.SetAsLastSibling();
            }

            Debug.Log(
                "LEVEL COMPLETE PANEL ACTIVATED!"
            );
        }
        else
        {
            Debug.LogError(
                "LEVEL COMPLETE PANEL NOT FOUND!"
            );
        }

        // =====================================================
        // SHOW LEVEL COMPLETE UI
        // =====================================================

        if (levelCompleteUI != null)
        {
            levelCompleteUI.ShowLevelComplete(
                score,
                moves,

                redGemsCollected,
                targetRedGems,

                blueGemsCollected,
                targetBlueGems,

                greenGemsCollected,
                targetGreenGems,

                pinkGemsCollected,
                targetPinkGems,

                purpleGemsCollected,
                targetPurpleGems,

                orangeGemsCollected,
                targetOrangeGems
            );

            Debug.Log(
                "LEVEL COMPLETE UI SHOW CALLED!"
            );
        }
        else
        {
            Debug.LogError(
                "LEVEL COMPLETE UI NOT FOUND!"
            );
        }

        // =====================================================
        // SOUND
        // =====================================================

        if (
            matchAudioSource != null &&
            levelCompleteSound != null
        )
        {
            matchAudioSource.PlayOneShot(
                levelCompleteSound
            );
        }

        // =====================================================
        // WINNER PARTICLE
        // =====================================================

        if (winnerParticleEffect != null)
        {
            winnerParticleEffect.gameObject.SetActive(
                true
            );

            winnerParticleEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            winnerParticleEffect.Play(
                true
            );
        }

        Debug.Log(
            "===================================="
        );
    }

    // =========================================================
    // LEVEL FAILED
    // =========================================================

    private void ShowLevelFailed()
    {
        if (levelFailed)
            return;

        if (AreAllGoalsCompleted())
            return;

        levelFailed = true;

        StopHint();

        Debug.Log(
            "LEVEL FAILED! MOVES FINISHED!"
        );

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(
                false
            );
        }

        if (winnerParticleEffect != null)
        {
            winnerParticleEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            winnerParticleEffect.gameObject.SetActive(
                false
            );
        }

        FailPanel failPanel =
            FindFirstObjectByType<FailPanel>(
                FindObjectsInactive.Include
            );

        if (failPanel != null)
        {
            failPanel.Show(
                score,

                redGemsCollected,
                targetRedGems,

                blueGemsCollected,
                targetBlueGems,

                greenGemsCollected,
                targetGreenGems,

                pinkGemsCollected,
                targetPinkGems,

                purpleGemsCollected,
                targetPurpleGems,

                orangeGemsCollected,
                targetOrangeGems,

                startingMoves - moves
            );
        }
        else
        {
            Debug.LogWarning(
                "FailPanel not found in scene."
            );
        }
    }

    // =========================================================
    // NEXT LEVEL
    // =========================================================

    public void NextLevel()
    {
        if (isDestroyed)
            return;

        if (isLoadingNextLevel)
            return;

        if (GameLevelManager.Instance == null)
        {
            Debug.LogError(
                "GameLevelManager not found!"
            );

            return;
        }

        Time.timeScale = 1f;

        int currentLevel =
            GameLevelManager.Instance.GetCurrentLevel();

        int nextLevel =
            currentLevel + 1;

        if (
            nextLevel >
            GameLevelManager.MaxLevel
        )
        {
            Debug.Log(
                "ALL LEVELS COMPLETED!"
            );

            return;
        }

        isLoadingNextLevel = true;

        GameLevelManager.Instance.UnlockLevel(
            nextLevel
        );

        GameLevelManager.Instance.SetCurrentLevel(
            nextLevel
        );

        PlayerPrefs.Save();

        Debug.Log(
            "NEXT LEVEL = " +
            GameLevelManager.Instance.GetCurrentLevel()
        );

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // EXTRA MOVE
    // =========================================================

    public bool AddExtraMove()
    {
        if (
            isDestroyed ||
            levelCompleted ||
            levelFailed
        )
        {
            return false;
        }

        moves++;

        UpdateUI();

        Debug.Log(
            "EXTRA MOVE ADDED = " +
            moves
        );

        return true;
    }

    // =========================================================
    // SHUFFLE
    // =========================================================

    public bool ShuffleBoard()
    {
        if (
            isDestroyed ||
            isProcessing ||
            levelCompleted ||
            levelFailed
        )
        {
            return false;
        }

        if (
            board == null ||
            gemViews == null
        )
        {
            return false;
        }

        List<Vector2Int> positions =
            new List<Vector2Int>();

        List<GemType> types =
            new List<GemType>();

        for (
            int row = 0;
            row < rows;
            row++
        )
        {
            for (
                int column = 0;
                column < columns;
                column++
            )
            {
                if (
                    !IsActiveCell(
                        row,
                        column
                    )
                )
                {
                    continue;
                }

                if (
                    gemViews[
                        row,
                        column
                    ] == null
                )
                {
                    continue;
                }

                positions.Add(
                    new Vector2Int(
                        row,
                        column
                    )
                );

                types.Add(
                    board[
                        row,
                        column
                    ]
                );
            }
        }

        if (positions.Count < 2)
            return false;

        for (
            int i = types.Count - 1;
            i > 0;
            i--
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );

            GemType temp =
                types[i];

            types[i] =
                types[
                    randomIndex
                ];

            types[
                randomIndex
            ] =
                temp;
        }

        for (
            int i = 0;
            i < positions.Count;
            i++
        )
        {
            int row =
                positions[i].x;

            int column =
                positions[i].y;

            GemType type =
                types[i];

            board[
                row,
                column
            ] =
                type;

            GemView gem =
                gemViews[
                    row,
                    column
                ];

            if (gem != null)
            {
                gem.SetColor(
                    type
                );

                Sprite sprite =
                    GetGemSprite(
                        type
                    );

                if (sprite != null)
                {
                    gem.SetGemSprite(
                        sprite
                    );
                }
            }
        }

        StopHint();

        hintTimer = 0f;

        UpdateUI();

        return true;
    }

    // =========================================================
    // HAMMER
    // =========================================================

    public bool UseHammer(
        GemView gem
    )
    {
        if (
            isDestroyed ||
            isProcessing ||
            levelCompleted ||
            levelFailed
        )
        {
            return false;
        }

        if (gem == null)
            return false;

        int row =
            gem.GetRow();

        int column =
            gem.GetColumn();

        if (
            !IsActiveCell(
                row,
                column
            )
        )
        {
            return false;
        }

        List<Vector2Int> cells =
            new List<Vector2Int>();

        cells.Add(
            new Vector2Int(
                row,
                column
            )
        );

        StartCoroutine(
            PowerUpRemoveCells(
                cells
            )
        );

        return true;
    }

    // =========================================================
    // BOMB
    // =========================================================

    public bool UseBomb(
        GemView gem
    )
    {
        if (
            isDestroyed ||
            isProcessing ||
            levelCompleted ||
            levelFailed
        )
        {
            return false;
        }

        if (gem == null)
            return false;

        int centerRow =
            gem.GetRow();

        int centerColumn =
            gem.GetColumn();

        if (
            !IsActiveCell(
                centerRow,
                centerColumn
            )
        )
        {
            return false;
        }

        List<Vector2Int> cells =
            new List<Vector2Int>();

        for (
            int row = centerRow - 1;
            row <= centerRow + 1;
            row++
        )
        {
            for (
                int column = centerColumn - 1;
                column <= centerColumn + 1;
                column++
            )
            {
                if (
                    !IsActiveCell(
                        row,
                        column
                    )
                )
                {
                    continue;
                }

                Vector2Int position =
                    new Vector2Int(
                        row,
                        column
                    );

                if (
                    !cells.Contains(
                        position
                    )
                )
                {
                    cells.Add(
                        position
                    );
                }
            }
        }

        if (cells.Count == 0)
            return false;

        StartCoroutine(
            PowerUpRemoveCells(
                cells
            )
        );

        return true;
    }

    // =========================================================
    // POWER UP REMOVE CELLS
    // =========================================================

    private IEnumerator PowerUpRemoveCells(
        List<Vector2Int> cells
    )
    {
        if (
            isDestroyed ||
            cells == null ||
            cells.Count == 0
        )
        {
            yield break;
        }

        isProcessing = true;

        StopHint();

        int redCount = 0;
        int blueCount = 0;
        int greenCount = 0;
        int pinkCount = 0;
        int purpleCount = 0;
        int orangeCount = 0;

        List<GameObject> objectsToDestroy =
            new List<GameObject>();

        foreach (
            Vector2Int position
            in cells
        )
        {
            int row =
                position.x;

            int column =
                position.y;

            if (
                !IsActiveCell(
                    row,
                    column
                )
            )
            {
                continue;
            }

            switch (
                board[
                    row,
                    column
                ]
            )
            {
                case GemType.Red:
                    redCount++;
                    break;

                case GemType.Blue:
                    blueCount++;
                    break;

                case GemType.Green:
                    greenCount++;
                    break;

                case GemType.Pink:
                    pinkCount++;
                    break;

                case GemType.Purple:
                    purpleCount++;
                    break;

                case GemType.Orange:
                    orangeCount++;
                    break;
            }

            GemView gem =
                gemViews[
                    row,
                    column
                ];

            if (gem != null)
            {
                objectsToDestroy.Add(
                    gem.gameObject
                );

                gemViews[
                    row,
                    column
                ] = null;
            }
        }

        AddCollectedGems(
            redCount,
            blueCount,
            greenCount,
            pinkCount,
            purpleCount,
            orangeCount
        );

        if (
            matchAudioSource != null &&
            matchSound != null
        )
        {
            matchAudioSource.PlayOneShot(
                matchSound
            );
        }

        yield return new WaitForSeconds(
            0.05f
        );

        foreach (
            Vector2Int position
            in cells
        )
        {
            int row =
                position.x;

            int column =
                position.y;

            if (
                !IsValidCell(
                    row,
                    column
                )
            )
            {
                continue;
            }

            board[
                row,
                column
            ] =
                GemType.Red;
        }

        foreach (
            GameObject gemObject
            in objectsToDestroy
        )
        {
            if (gemObject != null)
            {
                Destroy(
                    gemObject
                );
            }
        }

        yield return new WaitForSeconds(
            0.05f
        );

        for (
            int column = 0;
            column < columns;
            column++
        )
        {
            if (isDestroyed)
                yield break;

            yield return StartCoroutine(
                CollapseColumn(
                    column
                )
            );
        }

        yield return new WaitForSeconds(
            fallDuration
        );

        if (isDestroyed)
            yield break;

        // =====================================================
        // AUTOMATIC LEVEL COMPLETE
        // =====================================================

        if (AreAllGoalsCompleted())
        {
            isProcessing = false;

            ShowLevelComplete();

            yield break;
        }

        List<Vector2Int> newMatches =
            FindAllMatches();

        if (
            newMatches != null &&
            newMatches.Count >= 3
        )
        {
            int points;

            if (newMatches.Count == 3)
                points = 30;
            else if (newMatches.Count == 4)
                points = 50;
            else
                points = 100;

            AddScore(
                points
            );

            yield return StartCoroutine(
                RemoveAndFill(
                    newMatches
                )
            );
        }

        if (
            !levelCompleted &&
            moves <= 0
        )
        {
            ShowLevelFailed();
        }

        UpdateUI();

        hintTimer = 0f;

        isProcessing = false;
    }

    // =========================================================
    // GET SCORE
    // =========================================================

    public int GetScore()
    {
        return score;
    }

    // =========================================================
    // GET MOVES
    // =========================================================

    public int GetMoves()
    {
        return moves;
    }

    // =========================================================
    // GET COLLECTED GOALS
    // =========================================================

    public int GetRedGemsCollected()
    {
        return redGemsCollected;
    }

    public int GetBlueGemsCollected()
    {
        return blueGemsCollected;
    }

    public int GetGreenGemsCollected()
    {
        return greenGemsCollected;
    }

    public int GetPinkGemsCollected()
    {
        return pinkGemsCollected;
    }

    public int GetPurpleGemsCollected()
    {
        return purpleGemsCollected;
    }

    public int GetOrangeGemsCollected()
    {
        return orangeGemsCollected;
    }

    public bool AreGoalsCompleted()
    {
        return AreAllGoalsCompleted();
    }

    // =========================================================
    // GET CELL
    // =========================================================

    public GemType GetCell(
        int row,
        int column
    )
    {
        if (
            !IsValidCell(
                row,
                column
            )
        )
        {
            return GemType.Red;
        }

        return board[
            row,
            column
        ];
    }

    // =========================================================
    // SET CELL
    // =========================================================

    public void SetCell(
        int row,
        int column,
        GemType gemType
    )
    {
        if (
            !IsActiveCell(
                row,
                column
            )
        )
        {
            return;
        }

        board[
            row,
            column
        ] =
            gemType;

        if (
            gemViews[
                row,
                column
            ] != null
        )
        {
            gemViews[
                row,
                column
            ].SetColor(
                gemType
            );

            gemViews[
                row,
                column
            ].SetGemSprite(
                GetGemSprite(
                    gemType
                )
            );
        }
    }

    // =========================================================
    // VALID CELL
    // =========================================================

    public bool IsValidCell(
        int row,
        int column
    )
    {
        return
            row >= 0 &&
            row < rows &&
            column >= 0 &&
            column < columns;
    }

    // =========================================================
    // ACTIVE CELL
    // =========================================================

    public bool IsActiveCell(
        int row,
        int column
    )
    {
        if (
            !IsValidCell(
                row,
                column
            )
        )
        {
            return false;
        }

        if (activeCells == null)
            return false;

        return activeCells[
            row,
            column
        ];
    }

    // =========================================================
    // GET ROWS
    // =========================================================

    public int GetRows()
    {
        return rows;
    }

    // =========================================================
    // GET COLUMNS
    // =========================================================

    public int GetColumns()
    {
        return columns;
    }

    // =========================================================
    // PRINT BOARD
    // =========================================================

    private void PrintBoard()
    {
        if (board == null)
            return;

        string boardText = "";

        for (
            int row = rows - 1;
            row >= 0;
            row--
        )
        {
            for (
                int column = 0;
                column < columns;
                column++
            )
            {
                if (
                    !activeCells[
                        row,
                        column
                    ]
                )
                {
                    boardText +=
                        "X ";
                }
                else
                {
                    boardText +=
                        GetGemShortName(
                            board[
                                row,
                                column
                            ]
                        ) +
                        " ";
                }
            }

            boardText += "\n";
        }

        Debug.Log(
            boardText
        );
    }
    // HINT BUTTON

    public void OnHintButtonClicked()
    {
    if (audioSource != null && hintSound != null)
    {
         audioSource.PlayOneShot(hintSound);
    }

    if (isDestroyed || isProcessing || levelCompleted || levelFailed)
        return;

    if (moves <= 0)
        return;

    if (hintShowing)
        return;

    HintMove validMove;

    if (!FindValidHintMove(out validMove))
        return;

    GemView gem1 = GetGemView(
        validMove.row1,
        validMove.column1
    );

    GemView gem2 = GetGemView(
        validMove.row2,
        validMove.column2
    );

    if (gem1 == null || gem2 == null)
        return;

    StopHint();

    hintCoroutine = StartCoroutine(
        PlayHintAnimation(gem1, gem2)
    );
    }
    
    private IEnumerator PlayHintAnimation(GemView gem1, GemView gem2)
    {
    if (gem1 == null || gem2 == null)
        yield break;

    Image image1 = gem1.GetComponent<Image>();
    Image image2 = gem2.GetComponent<Image>();

    Color originalColor1 = image1 != null ? image1.color : Color.white;
    Color originalColor2 = image2 != null ? image2.color : Color.white;

    Vector3 scale1 = gem1.transform.localScale;
    Vector3 scale2 = gem2.transform.localScale;

    for (int i = 0; i < 2; i++)
    {
        float timer = 0f;

        while (timer < 0.6f)
        {
            timer += Time.deltaTime;

            float t = timer / 0.6f;

            float scale = Mathf.Lerp(
                1f,
                1.15f,
                Mathf.Sin(t * Mathf.PI)
            );

            gem1.transform.localScale = scale1 * scale;
            gem2.transform.localScale = scale2 * scale;

            bool blink = Mathf.Sin(timer * 30f) > 0f;

            Color hintColor1 = Color.yellow;
            Color hintColor2 = Color.cyan;

            if (image1 != null)
            image1.color = blink ? hintColor1 : originalColor1;

            if (image2 != null)
            image2.color = blink ? hintColor2 : originalColor2;

            yield return null;
        }
    }

    gem1.transform.localScale = scale1;
    gem2.transform.localScale = scale2;

    if (image1 != null)
        image1.color = originalColor1;

    if (image2 != null)
        image2.color = originalColor2;
    }
}
