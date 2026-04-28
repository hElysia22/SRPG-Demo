using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    // 地图数据（二维数组，运行时使用）
    public GridData[,] grids;
    private GameObject[,] gridObjects;

    [Header("地图设置")]
    public int gridSize = 16;       // 固定16x16
    public Color normalColor = Color.white;    // 普通颜色
    public Color highlightColor = Color.green; // 可移动高亮
    public Color obstacleColor = Color.gray;   // 障碍物颜色

    private void Awake()
    {
        Instance = this;
        //SaveDataToJson();
        LoadMapFromJson();    // 加载JSON地图数据
        GenerateGridVisual(); // 生成格子对象
    }

    // 生成格子视觉表现
    void GenerateGridVisual()
    {
        GameObject tilePrefab = Resources.Load<GameObject>("Prefab/Grid");
        GameObject obstraclePrefab = Resources.Load<GameObject>("Prefab/Obstracle");
        int width = grids.GetLength(0);
        int height = grids.GetLength(1);

        gridObjects = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridData data = grids[x, y];
                Vector3 pos = new Vector3(data.x, -1, data.y);

                if (grids[x, y].type == 0) 
                {
                    GameObject tile = Instantiate(tilePrefab, pos, tilePrefab.transform.rotation, transform);
                    tile.name = $"Tile_{x}_{y}";
                    gridObjects[x, y] = tile;

                    Renderer rend = tile.GetComponent<Renderer>();
                    data.tileRenderer = rend;
                    rend.material.color = normalColor;
                }
                else if (grids[x, y].type == 1)
                {
                    GameObject obstracle = Instantiate(obstraclePrefab, pos, obstraclePrefab.transform.rotation, transform);
                    obstracle.name = $"obstracle_{x}_{y}";
                    gridObjects[x, y] = obstracle;
                }
            }
        }
    }

    // 获取指定坐标的格子
    public GridData GetTile(int x, int y)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
            return grids[x, y];
        return null;
    }
    //将当前格子设为不可移动（防止角色之间碰撞和重叠）
    public void SetMoveFalse(int x, int y)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
            grids[x,y].canWalk = false;
    }
    //重置当前位置格子为可移动
    public void ResetMove(int x, int y)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
            grids[x, y].canWalk = true;
    }

    // 高亮可移动的格子
    public void HighlightMoveableTiles(GridData[] moveableTiles)
    {
        ResetAllTileColor();
        foreach (var tile in moveableTiles)
        {
            if (tile != null)
                tile.tileRenderer.material.color = highlightColor;
        }
    }

    // 重置所有格子颜色
    public void ResetAllTileColor()
    {
        foreach (var tile in grids)
        {
            if (tile.tileRenderer != null)
                tile.tileRenderer.material.color = tile.canWalk ? normalColor : obstacleColor;
        }
    }

    public bool isHighLight(GridData tile)
    {
        return tile.tileRenderer.material.color == highlightColor;
    }

    public void LoadMapFromJson()
    {
        string path = Application.dataPath + "/map.json";

        if (!File.Exists(path))
        {
            Debug.LogError("未找到 map.json，自动生成 16x16 默认地图");
            SaveDataToJson();
        }

        string json = File.ReadAllText(path);
        MapSaveData data = JsonUtility.FromJson<MapSaveData>(json);

        grids = new GridData[data.width, data.height];

        foreach (var gridData in data.grids)
        {
            grids[gridData.x, gridData.y] = gridData;
        }

        Debug.Log($"地图加载完成：{data.width}x{data.height}");
    }

    [ContextMenu("保存地图为JSON")]
    public void SaveDataToJson()
    {
        MapSaveData data = new MapSaveData();
        data.width = 16;
        data.height = 16;
        data.gridSize = 1;
        data.grids = new GridData[16 * 16];

        int index = 0;
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                data.grids[index++] = new GridData(x, y, true, null, 0);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        string path = Application.dataPath + "/map.json";
        File.WriteAllText(path, json);
        Debug.Log("16x16 地图已保存：" + path);
    }
}