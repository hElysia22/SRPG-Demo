using System;
using System.Collections.Generic;
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
    //后面要移到GameManage里面
    bool isMoving = false;

    private void Start()
    {
        map = GridManage.Instance.gridArray;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)&& !isMoving)
            {
                Vector3 start = new Vector3(Mathf.Round(transform.position.x), 0, Mathf.Round(transform.position.z));
                Vector3 end = new Vector3(Mathf.Round(hit.transform.position.x), 0, Mathf.Round(hit.transform.position.z));
                AStar(start, end);
                    foreach (var p in pathList)
                    {
                        Debug.Log("路径点：" + p);
                    }
                    isMoving = true;
                    StartCoroutine(MoveByGrid());
            }
        }
    }

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
            while(Mathf.RoundToInt(transform.position.x) != Mathf.RoundToInt(targetPos.x)
            || Mathf.RoundToInt(transform.position.z) != Mathf.RoundToInt(targetPos.z))
            {
                Vector3 dir = targetPos - transform.position;
                dir.y = 0;

                if(dir.magnitude > 0.1f)
                {
                    Quaternion quaternion = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, quaternion, 10f * Time.deltaTime);
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                    );
                 yield return null;  
            } 
        }
        isMoving = false;
    }

    public float CalculateDis(Vector3 start, Vector3 end)
    {
        return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.z - end.z);
    }
}
