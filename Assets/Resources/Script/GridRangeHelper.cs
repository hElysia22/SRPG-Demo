using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridRangeHelper
{
    private static readonly (int x, int y)[] dirs = { (0, 1), (1, 0), (-1, 0), (0, -1) };

    /// BFS计算可移动范围（只包含能行走的格子，不包含起点）
    public static GridData[] CalculateMoveRange(int startX, int startY, int step)
    {
        List<GridData> result = new List<GridData>();
        HashSet<GridData> visited = new HashSet<GridData>();
        Queue<(int x, int y, int remain)> queue = new Queue<(int, int, int)>();

        GridData startTile = GridManager.Instance.GetTile(startX, startY);
        if (startTile == null) return result.ToArray();

        visited.Add(startTile);
        queue.Enqueue((startX, startY, step));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.remain <= 0) continue;

            foreach (var dir in dirs)
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;
                GridData tile = GridManager.Instance.GetTile(newX, newY);

                if (tile == null || visited.Contains(tile) || !tile.canWalk)
                    continue;

                visited.Add(tile);
                result.Add(tile);
                queue.Enqueue((newX, newY, current.remain - 1));
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// 计算攻击范围（曼哈顿距离，不考虑地形阻挡）
    /// </summary>
    public static GridData[] CalculateAttackRange(int centerX, int centerY, int range)
    {
        List<GridData> result = new List<GridData>();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > range) continue;
                GridData tile = GridManager.Instance.GetTile(centerX + x, centerY + y);
                if (tile != null) result.Add(tile);
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// 判断两个坐标是否在指定曼哈顿距离内
    /// </summary>
    public static bool IsInManhattanRange(int x1, int y1, int x2, int y2, int range)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) <= range;
    }
}
