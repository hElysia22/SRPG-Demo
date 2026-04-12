using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class MapSaveData
{
    public int width;
    public int height;
    public float gridSize;

    public GridSaveData[] grids;
}
[System.Serializable]
public class GridSaveData
{
    public int x;
    public int y;
    public bool canwalk;
    public int defBonus;
    public bool isMoveAble;
}
