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


    List<(int, int)> ClickedItems = new List<(int, int)> ();

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
            (int, int) tuple= MapCreator.map(mouseDownPosition);
            int i = tuple.Item1, j = tuple.Item2;
            Debug.Log("i = "+ i + " " + "j = " + j);
            if (0 <= i && i <= HEIGHT / SIZE && 0 <= j && j <= WIDTH / SIZE && used[i + 1, j + 1] && !isIJInItems((i, j)))
            {
                ClickedItems.Add((i, j));
                StartCoroutine(ClickedEffect(MapCreator.map(i, j), targetScale));
                if (ClickedItems.Count == 2)
                {
                    for (int a = 0; a < HEIGHT / SIZE + 2; a++)
                    {
                        for (int b = 0; b < WIDTH / SIZE + 2; b++)
                        {
                            u[a, b] = used[a, b];
                        }
                    }
                    u[ClickedItems[0].Item1 + 1, ClickedItems[0].Item2 + 1] = false;
                    u[ClickedItems[1].Item1 + 1, ClickedItems[1].Item2 + 1] = false;
                    if (array[ClickedItems[0].Item1, ClickedItems[0].Item2] == array[ClickedItems[1].Item1, ClickedItems[1].Item2]
                        && DFS(ClickedItems[0], ClickedItems[1]))
                    {
                        Debug.Log("消除！");
                        Destroy(GetLinkObject(MapCreator.map(ClickedItems[0].Item1, ClickedItems[0].Item2)));
                        Destroy(GetLinkObject(MapCreator.map(ClickedItems[1].Item1, ClickedItems[1].Item2)));
                        used[ClickedItems[0].Item1 + 1, ClickedItems[0].Item2 + 1] = false;
                        used[ClickedItems[1].Item1 + 1, ClickedItems[1].Item2 + 1] = false;
                    }
                    else
                    {
                        Debug.Log("补兑！");
                    }
                    foreach ((int, int) a in ClickedItems)
                    {
                        StartCoroutine(RestoreClickedEffect(MapCreator.map(a.Item1, a.Item2)));
                    }
                    ClickedItems.Clear();
                }
            }
            else
            {
                foreach ((int, int) a in ClickedItems)
                {
                    StartCoroutine(RestoreClickedEffect(MapCreator.map(a.Item1, a.Item2)));
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
    bool DFS((int, int) start, (int, int) end)
    {
        if (start.Item1 == end.Item1 && start.Item2 == end.Item2)
        { 
            return true;
        }
        if (start.Item1 < -1 || start.Item1 > HEIGHT / SIZE || start.Item2 < -1 || start.Item2 > WIDTH / SIZE
            || u[start.Item1 + 1, start.Item2 + 1])
        {
            return false;
        }
        else
        {
            u[start.Item1 + 1, start.Item2 + 1] = true;
            bool found = DFS((start.Item1 + 1, start.Item2), end) ||
                         DFS((start.Item1, start.Item2 + 1), end) ||
                         DFS((start.Item1 - 1, start.Item2), end) ||
                         DFS((start.Item1, start.Item2 - 1), end);
            return found;
        }
    }

    GameObject GetLinkObject(Vector3 pos)
    {
        Collider[] colliders = Physics.OverlapSphere(pos, SIZE / 2 - 0.2f);
        foreach (Collider c in colliders)
        {
            if (c.tag == "LinkObject")
            {
                Debug.Log(c.name);
                return c.gameObject;
            }
        }
        Debug.Log("Not Found!");
        return null;
    }

    bool isIJInItems((int, int) t)
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

    IEnumerator ClickedEffect(Vector3 pos, float targetScale)
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

    IEnumerator RestoreClickedEffect(Vector3 pos)
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
            go.transform.localScale = Vector3.one * Mathf.Lerp(targetScale, originalScale, progress);

            yield return null;  // 等一帧
        }
    }
}
