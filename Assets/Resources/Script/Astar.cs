using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    // 四向移动方向（上下左右）
    private static readonly (int x, int y)[] dirs = { (0, 1), (1, 0), (-1, 0), (0, -1) };

    public static List<Vector3> FindPath(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        
        List<Vector3> path = new List<Vector3>();
        GridData[,] map = GridManager.Instance.grids;
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);

        // 世界坐标转网格坐标
        int startX = Mathf.RoundToInt(startWorldPos.x);
        int startY = Mathf.RoundToInt(startWorldPos.z);
        int endX = Mathf.RoundToInt(endWorldPos.x);
        int endY = Mathf.RoundToInt(endWorldPos.z);

        // 边界/障碍物判断
        if (!IsValid(startX, startY) || !IsValid(endX, endY))
            return path;

        List<PathNode> openList = new List<PathNode>();
        HashSet<PathNode> closedList = new HashSet<PathNode>();
        Dictionary<PathNode, PathNode> parentMap = new Dictionary<PathNode, PathNode>();

        PathNode startNode = new PathNode(startX, startY, 0, ManhattanDistance(startX, startY, endX, endY));
        PathNode endNode = new PathNode(endX, endY, 0, 0);

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // 排序获取代价最低的节点
            openList.Sort((a, b) => a.fCost.CompareTo(b.fCost));
            PathNode currentNode = openList[0];
            openList.RemoveAt(0);
            closedList.Add(currentNode);

            // 到达终点
            if (currentNode.x == endX && currentNode.y == endY)
            {
                path = RetracePath(currentNode, parentMap);
                return path;
            }

            // 遍历四向邻居
            foreach (var dir in dirs)
            {
                int newX = currentNode.x + dir.x;
                int newY = currentNode.y + dir.y;
                PathNode neighborNode = new PathNode(newX, newY, 0, 0);

                // 过滤：越界、障碍物、已访问
                if (!IsValid(newX, newY) || closedList.Contains(neighborNode))
                    continue;

                // 计算代价
                float newGCost = currentNode.gCost + 1;
                neighborNode.gCost = newGCost;
                neighborNode.hCost = ManhattanDistance(newX, newY, endX, endY);

                // 更新节点
                if (!openList.Contains(neighborNode))
                {
                    parentMap[neighborNode] = currentNode;
                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }
        // 无路径
        return path;
    }

    // 回溯路径
    private static List<Vector3> RetracePath(PathNode endNode, Dictionary<PathNode, PathNode> parentMap)
    {
        List<Vector3> path = new List<Vector3>();
        PathNode current = endNode;

        while (parentMap.ContainsKey(current))
        {
            path.Add(new Vector3(current.x, 0, current.y));
            current = parentMap[current];
        }

        path.Reverse();
        return path;
    }

    // 曼哈顿距离
    private static float ManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }

    // 检测格子是否合法
    private static bool IsValid(int x, int y)
    {
        GridData[,] map = GridManager.Instance.grids;
        return x >= 0 && y >= 0 && x < map.GetLength(0) && y < map.GetLength(1) && map[x, y].canWalk;
    }
}

public struct PathNode
{
    public int x;
    public int y;
    public float gCost;
    public float hCost;
    public float fCost => gCost + hCost;

    public PathNode(int x, int y, float gCost, float hCost)
    {
        this.x = x;
        this.y = y;
        this.gCost = gCost;
        this.hCost = hCost;
    }

    public override bool Equals(object obj)
    {
        return obj is PathNode node && x == node.x && y == node.y;
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(x, y);
    }
}