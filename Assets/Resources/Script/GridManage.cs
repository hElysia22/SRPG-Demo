using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public GridData[,] grids;
    private GameObject[,] gridObjects;

    [Header("地图设置")]
    public int gridSize = 16;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;
    public Color obstacleColor = Color.gray;

    private void Awake()
    {
        Instance = this;
        LoadMapFromJson();
        GenerateGridVisual();
    }

    void GenerateGridVisual()
    {
        GameObject tilePrefab = Resources.Load<GameObject>("Prefab/Grid");
        GameObject obstaclePrefab = Resources.Load<GameObject>("Prefab/Obstracle");
        GameObject defendPrefab = Resources.Load<GameObject>("Prefab/DefendGrid");

        int width = grids.GetLength(0);
        int height = grids.GetLength(1);
        gridObjects = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridData data = grids[x, y];
                Vector3 pos = new Vector3(data.x, -1, data.y);

                if (data.type == 0)
                {
                    GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                    tile.name = $"Tile_{x}_{y}";
                    gridObjects[x, y] = tile;
                    Renderer rend = tile.GetComponent<Renderer>();
                    data.tileRenderer = rend;
                    rend.material.color = normalColor;
                }
                else if (data.type == 1)
                {
                    GameObject obstacle = Instantiate(obstaclePrefab, pos, Quaternion.identity, transform);
                    obstacle.name = $"Obstacle_{x}_{y}";
                    gridObjects[x, y] = obstacle;
                }
                else if (data.type == 2)
                {
                    GameObject defend = Instantiate(defendPrefab, pos, Quaternion.identity, transform);
                    defend.name = $"Defend_{x}_{y}";
                    gridObjects[x, y] = defend;
                    Renderer rend = defend.GetComponent<Renderer>();
                    data.tileRenderer = rend;
                    rend.material.color = normalColor;
                }
            }
        }
    }

    public GridData GetTile(int x, int y)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
            return grids[x, y];
        return null;
    }

    public void SetMoveFalse(int x, int y)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
            grids[x, y].canWalk = false;
    }

    public void ResetMove(int x, int y)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
            grids[x, y].canWalk = true;
    }

    // 高亮可移动/可攻击格子
    public void HighlightMoveableTiles(GridData[] moveableTiles)
    {
        foreach (var tile in moveableTiles)
        {
            if (tile != null && tile.tileRenderer != null)
            {
                tile.tileRenderer.material.color = highlightColor;
                tile.isHighlighted = true; // 同步标记状态
            }
        }
    }

    // 重置所有格子颜色
    public void ResetAllTileColor()
    {
        foreach (var tile in grids)
        {
            if (tile.tileRenderer != null)
            {
                tile.tileRenderer.material.color = tile.canWalk ? normalColor : obstacleColor;
            }
            tile.isHighlighted = false;
        }
    }

    // 判断格子是否高亮
    public bool IsHighlight(GridData tile)
    {
        return tile != null && tile.isHighlighted;
    }

    public void LoadMapFromJson()
    {
        string path = Application.dataPath + "/map.json";
        if (!File.Exists(path))
        {
            Debug.LogError("未找到map.json，自动生成默认地图");
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
        MapSaveData data = new MapSaveData
        {
            width = 16,
            height = 16,
            gridSize = 1,
            grids = new GridData[16 * 16]
        };

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
        Debug.Log("地图已保存：" + path);
    }
}