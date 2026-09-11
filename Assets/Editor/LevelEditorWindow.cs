#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class LevelEditorWindow : EditorWindow
{
    private const int MaxLevels = 50;

    private int selectedLevel = 1;

    private LevelData currentLevelData;

    private GemType selectedGem = GemType.Red;

    private Vector2 scrollPosition;

    private bool shapeEditMode = false;

    private int editRows = 8;
    private int editColumns = 8;

    [MenuItem("Gem Blast/Level Editor")]
    public static void OpenWindow()
    {
        LevelEditorWindow window =
            GetWindow<LevelEditorWindow>(
                "Gem Blast Level Editor"
            );

        window.minSize =
            new Vector2(700f, 750f);

        window.Show();
    }

    private void OnEnable()
    {
        LoadLevel(selectedLevel);
    }

    // =========================================
    // GUI
    // =========================================

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        GUILayout.Label(
            "GEM BLAST LEVEL EDITOR",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(10);

        DrawLevelSelector();

        EditorGUILayout.Space(10);

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

            return;
        }

        DrawLevelSettings();

        EditorGUILayout.Space(15);

        DrawBoardSizeSettings();

        EditorGUILayout.Space(15);

        DrawTools();

        EditorGUILayout.Space(10);

        DrawBoard();

        EditorGUILayout.Space(15);

        DrawButtons();
    }

    // =========================================
    // LEVEL SELECTOR
    // =========================================

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
            selectedLevel = newLevel;

            LoadLevel(selectedLevel);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("◀ PREVIOUS"))
        {
            if (selectedLevel > 1)
            {
                selectedLevel--;

                LoadLevel(selectedLevel);
            }
        }

        if (GUILayout.Button("NEXT ▶"))
        {
            if (selectedLevel < MaxLevels)
            {
                selectedLevel++;

                LoadLevel(selectedLevel);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    // =========================================
    // LEVEL SETTINGS
    // =========================================

    private void DrawLevelSettings()
    {
        EditorGUILayout.LabelField(
            "LEVEL SETTINGS",
            EditorStyles.boldLabel
        );

        currentLevelData.moves =
            EditorGUILayout.IntField(
                "Moves",
                currentLevelData.moves
            );

        currentLevelData.redGoal =
            EditorGUILayout.IntField(
                "Red Gem Goal",
                currentLevelData.redGoal
            );

        currentLevelData.moves =
            Mathf.Max(
                1,
                currentLevelData.moves
            );

        currentLevelData.redGoal =
            Mathf.Max(
                1,
                currentLevelData.redGoal
            );
    }

    // =========================================
    // BOARD SIZE
    // =========================================

    private void DrawBoardSizeSettings()
    {
        EditorGUILayout.LabelField(
            "BOARD SIZE",
            EditorStyles.boldLabel
        );

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
            "You can make different board sizes for every level. Example: 8×8, 8×6, 7×7, 9×5.",
            MessageType.Info
        );
    }

    // =========================================
    // TOOLS
    // =========================================

    private void DrawTools()
    {
        EditorGUILayout.LabelField(
            "EDITOR TOOLS",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        if (!shapeEditMode)
        {
            GUI.backgroundColor =
                new Color(0.3f, 1f, 0.3f);
        }

        if (GUILayout.Button(
            "GEM MODE",
            GUILayout.Height(35)))
        {
            shapeEditMode = false;
        }

        GUI.backgroundColor = Color.white;

        if (shapeEditMode)
        {
            GUI.backgroundColor =
                new Color(1f, 0.7f, 0.3f);
        }

        if (GUILayout.Button(
            "SHAPE MODE",
            GUILayout.Height(35)))
        {
            shapeEditMode = true;
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        if (shapeEditMode)
        {
            EditorGUILayout.HelpBox(
                "SHAPE MODE: Click cells to enable/disable them.",
                MessageType.Warning
            );
        }
        else
        {
            selectedGem =
                (GemType)EditorGUILayout.EnumPopup(
                    "Selected Gem",
                    selectedGem
                );

            EditorGUILayout.HelpBox(
                "GEM MODE: Click an active cell to change its gem.",
                MessageType.Info
            );
        }
    }

    // =========================================
    // BOARD
    // =========================================

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

        scrollPosition =
            EditorGUILayout.BeginScrollView(
                scrollPosition
            );

        for (
            int row = currentLevelData.rows - 1;
            row >= 0;
            row--)
        {
            EditorGUILayout.BeginHorizontal();

            for (
                int column = 0;
                column < currentLevelData.columns;
                column++)
            {
                DrawCell(
                    row,
                    column
                );
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    // =========================================
    // CELL
    // =========================================

    private void DrawCell(
        int row,
        int column)
    {
        bool active =
            currentLevelData.IsValidCell(
                row,
                column
            );

        GUIStyle style =
            new GUIStyle(
                GUI.skin.button
            );

        style.fontSize = 10;

        style.alignment =
            TextAnchor.MiddleCenter;

        if (!active)
        {
            GUI.backgroundColor =
                new Color(
                    0.15f,
                    0.15f,
                    0.15f
                );
        }
        else
        {
            GUI.backgroundColor =
                Color.white;
        }

        string text;

        if (!active)
        {
            text = "EMPTY";
        }
        else
        {
            text =
                GetGemShortName(
                    currentLevelData.GetGem(
                        row,
                        column
                    )
                );
        }

        if (GUILayout.Button(
            text,
            style,
            GUILayout.Width(65),
            GUILayout.Height(65)))
        {
            if (shapeEditMode)
            {
                currentLevelData.SetCellActive(
                    row,
                    column,
                    !active
                );
            }
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

            EditorUtility.SetDirty(
                currentLevelData
            );
        }

        GUI.backgroundColor = Color.white;
    }

    // =========================================
    // GEM NAME
    // =========================================

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

    // =========================================
    // BUTTONS
    // =========================================

    private void DrawButtons()
    {
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

        EditorGUILayout.Space(10);

        if (GUILayout.Button(
            "RANDOMIZE LEVEL",
            GUILayout.Height(35)))
        {
            RandomizeLevel();
        }
    }

    // =========================================
    // APPLY BOARD SIZE
    // =========================================

    private void ApplyBoardSize()
    {
        if (currentLevelData == null)
            return;

        bool confirm =
            EditorUtility.DisplayDialog(
                "Change Board Size",
                "Changing the board size will reset the current level board. Continue?",
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

        Repaint();
    }

    // =========================================
    // LOAD
    // =========================================

    private void LoadLevel(int level)
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
            currentLevelData.Initialize();

            editRows =
                currentLevelData.rows;

            editColumns =
                currentLevelData.columns;
        }

        Repaint();
    }

    // =========================================
    // CREATE
    // =========================================

    private void CreateLevel(int level)
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

        if (existing != null)
        {
            currentLevelData = existing;

            currentLevelData.Initialize();

            editRows =
                currentLevelData.rows;

            editColumns =
                currentLevelData.columns;

            return;
        }

        LevelData data =
            ScriptableObject.CreateInstance<LevelData>();

        data.levelNumber = level;

        data.rows = 8;
        data.columns = 8;

        data.moves = 30;
        data.redGoal = 20;

        int totalCells =
            data.rows * data.columns;

        data.gems =
            new GemType[totalCells];

        data.activeCells =
            new bool[totalCells];

        for (int i = 0;
             i < totalCells;
             i++)
        {
            data.gems[i] =
                GemType.Red;

            data.activeCells[i] =
                true;
        }

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

        editRows = 8;
        editColumns = 8;

        Debug.Log(
            "Level " +
            level +
            " created!"
        );

        Repaint();
    }

    // =========================================
    // SAVE
    // =========================================

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
    }

    // =========================================
    // CLEAR
    // =========================================

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

    // =========================================
    // RANDOMIZE
    // =========================================

    private void RandomizeLevel()
    {
        if (currentLevelData == null)
            return;

        currentLevelData.Initialize();

        for (int i = 0;
             i < currentLevelData.gems.Length;
             i++)
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

        Repaint();
    }

    // =========================================
    // FOLDER
    // =========================================

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