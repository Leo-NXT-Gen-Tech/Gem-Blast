using UnityEngine;

[CreateAssetMenu(
    fileName = "Level_1",
    menuName = "Candy Crush/Level Data"
)]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber = 1;

    [Header("Board Size")]
    public int rows = 8;
    public int columns = 8;

    [Header("Level Settings")]
    public int moves = 30;
    public int redGoal = 20;

    [Header("Board Gems")]
    public GemType[] gems;

    [Header("Board Shape")]
    public bool[] activeCells;

    // =========================================
    // INITIALIZE
    // =========================================

    public void Initialize()
    {
        int totalCells = rows * columns;

        // Gems
        if (gems == null || gems.Length != totalCells)
        {
            gems = new GemType[totalCells];

            for (int i = 0; i < gems.Length; i++)
            {
                gems[i] = GemType.Red;
            }
        }

        // Board Shape
        if (activeCells == null ||
            activeCells.Length != totalCells)
        {
            activeCells = new bool[totalCells];

            for (int i = 0; i < activeCells.Length; i++)
            {
                activeCells[i] = true;
            }
        }
    }

    // =========================================
    // VALID CELL
    // =========================================

    public bool IsValidCell(int row, int column)
    {
        if (row < 0 ||
            row >= rows ||
            column < 0 ||
            column >= columns)
        {
            return false;
        }

        Initialize();

        int index =
            row * columns + column;

        return activeCells[index];
    }

    // =========================================
    // GET GEM
    // =========================================

    public GemType GetGem(int row, int column)
    {
        Initialize();

        if (row < 0 ||
            row >= rows ||
            column < 0 ||
            column >= columns)
        {
            return GemType.Red;
        }

        int index =
            row * columns + column;

        if (index < 0 ||
            index >= gems.Length)
        {
            return GemType.Red;
        }

        return gems[index];
    }

    // =========================================
    // SET GEM
    // =========================================

    public void SetGem(
        int row,
        int column,
        GemType gemType)
    {
        Initialize();

        if (row < 0 ||
            row >= rows ||
            column < 0 ||
            column >= columns)
        {
            return;
        }

        int index =
            row * columns + column;

        if (index < 0 ||
            index >= gems.Length)
        {
            return;
        }

        gems[index] = gemType;

        activeCells[index] = true;
    }

    // =========================================
    // SET CELL ACTIVE
    // =========================================

    public void SetCellActive(
        int row,
        int column,
        bool active)
    {
        Initialize();

        if (row < 0 ||
            row >= rows ||
            column < 0 ||
            column >= columns)
        {
            return;
        }

        int index =
            row * columns + column;

        if (index < 0 ||
            index >= activeCells.Length)
        {
            return;
        }

        activeCells[index] = active;
    }

    // =========================================
    // CLEAR BOARD
    // =========================================

    public void ClearBoard()
    {
        Initialize();

        for (int i = 0; i < gems.Length; i++)
        {
            gems[i] = GemType.Red;
            activeCells[i] = true;
        }
    }

    // =========================================
    // CHANGE BOARD SIZE
    // =========================================

    public void SetBoardSize(
        int newRows,
        int newColumns)
    {
        newRows = Mathf.Max(1, newRows);
        newColumns = Mathf.Max(1, newColumns);

        rows = newRows;
        columns = newColumns;

        int totalCells =
            rows * columns;

        gems = new GemType[totalCells];
        activeCells = new bool[totalCells];

        for (int i = 0; i < totalCells; i++)
        {
            gems[i] = GemType.Red;
            activeCells[i] = true;
        }
    }
}