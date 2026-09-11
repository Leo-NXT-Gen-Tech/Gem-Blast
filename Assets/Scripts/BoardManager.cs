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

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text goalProgressText;

    [Header("Game Settings")]
    [SerializeField] private int startingMoves = 30;

    [Header("Goal")]
    [SerializeField] private int targetRedGems = 20;

    [Header("Result Panels")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject levelFailedPanel;

    // =========================================================
    // HINT SYSTEM
    // =========================================================

    [Header("Hint System")]
    [SerializeField] private float hintDelay = 3f;
    [SerializeField] private float hintScale = 1.12f;
    [SerializeField] private float hintRotation = 6f;
    [SerializeField] private float hintAnimDuration = 0.30f;
    [SerializeField] private int hintCycles = 2;

    private float hintTimer = 0f;
    private Coroutine hintCoroutine;
    private GemView hintGem;
    private Vector3 hintOriginalScale;
    private Quaternion hintOriginalRotation;
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

    private float cellWidth;
    private float cellHeight;
    private float gemSize;

    private int score = 0;
    private int moves;
    private int redGemsCollected = 0;

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (isDestroyed)
            return;

        if (isProcessing)
        {
            hintTimer = 0f;
            return;
        }

        if (levelCompleted || levelFailed)
        {
            hintTimer = 0f;
            return;
        }

        if (moves <= 0)
        {
            hintTimer = 0f;
            return;
        }

        if (hintShowing)
            return;

        hintTimer += Time.deltaTime;

        if (hintTimer >= hintDelay)
        {
            HintMove validMove;

            bool foundMove =
                FindValidHintMove(out validMove);

            if (foundMove)
            {
                GemView gem =
                    GetGemView(
                        validMove.row1,
                        validMove.column1
                    );

                if (gem != null)
                {
                    hintCoroutine =
                        StartCoroutine(
                            PlayHintAnimation(gem)
                        );
                }
                else
                {
                    hintTimer = 0f;
                }
            }
            else
            {
                hintTimer = 0f;
            }
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (isDestroyed)
            return;

        Time.timeScale = 1f;

        int currentLevel = 1;

        if (GameLevelManager.Instance != null)
        {
            currentLevel =
                GameLevelManager.Instance.GetCurrentLevel();

            startingMoves =
                GameLevelManager.Instance.GetMoves(
                    currentLevel
                );

            targetRedGems =
                GameLevelManager.Instance.GetRedGoal(
                    currentLevel
                );
        }

        Debug.Log("====================================");
        Debug.Log("START LEVEL: " + currentLevel);
        Debug.Log("MOVES: " + startingMoves);
        Debug.Log("RED GOAL: " + targetRedGems);

        moves = startingMoves;
        score = 0;
        redGemsCollected = 0;

        levelCompleted = false;
        levelFailed = false;

        hintTimer = 0f;
        hintShowing = false;
        hintGem = null;
        hintCoroutine = null;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        if (levelFailedPanel != null)
            levelFailedPanel.SetActive(false);

        bool savedLevelLoaded =
            LoadSavedLevel(currentLevel);

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

        Debug.Log("====================================");
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
    // INITIALIZE BOARD
    // =========================================================

    private void InitializeBoard()
    {
        rows = Mathf.Max(1, rows);
        columns = Mathf.Max(1, columns);

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

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                activeCells[row, column] = true;
                board[row, column] = GemType.Red;
            }
        }
    }

    // =========================================================
    // RANDOM GEM
    // =========================================================

    private GemType GetRandomGem()
    {
        return (GemType)Random.Range(0, 6);
    }

    // =========================================================
    // GET GEM PREFAB
    // =========================================================

    private GameObject GetGemPrefab(GemType type)
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

    private Sprite GetGemSprite(GemType type)
    {
        GameObject prefab =
            GetGemPrefab(type);

        if (prefab == null)
        {
            Debug.LogError(
                "Prefab not assigned for GemType: " +
                type
            );

            return null;
        }

        Image image =
            prefab.GetComponentInChildren<Image>(true);

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
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (!activeCells[row, column])
                {
                    board[row, column] = GemType.Red;
                    continue;
                }

                board[row, column] =
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

    private bool LoadSavedLevel(int level)
    {
        if (GameLevelManager.Instance == null)
        {
            Debug.LogWarning(
                "GameLevelManager not found!"
            );

            return false;
        }

        LevelData levelData =
            GameLevelManager.Instance.GetLevelData(level);

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

        levelData.Initialize();

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                board[row, column] =
                    levelData.GetGem(
                        row,
                        column
                    );

                activeCells[row, column] =
                    levelData.IsValidCell(
                        row,
                        column
                    );
            }
        }

        Debug.Log("====================================");
        Debug.Log(
            "LEVEL " +
            level +
            " LOADED FROM LEVEL DATA"
        );

        Debug.Log(
            "BOARD SIZE: " +
            rows +
            " x " +
            columns
        );

        Debug.Log("====================================");

        return true;
    }

    // =========================================================
    // GEM SHORT NAME
    // =========================================================

    private string GetGemShortName(GemType type)
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
            panelWidth / columns;

        cellHeight =
            panelHeight / rows;

        gemSize =
            Mathf.Min(
                cellWidth,
                cellHeight
            ) * 0.85f;

        int createdCount = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (!activeCells[row, column])
                    continue;

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

        if (!IsActiveCell(row, column))
            return null;

        GemType currentType =
            board[row, column];

        GameObject selectedPrefab =
            GetGemPrefab(currentType);

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
                new Vector2(0.5f, 0.5f);

            gemRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            gemRect.pivot =
                new Vector2(0.5f, 0.5f);

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

            Destroy(gemObject);

            return null;
        }

        gemView.SetGem(
            currentType,
            row,
            column
        );

        Sprite sprite =
            GetGemSprite(currentType);

        if (sprite != null)
        {
            gemView.SetGemSprite(sprite);
        }

        gemView.SetBoardManager(this);

        gemViews[row, column] =
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
            ) * cellWidth;

        float y =
            (
                row -
                (rows - 1) / 2f
            ) * cellHeight;

        return new Vector2(x, y);
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

        if (!IsActiveCell(
                targetRow,
                targetColumn))
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

        hintGem = null;
        hintShowing = false;
    }

    // =========================================================
    // FIND VALID HINT MOVE
    // =========================================================

    private bool FindValidHintMove(
        out HintMove validMove
    )
    {
        validMove = new HintMove();

        if (
            board == null ||
            gemViews == null
        )
        {
            return false;
        }

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (!IsActiveCell(row, column))
                    continue;

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

        if (
            gemViews[
                row1,
                column1
            ] == null ||
            gemViews[
                row2,
                column2
            ] == null
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
        ] = type2;

        board[
            row2,
            column2
        ] = type1;

        List<Vector2Int> matches =
            FindAllMatches();

        bool createsMatch =
            matches != null &&
            matches.Count >= 3;

        board[
            row1,
            column1
        ] = type1;

        board[
            row2,
            column2
        ] = type2;

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

    private IEnumerator PlayHintAnimation(
        GemView gem
    )
    {
        if (gem == null)
            yield break;

        hintShowing = true;
        hintGem = gem;

        Transform target =
            gem.transform;

        hintOriginalScale =
            target.localScale;

        hintOriginalRotation =
            target.localRotation;

        for (
            int cycle = 0;
            cycle < hintCycles;
            cycle++
        )
        {
            if (isDestroyed)
                yield break;

            if (gem == null)
                yield break;

            yield return StartCoroutine(
                AnimateHint(
                    target,
                    hintOriginalScale,
                    hintOriginalScale * hintScale,
                    hintOriginalRotation,
                    Quaternion.Euler(
                        0f,
                        0f,
                        hintRotation
                    )
                )
            );

            if (gem == null)
                yield break;

            yield return StartCoroutine(
                AnimateHint(
                    target,
                    hintOriginalScale * hintScale,
                    hintOriginalScale,
                    Quaternion.Euler(
                        0f,
                        0f,
                        hintRotation
                    ),
                    Quaternion.Euler(
                        0f,
                        0f,
                        -hintRotation
                    )
                )
            );

            if (gem == null)
                yield break;

            yield return StartCoroutine(
                AnimateHint(
                    target,
                    hintOriginalScale,
                    hintOriginalScale,
                    Quaternion.Euler(
                        0f,
                        0f,
                        -hintRotation
                    ),
                    hintOriginalRotation
                )
            );
        }

        if (gem != null)
        {
            target.localScale =
                hintOriginalScale;

            target.localRotation =
                hintOriginalRotation;
        }

        hintGem = null;
        hintShowing = false;
        hintCoroutine = null;
        hintTimer = 0f;
    }

    // =========================================================
    // HINT ANIMATION HELPER
    // =========================================================

    private IEnumerator AnimateHint(
        Transform target,
        Vector3 startScale,
        Vector3 endScale,
        Quaternion startRotation,
        Quaternion endRotation
    )
    {
        if (target == null)
            yield break;

        float timer = 0f;

        while (
            timer <
            hintAnimDuration
        )
        {
            if (isDestroyed)
                yield break;

            if (target == null)
                yield break;

            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    hintAnimDuration
                );

            t =
                t *
                t *
                (3f - 2f * t);

            target.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    t
                );

            target.localRotation =
                Quaternion.Lerp(
                    startRotation,
                    endRotation,
                    t
                );

            yield return null;
        }

        if (target != null)
        {
            target.localScale =
                endScale;

            target.localRotation =
                endRotation;
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
        if (isDestroyed)
            yield break;

        if (isProcessing)
            yield break;

        if (
            gem1 == null ||
            gem2 == null
        )
            yield break;

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
        ] = type2;

        board[
            row2,
            column2
        ] = type1;

        gemViews[
            row1,
            column1
        ] = gem2;

        gemViews[
            row2,
            column2
        ] = gem1;

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
            Debug.Log(
                "MATCH FOUND: " +
                matches.Count
            );

            int points;

            if (matches.Count == 3)
                points = 30;
            else if (matches.Count == 4)
                points = 50;
            else
                points = 100;

            AddScore(points);

            yield return StartCoroutine(
                RemoveAndFill(matches)
            );
        }
        else
        {
            Debug.Log(
                "NO MATCH FOUND - SWAPPING BACK!"
            );

            board[
                row1,
                column1
            ] = type1;

            board[
                row2,
                column2
            ] = type2;

            gemViews[
                row1,
                column1
            ] = gem1;

            gemViews[
                row2,
                column2
            ] = gem2;

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

        // HORIZONTAL
        for (int row = 0; row < rows; row++)
        {
            int column = 0;

            while (column < columns)
            {
                if (!IsActiveCell(row, column))
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

                        if (!matches.Contains(position))
                        {
                            matches.Add(position);
                        }
                    }
                }
            }
        }

        // VERTICAL
        for (
            int column = 0;
            column < columns;
            column++
        )
        {
            int row = 0;

            while (row < rows)
            {
                if (!IsActiveCell(row, column))
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

                        if (!matches.Contains(position))
                        {
                            matches.Add(position);
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

        foreach (
            Vector2Int position
            in matches
        )
        {
            int row = position.x;
            int column = position.y;

            if (
                IsActiveCell(row, column) &&
                board[row, column] ==
                GemType.Red
            )
            {
                redCountThisMatch++;
            }
        }

        if (redCountThisMatch > 0)
        {
            AddRedGemsCollected(
                redCountThisMatch
            );
        }

        foreach (
            Vector2Int position
            in matches
        )
        {
            int row = position.x;
            int column = position.y;

            if (
                gemViews[
                    row,
                    column
                ] != null
            )
            {
                Destroy(
                    gemViews[
                        row,
                        column
                    ].gameObject
                );

                gemViews[
                    row,
                    column
                ] = null;
            }
        }

        yield return new WaitForSeconds(0.1f);

        for (
            int column = 0;
            column < columns;
            column++
        )
        {
            if (isDestroyed)
                yield break;

            yield return StartCoroutine(
                CollapseColumn(column)
            );
        }

        yield return new WaitForSeconds(
            fallDuration
        );

        if (isDestroyed)
            yield break;

        if (
            redGemsCollected >=
            targetRedGems
        )
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

            AddScore(chainPoints);

            yield return StartCoroutine(
                RemoveAndFill(newMatches)
            );
        }
    }

    // =========================================================
    // RED GEMS
    // =========================================================

    private void AddRedGemsCollected(
        int amount
    )
    {
        redGemsCollected += amount;

        if (
            redGemsCollected >
            targetRedGems
        )
        {
            redGemsCollected =
                targetRedGems;
        }

        UpdateUI();

        Debug.Log(
            "RED GEMS COLLECTED: " +
            redGemsCollected +
            " / " +
            targetRedGems
        );
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

        while (row < rows)
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

            int segmentStart = row;

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

            newViews[i] = gem;
            newTypes[i] = type;

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
                existingCount + i;

            int targetRow =
                segmentStart +
                arrayIndex;

            GemType newType =
                GetRandomGem();

            newTypes[arrayIndex] =
                newType;

            board[
                targetRow,
                column
            ] = newType;

            GemView newGem =
                CreateGem(
                    targetRow,
                    column,
                    true
                );

            newViews[arrayIndex] =
                newGem;
        }

        for (
            int i = 0;
            i < segmentSize;
            i++
        )
        {
            int targetRow =
                segmentStart + i;

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

        for (
            int i = 0;
            i < segmentSize;
            i++
        )
        {
            int targetRow =
                segmentStart + i;

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

    private void AddScore(int amount)
    {
        score += amount;

        UpdateUI();

        Debug.Log(
            "Score: " +
            score
        );
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
            goalProgressText.text =
                redGemsCollected +
                " / " +
                targetRedGems;
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

        StopHint();

        Debug.Log(
            "LEVEL COMPLETE! RED GOAL COMPLETED!"
        );

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
    }

    // =========================================================
    // LEVEL FAILED
    // =========================================================

    private void ShowLevelFailed()
    {
        if (levelFailed)
            return;

        if (
            redGemsCollected >=
            targetRedGems
        )
        {
            return;
        }

        levelFailed = true;

        StopHint();

        Debug.Log(
            "LEVEL FAILED! MOVES FINISHED!"
        );

        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
        }
    }

    // =========================================================
    // RETRY
    // =========================================================

    public void RetryLevel()
    {
        if (isDestroyed)
            return;

        Time.timeScale = 1f;

        Debug.Log(
            "RETRY CURRENT LEVEL"
        );

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    // =========================================================
    // NEXT LEVEL - FIXED
    // =========================================================

    public void NextLevel()
    {
        if (isDestroyed)
            return;

        Time.timeScale = 1f;

        Debug.Log("====================================");
        Debug.Log("NEXT LEVEL BUTTON CLICKED");

        if (GameLevelManager.Instance == null)
        {
            Debug.LogError(
                "GameLevelManager not found!"
            );

            return;
        }

        int currentLevel =
            GameLevelManager.Instance.GetCurrentLevel();

        int nextLevel =
            currentLevel + 1;

        Debug.Log(
            "CURRENT LEVEL = " +
            currentLevel
        );

        Debug.Log(
            "NEXT LEVEL = " +
            nextLevel
        );

        // -----------------------------------------------------
        // CHECK MAX LEVEL
        // -----------------------------------------------------

        if (
            nextLevel >
            GameLevelManager.MaxLevel
        )
        {
            Debug.Log(
                "ALL LEVELS COMPLETED!"
            );

            Debug.Log("====================================");

            return;
        }

        // -----------------------------------------------------
        // IMPORTANT
        // UNLOCK FIRST
        // THEN SET CURRENT LEVEL
        // -----------------------------------------------------

        GameLevelManager.Instance.UnlockNextLevel();

        GameLevelManager.Instance.SetCurrentLevel(
            nextLevel
        );

        // -----------------------------------------------------
        // VERIFY CURRENT LEVEL
        // -----------------------------------------------------

        int savedLevel =
            GameLevelManager.Instance.GetCurrentLevel();

        Debug.Log(
            "CURRENT LEVEL AFTER SAVE = " +
            savedLevel
        );

        // -----------------------------------------------------
        // SAVE PLAYER PREFS
        // -----------------------------------------------------

        PlayerPrefs.Save();

        Debug.Log(
            "LEVEL " +
            nextLevel +
            " SAVED SUCCESSFULLY"
        );

        Debug.Log(
            "LOADING GAME SCENE..."
        );

        Debug.Log("====================================");

        // -----------------------------------------------------
        // LOAD SAME GAME SCENE
        // -----------------------------------------------------

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // MAIN MENU
    // =========================================================

    public void MainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(0);
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
    // GET RED GEMS
    // =========================================================

    public int GetRedGemsCollected()
    {
        return redGemsCollected;
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
        ] = gemType;

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
                    boardText += "X ";
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

        Debug.Log(boardText);
    }
}