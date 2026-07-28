using UnityEngine;

[System.Serializable]
public class MapSaveData
{
    public int width;
    public int height;
    public float gridSize;
    public GridData[] grids;
}

[System.Serializable]
public class GridData
{
    public int x;
    public int y;
    public bool canWalk;
    public int type;
    public int defBonus;

    [HideInInspector] public Renderer tileRenderer;
    [HideInInspector] public bool isHighlighted; // 新增：高亮状态标记

    public GridData(int x, int y, bool isWalkable, Renderer renderer, int type)
    {
        this.x = x;
        this.y = y;
        this.canWalk = isWalkable;
        this.tileRenderer = renderer;
        this.type = type;
        this.defBonus = 0;
        this.isHighlighted = false;
    }

    public override bool Equals(object obj)
    {
        return obj is GridData grid && x == grid.x && y == grid.y;
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(x, y);
    }
}
