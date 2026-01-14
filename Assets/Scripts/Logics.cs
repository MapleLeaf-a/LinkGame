using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Logics : MonoBehaviour
{
    const int HEIGHT = Global.HEIGHT;
    const int WIDTH = Global.WIDTH;
    const int SIZE = Global.SIZE;
    const float BIAS = Global.BIAS;
    private int[,] array;
    
    //Link是否存在
    private bool[,] used = new bool[HEIGHT / SIZE + 2, WIDTH / SIZE + 2];
    bool[,]u = new bool[HEIGHT / SIZE + 2, WIDTH / SIZE + 2];
    //标记路径
    List<Vector2Int> path = new List<Vector2Int>();
    List<List<Vector2Int>> allPaths = new List<List<Vector2Int>>();
    //绘制路径
    public LineRenderer lineRenderer;

    List<Vector2Int> ClickedItems = new List<Vector2Int> ();

    //点击LinkObject的缩放
    const float originalScale = 0.25f;
    const float targetScale = 0.35f;

    // Start is called before the first frame update
    void Start()
    {
        array = Global.array;
        for (int i = 0; i < HEIGHT / SIZE + 2; i++)
        {
            for (int j = 0; j < WIDTH / SIZE + 2; j++)
            {
                if (i == 0 || j == 0 || i == HEIGHT / SIZE + 1 || j == WIDTH / SIZE + 1)
                {
                    used[i, j] = false;
                }
                else
                { 
                    used[i, j] = true;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseDownPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int tuple= MapCreator.map(mouseDownPosition);
            int i = tuple.x, j = tuple.y;
            Debug.Log("i = "+ i + " " + "j = " + j);
            if (0 <= i && i <= HEIGHT / SIZE && 0 <= j && j <= WIDTH / SIZE && used[i + 1, j + 1] && !isIJInItems(new Vector2Int(i, j)))
            {
                ClickedItems.Add(new Vector2Int(i, j));
                ClickedEffect(MapCreator.map(i, j), originalScale, targetScale);
                if (ClickedItems.Count == 2)
                {
                    for (int a = 0; a < HEIGHT / SIZE + 2; a++)
                    {
                        for (int b = 0; b < WIDTH / SIZE + 2; b++)
                        {
                            u[a, b] = used[a, b];
                        }
                    }
                    u[ClickedItems[0].x + 1, ClickedItems[0].y + 1] = false;
                    u[ClickedItems[1].x + 1, ClickedItems[1].y + 1] = false;
                    if (array[ClickedItems[0].x, ClickedItems[0].y] == array[ClickedItems[1].x, ClickedItems[1].y])
                    {
                        if (DFS(ClickedItems[0], ClickedItems[1]))
                        {
                            Debug.Log("消除！");

                            //DFS探索下的绘制路径
                            path.Add(ClickedItems[1]); //加上终点
                            DrawPath(GetLinkObject(MapCreator.map(ClickedItems[0].x, ClickedItems[0].y)).GetComponent<SpriteRenderer>().color);
                            for (int a = 0; a < path.Count; a++)
                            {
                                Debug.Log("path:" + path[a]);
                            }
                            path.Clear();

                            Destroy(GetLinkObject(MapCreator.map(ClickedItems[0].x, ClickedItems[0].y)));
                            Destroy(GetLinkObject(MapCreator.map(ClickedItems[1].x, ClickedItems[1].y)));
                            used[ClickedItems[0].x + 1, ClickedItems[0].y + 1] = false;
                            used[ClickedItems[1].x + 1, ClickedItems[1].y + 1] = false;
                        }
                        else
                        {
                            Debug.Log("补兑！");
                        }
                    }
                    
                    foreach (Vector2Int a in ClickedItems)
                    {
                        RestoreClickedEffect(MapCreator.map(a.x, a.y), originalScale, targetScale);
                    }
                    ClickedItems.Clear();
                }
            }
            else
            {
                foreach (Vector2Int a in ClickedItems)
                {
                    RestoreClickedEffect(MapCreator.map(a.x, a.y), originalScale, targetScale);
                }
                ClickedItems.Clear();
                if (0 <= i && i <= HEIGHT / SIZE && 0 <= j && j <= WIDTH / SIZE)
                {
                    Debug.Log("out of range or i,j in items or used[i + 1, j + 1] = " + used[i + 1, j + 1]);
                }
                else
                {
                    Debug.Log("out of range or i,j in items");
                }
            }
        }
    }

    //深度优先判断是否连通
    bool DFS(Vector2Int start, Vector2Int end)
    {
        if (start.x == end.x && start.y == end.y)
        { 
            return true;
        }
        if (start.x < -1 || start.x > HEIGHT / SIZE || start.y < -1 || start.y > WIDTH / SIZE
            || u[start.x + 1, start.y + 1])
        {
            return false;
        }
        
        path.Add(start);

        u[start.x + 1, start.y + 1] = true;
        bool found = DFS(new Vector2Int(start.x + 1, start.y), end) ||
                        DFS(new Vector2Int(start.x, start.y + 1), end) ||
                        DFS(new Vector2Int(start.x - 1, start.y), end) ||
                        DFS(new Vector2Int(start.x, start.y - 1), end);
        if (!found) path.RemoveAt(path.Count - 1);
        return found;

    }

    GameObject GetLinkObject(Vector3 pos)
    {
        Collider[] colliders = Physics.OverlapSphere(pos, SIZE / 2 - 0.2f);
        foreach (Collider c in colliders)
        {
            if (c.tag == "LinkObject")
            {
                //Debug.Log(c.name);
                return c.gameObject;
            }
        }
        //Debug.Log("Not Found!");
        return null;
    }

    bool isIJInItems(Vector2Int t)
    {
        for (int i = 0; i < ClickedItems.Count; i++)
        {
            if (t == ClickedItems[i])
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator ScaleEffect(Vector3 pos, float originalScale, float targetScale)
    { 
        GameObject go = GetLinkObject(pos);
        float duration = 0.25f;          // 持续时间
        float timer = 0f;             // 计时器

        while (timer < duration)
        {
            timer += Time.deltaTime;  // 计时
            float progress = timer / duration;  // 计算进度（0到1）

            if (go == null) break;
            // 线性插值
            go.transform.localScale = Vector3.one * Mathf.Lerp(originalScale, targetScale, progress);

            yield return null;  // 等一帧
        }
    }
    
    void ClickedEffect(Vector3 pos, float originalScale, float targetScale)
    {
        StartCoroutine(ScaleEffect(pos, originalScale, targetScale));
    }
    
    void RestoreClickedEffect(Vector3 pos, float originalScale, float targetScale)
    {
        StartCoroutine(ScaleEffect(pos, targetScale, originalScale));
    }

    void DrawPath(Color color)
    {
        
        for (int i = 0; i < path.Count - 1; i++)
        {
            GameObject lineObject = new GameObject();
            LineRenderer newLR = lineObject.AddComponent<LineRenderer>();
            newLR.material = new Material(Shader.Find("Sprites/Default"));
            newLR.startColor = color;
            newLR.endColor = color;
            newLR.startWidth = 0.1f;
            newLR.endWidth = 0.1f;
            newLR.positionCount = 2;
            newLR.SetPosition(0 ,MapCreator.map(path[i].x, path[i].y));
            newLR.SetPosition(1, MapCreator.map(path[i + 1].x, path[i + 1].y));
            Destroy(lineObject, 1f);
        }
    }
    
}
