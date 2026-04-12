using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public struct Path : IComparable<Path>
{
    public Vector3 pos;
    public float cost;
    public Vector3 parentPos;


    public Path(Vector3 pos, float cost, Vector3 parentPos)
    {
        this.pos = pos;
        this.cost = cost;
        this.parentPos = parentPos;
    }

    public int CompareTo(Path other)
    {
        return this.cost.CompareTo(other.cost);
    }
}


public class Move : MonoBehaviour
{
    public GridData[,] map;
    public List<Path> costList = new List<Path>();
    public List<Path> visited = new List<Path>();
    public List<Vector3> pathList = new List<Vector3>();
    public bool isFind = false;
    public float moveSpeed = 5f;
    bool isMoving = false;
    public bool canPlay = false;
    //能移动到X格以内的位置
    /*public int canMove = 3;
    //记录能移动到的格子
    public List<Vector3> canMovePos = new List<Vector3>();
    private bool[,] BFSvisited;
    public bool isEnd = false;*/

    private void Start()
    {
        map = GridManage.Instance.gridArray;
        //BFSvisited =  new bool[map.GetLength(0), map.GetLength(1)];
    }
    private void Update()
    {
        /*if(canPlay&&!isEnd)
        {
            BFS();
        }*/
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)&& !isMoving && canPlay)
            {
                /*if (!map[(int)(hit.transform.position.x), (int)(hit.transform.position.z)].isMoveAble)
                {
                    return;
                }*/
                Vector3 start = new Vector3(Mathf.Round(transform.position.x), 1, Mathf.Round(transform.position.z));
                Vector3 end = new Vector3(Mathf.Round(hit.transform.position.x), 1, Mathf.Round(hit.transform.position.z));
                AStar(start, end);
                isMoving = true;
                StartCoroutine(MoveByGrid());
            }
        }
    }
    //BFS搜索可移动的格子
    /*public void BFS()
    {
        canMovePos.Clear();
        System.Array.Clear(BFSvisited, 0, BFSvisited.Length);

        int startX = Mathf.FloorToInt(transform.position.x);
        int startY = Mathf.FloorToInt(transform.position.z);
        Vector3 startPos = new Vector3(startX, 1, startY);

        Queue<Vector3> queue = new Queue<Vector3>();
        queue.Enqueue(startPos);
        if (startX >= 0 && startX < map.GetLength(0) &&
               startY >= 0 && startY < map.GetLength(1) &&
               map[startX, startY].canWalk)
        {
            BFSvisited[startX, startY] = true;
        }
        (int x, int y)[] dirs = new (int, int)[] { (0, 1), (1, 0), (-1, 0), (0, -1) };

        while (queue.Count > 0)
        {
            Vector3 currentPos = queue.Dequeue();
            int x = (int)(currentPos.x);
            int y = (int)(currentPos.z);
            if (x >= 0 && x < map.GetLength(0) &&
               y >= 0 && y < map.GetLength(1) &&
               map[x, y].canWalk && !BFSvisited[x,y])
            {
                BFSvisited[x, y] = true;
            }

            int distance = (int)CalculateDis(startPos, currentPos);
            if (distance > canMove)
            {
                continue;
            }
            canMovePos.Add(currentPos);
            if (x >= 0 && x < map.GetLength(0) &&
               y >= 0 && y < map.GetLength(1) && 
               map[x,y].canWalk)
            {
                map[x, y].isMoveAble = true;
            }    

            foreach (var dir in dirs)
            {
                int newX = x + dir.x;
                int newY = y + dir.y;

                // 判断：地图内 + 可行走 + 未访问
                if (newX >= 0 && newX < map.GetLength(0) &&
               newY >= 0 && newY < map.GetLength(1) &&
               map[newX, newY].canWalk && !BFSvisited[newX, newY])
                {
                    queue.Enqueue(new Vector3(newX, 1, newY));
                }
            }
        }
        Debug.Log("可移动格子数量：" + canMovePos.Count);
        isEnd = true;
    }*/

    /*private void ResetAllMoveable()
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                map[x, y].isMoveAble = false;
            }
        }
        isEnd = false;
    }*/

    //A*寻路
    public void AStar(Vector3 start, Vector3 end)
    {
        // 初始化清空
        costList.Clear();
        visited.Clear();
        pathList.Clear();
        isFind = false;
        FindPath(start, end, Vector3.zero);

        pathList.Reverse();
    }

    public void FindPath(Vector3 start, Vector3 end, Vector3 parentPos)
    {
        if (!visited.Exists(p => p.pos == start))
        {
            Path currentPath = new Path(start, CalculateDis(start, end), parentPos);
            visited.Add(currentPath);
        }
        if (isFind)
        {
            return;
        }
        if (start == end)
        {
            isFind = true;
            RetracePath(start);
            return;
        } 

        int startX = (int)start.x;
        int startY = (int)start.z;
        (int x, int y)[] dirs = new (int, int)[]
        {
            (0, 1),
            (1, 0),
            (-1, 0),
            (0, -1)
        };

        foreach (var dir in dirs)
        {
            int targetX = startX + dir.x;
            int targetY = startY + dir.y;
            if (targetX >= 0 && targetX < map.GetLength(0) &&
               targetY >= 0 && targetY < map.GetLength(1) &&
               map[targetX, targetY].canWalk)
            {
                    Vector3 pos = new Vector3(targetX, start.y, targetY);
                    Path _path = new Path(pos, CalculateDis(pos, end), start);
                    if (!visited.Exists(p => p.pos == _path.pos))
                    {
                        costList.Add(_path);
                    } 
            }
        }
        costList.Sort();
        if (costList.Count > 0)
        {
            Path nextPos = costList[0];
            costList.RemoveAt(0);
            FindPath(nextPos.pos, end, nextPos.parentPos);
        }
        else
        {
            Debug.Log("未找到");
            return;
        }
    }

    public void RetracePath(Vector3 pos)
    {
        Vector3 current = pos;
        while(current != Vector3.zero)
        {
            pathList.Add(current);
            Path currentParent = visited.Find(p => p.pos == current);

            if (currentParent.pos == Vector3.zero)
            {
                Debug.Log("寻路出错");
                break;
            }

            current = currentParent.parentPos;
        }
    }

    public System.Collections.IEnumerator MoveByGrid()
    {
        foreach (var targetPos in pathList)
        {
            Vector3 Dir = targetPos - transform.position;
            if (Dir.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(Dir);
                while (Quaternion.Angle(transform.rotation,targetRot) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
                }
            }

            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                    );
                 yield return null;  
            } 
        }
        isMoving = false;
        GameManage.Instance.EndTurn();
        //ResetAllMoveable();
    }

    public float CalculateDis(Vector3 start, Vector3 end)
    {
        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.z - end.z);
    }
}
