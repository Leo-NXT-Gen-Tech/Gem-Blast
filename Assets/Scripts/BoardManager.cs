using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BoardManager : MonoBehaviour
{
    [Header("Board Size")]
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;

    [Header("Gem Setup")]
    [SerializeField] private GameObject gemPrefab;
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

    private GemType[,] board;
    private GemView[,] gemViews;

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

    private void Start()
    {
        if (isDestroyed)
            return;

        Time.timeScale = 1f;

        moves = startingMoves;
        score = 0;
        redGemsCollected = 0;

        levelCompleted = false;
        levelFailed = false;

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        if (levelFailedPanel != null)
            levelFailedPanel.SetActive(false);

        InitializeBoard();
        GenerateRandomBoard();
        GenerateVisualBoard();
        UpdateUI();
        PrintBoard();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        StopAllCoroutines();
    }

    private void InitializeBoard()
    {
        board = new GemType[rows, columns];
        gemViews = new GemView[rows, columns];
    }

    private GemType GetRandomGem()
    {
        return (GemType)Random.Range(0, 6);
    }

    private void GenerateRandomBoard()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                board[row, column] = GetRandomGem();
            }
        }

        Debug.Log("8x8 Board Generated!");
    }

    private void GenerateVisualBoard()
    {
        if (gemPrefab == null)
        {
            Debug.LogError("Gem Prefab is not assigned!");
            return;
        }

        if (boardPanel == null)
        {
            Debug.LogError("Board Panel is not assigned!");
            return;
        }

        for (int i = boardPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(boardPanel.GetChild(i).gameObject);
        }

        RectTransform panelRect =
            boardPanel.GetComponent<RectTransform>();

        if (panelRect == null)
        {
            Debug.LogError("Board Panel needs RectTransform!");
            return;
        }

        float panelWidth = panelRect.rect.width;
        float panelHeight = panelRect.rect.height;

        cellWidth = panelWidth / columns;
        cellHeight = panelHeight / rows;

        gemSize =
            Mathf.Min(cellWidth, cellHeight) * 0.85f;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                CreateGem(row, column, false);
            }
        }

        Debug.Log("64 Gems Created!");
    }

    private GemView CreateGem(
        int row,
        int column,
        bool startAboveBoard)
    {
        if (isDestroyed)
            return null;

        GameObject gemObject =
            Instantiate(gemPrefab, boardPanel);

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
                new Vector2(gemSize, gemSize);

            Vector2 targetPosition =
                GetGridPosition(row, column);

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
                "GemView missing on Gem Prefab!"
            );

            Destroy(gemObject);

            return null;
        }

        gemView.SetGem(
            board[row, column],
            row,
            column
        );

        gemView.SetBoardManager(this);

        gemViews[row, column] =
            gemView;

        return gemView;
    }

    private Vector2 GetGridPosition(
        int row,
        int column)
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

    public void OnGemDragged(
        GemView draggedGem,
        Vector2 direction)
    {
        if (isDestroyed)
            return;

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
            targetRow < 0 ||
            targetRow >= rows ||
            targetColumn < 0 ||
            targetColumn >= columns
        )
        {
            return;
        }

        GemView targetGem =
            gemViews[targetRow, targetColumn];

        if (targetGem == null)
            return;

        StartCoroutine(
            SwapAndProcess(
                draggedGem,
                targetGem
            )
        );
    }

    private IEnumerator SwapAndProcess(
        GemView gem1,
        GemView gem2)
    {
        if (isDestroyed)
            yield break;

        if (isProcessing)
            yield break;

        if (gem1 == null || gem2 == null)
            yield break;

        isProcessing = true;

        int row1 = gem1.GetRow();
        int column1 = gem1.GetColumn();

        int row2 = gem2.GetRow();
        int column2 = gem2.GetColumn();

        GemType type1 =
            board[row1, column1];

        GemType type2 =
            board[row2, column2];

        RectTransform rect1 =
            gem1.GetComponent<RectTransform>();

        RectTransform rect2 =
            gem2.GetComponent<RectTransform>();

        Vector2 position1 =
            GetGridPosition(row1, column1);

        Vector2 position2 =
            GetGridPosition(row2, column2);

        moves--;
        UpdateUI();

        board[row1, column1] = type2;
        board[row2, column2] = type1;

        gemViews[row1, column1] = gem2;
        gemViews[row2, column2] = gem1;

        gem1.SetGridPosition(
            row2,
            column2
        );

        gem2.SetGridPosition(
            row1,
            column1
        );

        gem1.SetColor(type2);
        gem2.SetColor(type1);

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
            {
                points = 30;
            }
            else if (matches.Count == 4)
            {
                points = 50;
            }
            else
            {
                points = 100;
            }

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

            board[row1, column1] = type1;
            board[row2, column2] = type2;

            gemViews[row1, column1] = gem1;
            gemViews[row2, column2] = gem2;

            gem1.SetGridPosition(
                row1,
                column1
            );

            gem2.SetGridPosition(
                row2,
                column2
            );

            gem1.SetColor(type1);
            gem2.SetColor(type2);

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

            Debug.Log(
                "SWAP BACK SUCCESS!"
            );
        }

        if (
            !levelCompleted &&
            moves <= 0
        )
        {
            ShowLevelFailed();
        }

        isProcessing = false;
    }

    private IEnumerator AnimateGemSwap(
        RectTransform gem1Rect,
        Vector2 gem1Start,
        Vector2 gem1Target,
        RectTransform gem2Rect,
        Vector2 gem2Start,
        Vector2 gem2Target)
    {
        if (gem1Rect == null ||
            gem2Rect == null)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < swapDuration)
        {
            if (isDestroyed)
                yield break;

            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / swapDuration
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

    private List<Vector2Int> FindAllMatches()
    {
        List<Vector2Int> matches =
            new List<Vector2Int>();

        for (int row = 0; row < rows; row++)
        {
            int startColumn = 0;

            while (startColumn < columns)
            {
                GemType type =
                    board[row, startColumn];

                int count = 1;

                int column =
                    startColumn + 1;

                while (
                    column < columns &&
                    board[row, column] == type
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
                        c++)
                    {
                        Vector2Int position =
                            new Vector2Int(row, c);

                        if (!matches.Contains(position))
                        {
                            matches.Add(position);
                        }
                    }
                }

                startColumn = column;
            }
        }

        for (int column = 0; column < columns; column++)
        {
            int startRow = 0;

            while (startRow < rows)
            {
                GemType type =
                    board[startRow, column];

                int count = 1;

                int row =
                    startRow + 1;

                while (
                    row < rows &&
                    board[row, column] == type
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
                        r++)
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

                startRow = row;
            }
        }

        return matches;
    }

    private IEnumerator RemoveAndFill(
        List<Vector2Int> matches)
    {
        if (isDestroyed)
            yield break;

        int redCountThisMatch = 0;

        foreach (Vector2Int position in matches)
        {
            int row = position.x;
            int column = position.y;

            if (
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

        foreach (Vector2Int position in matches)
        {
            int row = position.x;
            int column = position.y;

            if (gemViews[row, column] != null)
            {
                Destroy(
                    gemViews[row, column].gameObject
                );

                gemViews[row, column] = null;
            }
        }

        yield return new WaitForSeconds(0.1f);

        for (
            int column = 0;
            column < columns;
            column++)
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
            Debug.Log(
                "NEW MATCH: " +
                newMatches.Count
            );

            int chainPoints;

            if (newMatches.Count == 3)
            {
                chainPoints = 30;
            }
            else if (newMatches.Count == 4)
            {
                chainPoints = 50;
            }
            else
            {
                chainPoints = 100;
            }

            AddScore(chainPoints);

            yield return StartCoroutine(
                RemoveAndFill(
                    newMatches
                )
            );
        }
    }

    private void AddRedGemsCollected(
        int amount)
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

    private IEnumerator CollapseColumn(
        int column)
    {
        if (isDestroyed)
            yield break;

        List<GemType> remainingTypes =
            new List<GemType>();

        List<GemView> remainingViews =
            new List<GemView>();

        for (
            int row = 0;
            row < rows;
            row++)
        {
            if (gemViews[row, column] != null)
            {
                remainingTypes.Add(
                    board[row, column]
                );

                remainingViews.Add(
                    gemViews[row, column]
                );
            }
        }

        int existingCount =
            remainingViews.Count;

        GemView[] newViews =
            new GemView[rows];

        GemType[] newTypes =
            new GemType[rows];

        for (
            int row = 0;
            row < existingCount;
            row++)
        {
            GemView gem =
                remainingViews[row];

            GemType type =
                remainingTypes[row];

            newViews[row] = gem;
            newTypes[row] = type;

            if (gem != null)
            {
                gem.SetGridPosition(
                    row,
                    column
                );
            }
        }

        int newGemCount =
            rows - existingCount;

        for (
            int i = 0;
            i < newGemCount;
            i++)
        {
            int row =
                existingCount + i;

            GemType newType =
                GetRandomGem();

            newTypes[row] =
                newType;

            board[row, column] =
                newType;

            GemView newGem =
                CreateGem(
                    row,
                    column,
                    true
                );

            newViews[row] =
                newGem;
        }

        for (
            int row = 0;
            row < rows;
            row++)
        {
            board[row, column] =
                newTypes[row];

            gemViews[row, column] =
                newViews[row];
        }

        for (
            int row = 0;
            row < rows;
            row++)
        {
            GemView gem =
                newViews[row];

            if (gem != null)
            {
                StartCoroutine(
                    MoveGemToPosition(
                        gem,
                        GetGridPosition(
                            row,
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

    private IEnumerator MoveGemToPosition(
        GemView gem,
        Vector2 targetPosition)
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

        while (timer < fallDuration)
        {
            if (isDestroyed)
                yield break;

            if (gem == null)
                yield break;

            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fallDuration
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

    private void AddScore(int amount)
    {
        score += amount;
        UpdateUI();

        Debug.Log(
            "Score: " +
            score
        );
    }

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

    private void ShowLevelComplete()
    {
        if (levelCompleted)
            return;

        levelCompleted = true;

        Debug.Log(
            "LEVEL COMPLETE! RED GOAL COMPLETED!"
        );

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
    }

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

        Debug.Log(
            "LEVEL FAILED! MOVES FINISHED!"
        );

        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
        }
    }


    // =========================================
    // RETRY CURRENT LEVEL
    // =========================================

    public void RetryLevel()
    {
        if (isDestroyed)
            return;

        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }


    // =========================================
    // NEXT LEVEL
    // =========================================

    public void NextLevel()
    {
        if (isDestroyed)
            return;

        Time.timeScale = 1f;

        int currentLevel =
            SceneManager.GetActiveScene().buildIndex;

        int nextLevel =
            currentLevel + 1;

        if (
            nextLevel <
            SceneManager.sceneCountInBuildSettings
        )
        {
            SceneManager.LoadScene(
                nextLevel
            );
        }
        else
        {
            Debug.Log(
                "No more levels available!"
            );
        }
    }


    // =========================================
    // MAIN MENU
    // =========================================

    public void MainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(0);
    }


    public int GetScore()
    {
        return score;
    }

    public int GetMoves()
    {
        return moves;
    }

    public int GetRedGemsCollected()
    {
        return redGemsCollected;
    }

    public GemType GetCell(
        int row,
        int column)
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

        return board[row, column];
    }

    public void SetCell(
        int row,
        int column,
        GemType gemType)
    {
        if (
            !IsValidCell(
                row,
                column
            )
        )
        {
            return;
        }

        board[row, column] =
            gemType;

        if (
            gemViews[row, column] != null
        )
        {
            gemViews[row, column]
                .SetColor(gemType);
        }
    }

    public bool IsValidCell(
        int row,
        int column)
    {
        return
            row >= 0 &&
            row < rows &&
            column >= 0 &&
            column < columns;
    }

    public int GetRows()
    {
        return rows;
    }

    public int GetColumns()
    {
        return columns;
    }

    private void PrintBoard()
    {
        string boardText = "";

        for (
            int row = rows - 1;
            row >= 0;
            row--)
        {
            for (
                int column = 0;
                column < columns;
                column++)
            {
                boardText +=
                    board[row, column] +
                    " ";
            }

            boardText += "\n";
        }

        Debug.Log(boardText);
    }
}