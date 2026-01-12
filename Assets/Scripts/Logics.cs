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
    
    //Link «∑Ò¥Ê‘⁄
    private bool[,] used = new bool[HEIGHT / SIZE + 2, WIDTH / SIZE + 2];
    bool[,]u = new bool[HEIGHT / SIZE + 2, WIDTH / SIZE + 2];


    List<(int, int)> ClickedItems = new List<(int, int)> ();

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
                        Debug.Log("œ˚≥˝£°");
                        Destroy(GetLinkObject(MapCreator.map(ClickedItems[0].Item1, ClickedItems[0].Item2)));
                        Destroy(GetLinkObject(MapCreator.map(ClickedItems[1].Item1, ClickedItems[1].Item2)));
                        used[ClickedItems[0].Item1 + 1, ClickedItems[0].Item2 + 1] = false;
                        used[ClickedItems[1].Item1 + 1, ClickedItems[1].Item2 + 1] = false;
                    }
                    else
                    {
                        Debug.Log("≤π∂“£°");
                    }
                    ClickedItems.Clear();
                }
            }
            else
            {
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

    bool DFS((int, int) start, (int, int) end)
    {
        //buggy!
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
        Collider[] colliders = Physics.OverlapSphere(pos, SIZE / 2 - 0.1f);
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
}
