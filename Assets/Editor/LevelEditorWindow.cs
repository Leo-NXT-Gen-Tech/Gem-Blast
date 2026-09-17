#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class LevelEditorWindow : EditorWindow
{
    // =========================================================
    // SETTINGS
    // =========================================================

    private const int MaxLevels = 50;

    private int selectedLevel = 1;

    private LevelData currentLevelData;

    private GemType selectedGem = GemType.Red;

    // Main window scroll
    private Vector2 windowScrollPosition;

    // Board scroll
    private Vector2 boardScrollPosition;

    private bool shapeEditMode = false;

    private int editRows = 8;
    private int editColumns = 8;


    // =========================================================
    // OPEN WINDOW
    // =========================================================

    [MenuItem("Gem Blast/Level Editor")]
    public static void OpenWindow()
    {
        LevelEditorWindow window =
            GetWindow<LevelEditorWindow>(
                "Gem Blast Level Editor"
            );

        window.minSize =
            new Vector2(760f, 780f);

        window.Show();
    }


    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        LoadLevel(selectedLevel);
    }


    // =========================================================
    // GUI
    // =========================================================

    private void OnGUI()
    {
        // =====================================================
        // FULL WINDOW SCROLL
        // =====================================================

        windowScrollPosition =
            EditorGUILayout.BeginScrollView(
                windowScrollPosition
            );


        EditorGUILayout.Space(8);


        // =====================================================
        // TITLE
        // =====================================================

        GUILayout.Label(
            "GEM BLAST LEVEL EDITOR",
            EditorStyles.boldLabel
        );


        EditorGUILayout.Space(8);


        // =====================================================
        // LEVEL SELECTOR
        // =====================================================

        DrawLevelSelector();


        EditorGUILayout.Space(10);


        // =====================================================
        // LEVEL NOT FOUND
        // =====================================================

        if (currentLevelData == null)
        {
            EditorGUILayout.HelpBox(
                "Level data not found. Create the level first.",
                MessageType.Warning
            );


            if (GUILayout.Button(
                "CREATE LEVEL " + selectedLevel,
                GUILayout.Height(40)))
            {
                CreateLevel(selectedLevel);
            }


            EditorGUILayout.Space(20);


            EditorGUILayout.EndScrollView();

            return;
        }


        // =====================================================
        // LEVEL SETTINGS
        // =====================================================

        DrawLevelSettings();


        EditorGUILayout.Space(10);


        // =====================================================
        // BOARD SIZE
        // =====================================================

        DrawBoardSizeSettings();


        EditorGUILayout.Space(10);


        // =====================================================
        // TOOLS
        // =====================================================

        DrawTools();


        EditorGUILayout.Space(10);


        // =====================================================
        // BOARD
        // =====================================================

        DrawBoard();


        EditorGUILayout.Space(15);


        // =====================================================
        // SAVE / CLEAR / RANDOMIZE
        // =====================================================

        DrawButtons();


        EditorGUILayout.Space(20);


        EditorGUILayout.EndScrollView();
    }


    // =========================================================
    // LEVEL SELECTOR
    // =========================================================

    private void DrawLevelSelector()
    {
        EditorGUILayout.LabelField(
            "LEVEL SELECTOR",
            EditorStyles.boldLabel
        );


        EditorGUILayout.BeginHorizontal();


        GUILayout.Label(
            "LEVEL",
            GUILayout.Width(60)
        );


        int newLevel =
            EditorGUILayout.IntSlider(
                selectedLevel,
                1,
                MaxLevels
            );


        if (newLevel != selectedLevel)
        {
            selectedLevel =
                newLevel;

            LoadLevel(
                selectedLevel
            );
        }


        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space(5);


        // =====================================================
        // PREVIOUS / NEXT
        // =====================================================

        EditorGUILayout.BeginHorizontal();


        if (GUILayout.Button(
            "◀ PREVIOUS",
            GUILayout.Height(30)))
        {
            if (selectedLevel > 1)
            {
                selectedLevel--;

                LoadLevel(
                    selectedLevel
                );
            }
        }


        if (GUILayout.Button(
            "NEXT ▶",
            GUILayout.Height(30)))
        {
            if (selectedLevel < MaxLevels)
            {
                selectedLevel++;

                LoadLevel(
                    selectedLevel
                );
            }
        }


        EditorGUILayout.EndHorizontal();
    }


    // =========================================================
    // LEVEL SETTINGS
    // =========================================================

    private void DrawLevelSettings()
    {
        EditorGUILayout.LabelField(
            "LEVEL SETTINGS",
            EditorStyles.boldLabel
        );


        EditorGUILayout.Space(3);


        // =====================================================
        // MOVES
        // =====================================================

        currentLevelData.moves =
            EditorGUILayout.IntField(
                "Moves",
                currentLevelData.moves
            );


        // =====================================================
        // 6 GOALS
        // =====================================================

        currentLevelData.redGoal =
            EditorGUILayout.IntField(
                "Red Gem Goal",
                currentLevelData.redGoal
            );


        currentLevelData.blueGoal =
            EditorGUILayout.IntField(
                "Blue Gem Goal",
                currentLevelData.blueGoal
            );


        currentLevelData.greenGoal =
            EditorGUILayout.IntField(
                "Green Gem Goal",
                currentLevelData.greenGoal
            );


        currentLevelData.pinkGoal =
            EditorGUILayout.IntField(
                "Pink Gem Goal",
                currentLevelData.pinkGoal
            );


        currentLevelData.purpleGoal =
            EditorGUILayout.IntField(
                "Purple Gem Goal",
                currentLevelData.purpleGoal
            );


        currentLevelData.orangeGoal =
            EditorGUILayout.IntField(
                "Orange Gem Goal",
                currentLevelData.orangeGoal
            );


        // =====================================================
        // LIMIT VALUES
        // =====================================================

        currentLevelData.moves =
            Mathf.Max(
                1,
                currentLevelData.moves
            );


        currentLevelData.redGoal =
            Mathf.Max(
                0,
                currentLevelData.redGoal
            );


        currentLevelData.blueGoal =
            Mathf.Max(
                0,
                currentLevelData.blueGoal
            );


        currentLevelData.greenGoal =
            Mathf.Max(
                0,
                currentLevelData.greenGoal
            );


        currentLevelData.pinkGoal =
            Mathf.Max(
                0,
                currentLevelData.pinkGoal
            );


        currentLevelData.purpleGoal =
            Mathf.Max(
                0,
                currentLevelData.purpleGoal
            );


        currentLevelData.orangeGoal =
            Mathf.Max(
                0,
                currentLevelData.orangeGoal
            );
    }


    // =========================================================
    // BOARD SIZE
    // =========================================================

    private void DrawBoardSizeSettings()
    {
        EditorGUILayout.LabelField(
            "BOARD SIZE",
            EditorStyles.boldLabel
        );


        EditorGUILayout.Space(3);


        EditorGUILayout.BeginHorizontal();


        editRows =
            EditorGUILayout.IntField(
                "Rows",
                editRows
            );


        editColumns =
            EditorGUILayout.IntField(
                "Columns",
                editColumns
            );


        EditorGUILayout.EndHorizontal();


        // =====================================================
        // LIMIT
        // =====================================================

        editRows =
            Mathf.Clamp(
                editRows,
                1,
                12
            );


        editColumns =
            Mathf.Clamp(
                editColumns,
                1,
                12
            );


        EditorGUILayout.Space(5);


        if (GUILayout.Button(
            "APPLY BOARD SIZE",
            GUILayout.Height(35)))
        {
            ApplyBoardSize();
        }


        EditorGUILayout.HelpBox(
            "Set Rows and Columns separately. Example: 7 × 7, 7 × 5, 8 × 6, 9 × 5.",
            MessageType.Info
        );
    }


    // =========================================================
    // TOOLS
    // =========================================================

    private void DrawTools()
    {
        EditorGUILayout.LabelField(
            "EDITOR TOOLS",
            EditorStyles.boldLabel
        );


        EditorGUILayout.Space(3);


        // =====================================================
        // GEM / SHAPE MODE
        // =====================================================

        EditorGUILayout.BeginHorizontal();


        // -----------------------------------------------------
        // GEM MODE
        // -----------------------------------------------------

        Color oldColor =
            GUI.backgroundColor;


        GUI.backgroundColor =
            !shapeEditMode
                ? new Color(
                    0.35f,
                    1f,
                    0.35f
                )
                : Color.white;


        if (GUILayout.Button(
            "GEM MODE",
            GUILayout.Height(38)))
        {
            shapeEditMode =
                false;
        }


        // -----------------------------------------------------
        // SHAPE MODE
        // -----------------------------------------------------

        GUI.backgroundColor =
            shapeEditMode
                ? new Color(
                    1f,
                    0.70f,
                    0.30f
                )
                : Color.white;


        if (GUILayout.Button(
            "SHAPE MODE",
            GUILayout.Height(38)))
        {
            shapeEditMode =
                true;
        }


        GUI.backgroundColor =
            oldColor;


        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space(8);


        // =====================================================
        // GEM MODE
        // =====================================================

        if (!shapeEditMode)
        {
            DrawGemPalette();
        }
        else
        {
            EditorGUILayout.HelpBox(
                "SHAPE MODE: Click any cell to enable / disable that cell.",
                MessageType.Warning
            );
        }
    }


    // =========================================================
    // GEM PALETTE
    // =========================================================

    private void DrawGemPalette()
    {
        EditorGUILayout.LabelField(
            "SELECT GEM COLOR",
            EditorStyles.boldLabel
        );


        EditorGUILayout.Space(4);


        EditorGUILayout.BeginHorizontal();


        DrawGemButton(
            GemType.Red,
            "RED"
        );


        DrawGemButton(
            GemType.Blue,
            "BLUE"
        );


        DrawGemButton(
            GemType.Green,
            "GREEN"
        );


        DrawGemButton(
            GemType.Pink,
            "PINK"
        );


        DrawGemButton(
            GemType.Purple,
            "PURPLE"
        );


        DrawGemButton(
            GemType.Orange,
            "ORANGE"
        );


        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space(6);


        // =====================================================
        // SELECTED COLOR
        // =====================================================

        EditorGUILayout.BeginHorizontal();


        GUILayout.Label(
            "Selected:",
            GUILayout.Width(60)
        );


        Color oldColor =
            GUI.backgroundColor;


        GUI.backgroundColor =
            GetGemColor(
                selectedGem
            );


        GUIStyle selectedStyle =
            new GUIStyle(
                EditorStyles.boldLabel
            );


        selectedStyle.alignment =
            TextAnchor.MiddleCenter;


        GUILayout.Label(
            GetGemShortName(
                selectedGem
            ),
            selectedStyle,
            GUILayout.Width(90),
            GUILayout.Height(24)
        );


        GUI.backgroundColor =
            oldColor;


        EditorGUILayout.EndHorizontal();


        EditorGUILayout.HelpBox(
            "Select one of the 6 colors and click any active cell on the board to change its color.",
            MessageType.Info
        );
    }


    // =========================================================
    // GEM BUTTON
    // =========================================================

    private void DrawGemButton(
        GemType gem,
        string label)
    {
        bool selected =
            selectedGem == gem;


        Color oldColor =
            GUI.backgroundColor;


        Color gemColor =
            GetGemColor(
                gem
            );


        if (selected)
        {
            GUI.backgroundColor =
                new Color(
                    Mathf.Min(
                        gemColor.r + 0.15f,
                        1f
                    ),
                    Mathf.Min(
                        gemColor.g + 0.15f,
                        1f
                    ),
                    Mathf.Min(
                        gemColor.b + 0.15f,
                        1f
                    )
                );
        }
        else
        {
            GUI.backgroundColor =
                gemColor;
        }


        GUIStyle style =
            new GUIStyle(
                GUI.skin.button
            );


        style.alignment =
            TextAnchor.MiddleCenter;


        style.fontStyle =
            selected
                ? FontStyle.Bold
                : FontStyle.Normal;


        if (GUILayout.Button(
            label,
            style,
            GUILayout.Height(38)))
        {
            selectedGem =
                gem;

            Repaint();
        }


        GUI.backgroundColor =
            oldColor;
    }


    // =========================================================
    // BOARD
    // =========================================================

    private void DrawBoard()
    {
        EditorGUILayout.LabelField(
            "LEVEL BOARD   " +
            currentLevelData.rows +
            " × " +
            currentLevelData.columns,
            EditorStyles.boldLabel
        );


        EditorGUILayout.Space(5);


        int rows =
            Mathf.Max(
                1,
                currentLevelData.rows
            );


        int columns =
            Mathf.Max(
                1,
                currentLevelData.columns
            );


        // =====================================================
        // CELL SETTINGS
        // =====================================================

        float cellSize =
            58f;


        float gap =
            3f;


        float boardWidth =
            columns * cellSize +
            (columns - 1) * gap;


        float boardHeight =
            rows * cellSize +
            (rows - 1) * gap;


        // =====================================================
        // BOARD HOLDER
        // =====================================================

        GUIStyle boardStyle =
            new GUIStyle(
                GUI.skin.box
            );


        boardStyle.padding =
            new RectOffset(
                10,
                10,
                10,
                10
            );


        EditorGUILayout.BeginVertical(
            boardStyle
        );


        // =====================================================
        // BOARD SCROLL
        // =====================================================

        boardScrollPosition =
            EditorGUILayout.BeginScrollView(
                boardScrollPosition,
                GUILayout.MinHeight(
                    Mathf.Min(
                        boardHeight + 25f,
                        430f
                    )
                ),
                GUILayout.MaxHeight(430f)
            );


        // =====================================================
        // BOARD RECT
        // =====================================================

        float availableWidth =
            position.width - 55f;


        float contentWidth =
            Mathf.Max(
                boardWidth + 20f,
                availableWidth
            );


        Rect boardArea =
            GUILayoutUtility.GetRect(
                contentWidth,
                boardHeight + 20f,
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false)
            );


        // =====================================================
        // CENTER BOARD
        // =====================================================

        float startX =
            boardArea.x +
            Mathf.Max(
                10f,
                (boardArea.width - boardWidth) / 2f
            );


        float startY =
            boardArea.y + 10f;


        // =====================================================
        // DRAW GRID
        // =====================================================

        for (
            int displayRow = 0;
            displayRow < rows;
            displayRow++
        )
        {
            // Top to bottom display
            int dataRow =
                rows -
                1 -
                displayRow;


            for (
                int column = 0;
                column < columns;
                column++
            )
            {
                float x =
                    startX +
                    column *
                    (cellSize + gap);


                float y =
                    startY +
                    displayRow *
                    (cellSize + gap);


                Rect cellRect =
                    new Rect(
                        x,
                        y,
                        cellSize,
                        cellSize
                    );


                DrawCell(
                    dataRow,
                    column,
                    cellRect
                );
            }
        }


        GUILayout.Space(
            boardHeight + 5f
        );


        EditorGUILayout.EndScrollView();


        EditorGUILayout.EndVertical();


        EditorGUILayout.Space(5);


        // =====================================================
        // BOARD INFO
        // =====================================================

        if (shapeEditMode)
        {
            EditorGUILayout.HelpBox(
                "Shape Mode: Click a cell to turn it ON/OFF. X = inactive cell.",
                MessageType.Warning
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Gem Mode: Select a color above and click an active cell.",
                MessageType.Info
            );
        }
    }


    // =========================================================
    // CELL
    // =========================================================

    private void DrawCell(
        int row,
        int column,
        Rect rect)
    {
        // =====================================================
        // SAFETY
        // =====================================================

        if (currentLevelData == null)
            return;


        if (row < 0 ||
            row >= currentLevelData.rows)
            return;


        if (column < 0 ||
            column >= currentLevelData.columns)
            return;


        // =====================================================
        // ACTIVE
        // =====================================================

        bool active =
            currentLevelData.IsValidCell(
                row,
                column
            );


        // =====================================================
        // GEM
        // =====================================================

        GemType gem =
            currentLevelData.GetGem(
                row,
                column
            );


        // =====================================================
        // STYLE
        // =====================================================

        GUIStyle style =
            new GUIStyle(
                GUI.skin.button
            );


        style.alignment =
            TextAnchor.MiddleCenter;


        style.fontStyle =
            FontStyle.Bold;


        style.fontSize =
            9;


        // =====================================================
        // COLOR
        // =====================================================

        Color oldColor =
            GUI.backgroundColor;


        if (active)
        {
            GUI.backgroundColor =
                GetGemColor(
                    gem
                );
        }
        else
        {
            GUI.backgroundColor =
                new Color(
                    0.12f,
                    0.12f,
                    0.12f
                );
        }


        // =====================================================
        // TEXT
        // =====================================================

        string text =
            active
                ? GetGemShortName(
                    gem
                )
                : "X";


        // =====================================================
        // CLICK
        // =====================================================

        if (GUI.Button(
            rect,
            text,
            style))
        {
            // =================================================
            // SHAPE MODE
            // =================================================

            if (shapeEditMode)
            {
                bool newActive =
                    !active;


                currentLevelData.SetCellActive(
                    row,
                    column,
                    newActive
                );


                // -------------------------------------------------
                // NEW ACTIVE CELL
                // -------------------------------------------------

                if (newActive)
                {
                    currentLevelData.SetGem(
                        row,
                        column,
                        selectedGem
                    );
                }
            }


            // =================================================
            // GEM MODE
            // =================================================

            else
            {
                if (active)
                {
                    currentLevelData.SetGem(
                        row,
                        column,
                        selectedGem
                    );
                }
            }


            // =================================================
            // SAVE DIRTY STATE
            // =================================================

            EditorUtility.SetDirty(
                currentLevelData
            );


            GUI.FocusControl(
                null
            );


            Repaint();
        }


        // =====================================================
        // RESET COLOR
        // =====================================================

        GUI.backgroundColor =
            oldColor;
    }


    // =========================================================
    // GEM COLORS
    // =========================================================

    private Color GetGemColor(
        GemType gem)
    {
        switch (gem)
        {
            case GemType.Red:

                return new Color(
                    0.95f,
                    0.15f,
                    0.15f
                );


            case GemType.Blue:

                return new Color(
                    0.15f,
                    0.45f,
                    0.95f
                );


            case GemType.Green:

                return new Color(
                    0.15f,
                    0.75f,
                    0.25f
                );


            case GemType.Pink:

                return new Color(
                    1f,
                    0.20f,
                    0.65f
                );


            case GemType.Purple:

                return new Color(
                    0.55f,
                    0.20f,
                    0.90f
                );


            case GemType.Orange:

                return new Color(
                    1f,
                    0.45f,
                    0.05f
                );
        }


        return Color.white;
    }


    // =========================================================
    // GEM NAME
    // =========================================================

    private string GetGemShortName(
        GemType gem)
    {
        switch (gem)
        {
            case GemType.Red:
                return "RED";


            case GemType.Blue:
                return "BLUE";


            case GemType.Green:
                return "GREEN";


            case GemType.Pink:
                return "PINK";


            case GemType.Purple:
                return "PURPLE";


            case GemType.Orange:
                return "ORANGE";
        }


        return "?";
    }


    // =========================================================
    // BUTTONS
    // =========================================================

    private void DrawButtons()
    {
        EditorGUILayout.LabelField(
            "LEVEL ACTIONS",
            EditorStyles.boldLabel
        );


        EditorGUILayout.Space(5);


        // =====================================================
        // SAVE + CLEAR
        // =====================================================

        EditorGUILayout.BeginHorizontal();


        if (GUILayout.Button(
            "SAVE LEVEL",
            GUILayout.Height(45)))
        {
            SaveLevel();
        }


        if (GUILayout.Button(
            "CLEAR LEVEL",
            GUILayout.Height(45)))
        {
            ClearLevel();
        }


        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space(8);


        // =====================================================
        // RANDOMIZE
        // =====================================================

        if (GUILayout.Button(
            "RANDOMIZE LEVEL",
            GUILayout.Height(38)))
        {
            RandomizeLevel();
        }


        EditorGUILayout.Space(5);


        EditorGUILayout.HelpBox(
            "SAVE LEVEL press karo after editing colors, shape, goals or board size.",
            MessageType.Info
        );
    }


    // =========================================================
    // APPLY BOARD SIZE
    // =========================================================

    private void ApplyBoardSize()
    {
        if (currentLevelData == null)
            return;


        editRows =
            Mathf.Clamp(
                editRows,
                1,
                12
            );


        editColumns =
            Mathf.Clamp(
                editColumns,
                1,
                12
            );


        bool confirm =
            EditorUtility.DisplayDialog(
                "Change Board Size",

                "Changing the board size will reset the current board.\n\n" +
                "New Size: " +
                editRows +
                " × " +
                editColumns +
                "\n\nContinue?",

                "YES",
                "CANCEL"
            );


        if (!confirm)
            return;


        currentLevelData.SetBoardSize(
            editRows,
            editColumns
        );


        EditorUtility.SetDirty(
            currentLevelData
        );


        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();


        boardScrollPosition =
            Vector2.zero;


        Repaint();
    }


    // =========================================================
    // LOAD LEVEL
    // =========================================================

    private void LoadLevel(
        int level)
    {
        string path =
            "Assets/Resources/Levels/Level_" +
            level +
            ".asset";


        currentLevelData =
            AssetDatabase.LoadAssetAtPath<LevelData>(
                path
            );


        if (currentLevelData != null)
        {
            // IMPORTANT:
            // Do NOT call Initialize().
            // Existing board colors and shape remain.

            if (currentLevelData.rows < 1)
            {
                currentLevelData.rows =
                    8;
            }


            if (currentLevelData.columns < 1)
            {
                currentLevelData.columns =
                    8;
            }


            editRows =
                currentLevelData.rows;


            editColumns =
                currentLevelData.columns;


            int totalCells =
                currentLevelData.rows *
                currentLevelData.columns;


            // =================================================
            // GEMS SAFETY
            // =================================================

            if (currentLevelData.gems == null ||
                currentLevelData.gems.Length != totalCells)
            {
                currentLevelData.gems =
                    new GemType[
                        totalCells
                    ];


                for (
                    int i = 0;
                    i < totalCells;
                    i++
                )
                {
                    currentLevelData.gems[i] =
                        GemType.Red;
                }


                EditorUtility.SetDirty(
                    currentLevelData
                );
            }


            // =================================================
            // ACTIVE CELLS SAFETY
            // =================================================

            if (currentLevelData.activeCells == null ||
                currentLevelData.activeCells.Length != totalCells)
            {
                currentLevelData.activeCells =
                    new bool[
                        totalCells
                    ];


                for (
                    int i = 0;
                    i < totalCells;
                    i++
                )
                {
                    currentLevelData.activeCells[i] =
                        true;
                }


                EditorUtility.SetDirty(
                    currentLevelData
                );
            }
        }


        boardScrollPosition =
            Vector2.zero;


        Repaint();
    }


    // =========================================================
    // CREATE LEVEL
    // =========================================================

    private void CreateLevel(
        int level)
    {
        EnsureFolderExists();


        string path =
            "Assets/Resources/Levels/Level_" +
            level +
            ".asset";


        LevelData existing =
            AssetDatabase.LoadAssetAtPath<LevelData>(
                path
            );


        // =====================================================
        // EXISTING LEVEL
        // =====================================================

        if (existing != null)
        {
            currentLevelData =
                existing;


            if (currentLevelData.rows < 1)
            {
                currentLevelData.rows =
                    8;
            }


            if (currentLevelData.columns < 1)
            {
                currentLevelData.columns =
                    8;
            }


            editRows =
                currentLevelData.rows;


            editColumns =
                currentLevelData.columns;


            LoadLevel(
                level
            );


            return;
        }


        // =====================================================
        // CREATE NEW LEVEL
        // =====================================================

        LevelData data =
            ScriptableObject.CreateInstance<LevelData>();


        data.levelNumber =
            level;


        data.rows =
            8;


        data.columns =
            8;


        data.moves =
            30;


        // =====================================================
        // DEFAULT GOALS
        // =====================================================

        data.redGoal =
            20;


        data.blueGoal =
            0;


        data.greenGoal =
            0;


        data.pinkGoal =
            0;


        data.purpleGoal =
            0;


        data.orangeGoal =
            0;


        // =====================================================
        // BOARD
        // =====================================================

        int totalCells =
            data.rows *
            data.columns;


        data.gems =
            new GemType[
                totalCells
            ];


        data.activeCells =
            new bool[
                totalCells
            ];


        for (
            int i = 0;
            i < totalCells;
            i++
        )
        {
            data.gems[i] =
                GemType.Red;


            data.activeCells[i] =
                true;
        }


        // =====================================================
        // CREATE ASSET
        // =====================================================

        AssetDatabase.CreateAsset(
            data,
            path
        );


        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();


        currentLevelData =
            AssetDatabase.LoadAssetAtPath<LevelData>(
                path
            );


        editRows =
            8;


        editColumns =
            8;


        boardScrollPosition =
            Vector2.zero;


        Debug.Log(
            "Level " +
            level +
            " created!"
        );


        Repaint();
    }


    // =========================================================
    // SAVE
    // =========================================================

    private void SaveLevel()
    {
        if (currentLevelData == null)
            return;


        EditorUtility.SetDirty(
            currentLevelData
        );


        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();


        Debug.Log(
            "LEVEL " +
            selectedLevel +
            " SAVED!"
        );


        Repaint();
    }


    // =========================================================
    // CLEAR
    // =========================================================

    private void ClearLevel()
    {
        if (currentLevelData == null)
            return;


        bool confirm =
            EditorUtility.DisplayDialog(
                "Clear Level",

                "Reset all cells in Level " +
                selectedLevel +
                "?",

                "YES",
                "CANCEL"
            );


        if (!confirm)
            return;


        currentLevelData.ClearBoard();


        EditorUtility.SetDirty(
            currentLevelData
        );


        AssetDatabase.SaveAssets();


        Repaint();
    }


    // =========================================================
    // RANDOMIZE
    // =========================================================

    private void RandomizeLevel()
    {
        if (currentLevelData == null)
            return;


        if (currentLevelData.gems == null ||
            currentLevelData.activeCells == null)
        {
            return;
        }


        // IMPORTANT:
        // Shape remains unchanged.

        for (
            int i = 0;
            i < currentLevelData.gems.Length;
            i++
        )
        {
            if (currentLevelData.activeCells[i])
            {
                currentLevelData.gems[i] =
                    (GemType)Random.Range(
                        0,
                        6
                    );
            }
        }


        EditorUtility.SetDirty(
            currentLevelData
        );


        AssetDatabase.SaveAssets();


        Repaint();
    }


    // =========================================================
    // FOLDER
    // =========================================================

    private void EnsureFolderExists()
    {
        if (!AssetDatabase.IsValidFolder(
            "Assets/Resources"))
        {
            AssetDatabase.CreateFolder(
                "Assets",
                "Resources"
            );
        }


        if (!AssetDatabase.IsValidFolder(
            "Assets/Resources/Levels"))
        {
            AssetDatabase.CreateFolder(
                "Assets/Resources",
                "Levels"
            );
        }
    }
}

#endif