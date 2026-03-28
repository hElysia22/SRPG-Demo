using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct GridData
{
    public int x;
    public int y;
    public bool canWalk;
    public int defBonus;
}

public class GridManage : MonoBehaviour
{
    public static GridManage Instance;
    public GridData[,] gridArray;
    private void Awake()
    {
        Instance = this;
        //SaveDataToJson();
        LoadMapFromJson();
        InitAllGrid();
    }

    public void InitAllGrid()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefab/Grid");
        int width = gridArray.GetLength(0);
        int height = gridArray.GetLength(1);

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                GridData grid = gridArray[i, j];
                Vector3 pos = new Vector3(grid.x, 0, grid.y);
                Instantiate(prefab, pos, prefab.transform.rotation);
            }
        }
    }

    public void LoadMapFromJson()
    {
        string path = Application.dataPath + "/map.json";
        string json = System.IO.File.ReadAllText(path);

        MapSaveData data = JsonUtility.FromJson<MapSaveData>(json);
        int w = data.width; 
        int h = data.height;
        gridArray = new GridData[w, h];

        foreach(var grid in data.grids)
        {
            //给gridArray里面每一项赋值
            gridArray[grid.x, grid.y] = new GridData
            {
                x = grid.x,
                y = grid.y,
                canWalk = grid.canwalk,
                defBonus = grid.defBonus
            };
        }
        Debug.Log("地图读取完成！");
    }

    //用于生成一个map.json文件
    /*private void SaveDataToJson()
    {
        MapSaveData data = new MapSaveData();
        data.width = 8;
        data.height = 8;
        data.gridSize = 1;
        data.grids = new GridSaveData[8 * 8];

        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                GridSaveData grid = new GridSaveData();
                grid.x = i;
                grid.y = j;
                grid.canwalk = true;
                grid.defBonus = 2;

                data.grids[i + j * 8] = grid;
            }
        }
        string json = JsonUtility.ToJson(data);

        string path = Application.dataPath + "/map.json";
        System.IO.File.WriteAllText(path, json);
        Debug.Log("保存成功");
    }*/

}
