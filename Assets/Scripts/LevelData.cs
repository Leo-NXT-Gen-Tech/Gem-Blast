using System;

[Serializable]
public class LevelData
{
    public int levelNumber;
    public int moves;
    public int redGoal;

    public LevelData(int levelNumber, int moves, int redGoal)
    {
        this.levelNumber = levelNumber;
        this.moves = moves;
        this.redGoal = redGoal;
    }
}