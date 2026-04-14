using System.Collections;
using System.Collections.Generic;
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
    public int defBonus;
    [HideInInspector] public Renderer tileRenderer;
    public GridData(int x, int y, bool isWalkable, Renderer renderer)
    {
        this.x = x;
        this.y = y;
        this.canWalk = isWalkable;
        this.tileRenderer = renderer;
    }
}
