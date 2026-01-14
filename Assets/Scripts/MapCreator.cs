using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCreator : MonoBehaviour
{
    const int HEIGHT = Global.HEIGHT;
    const int WIDTH = Global.WIDTH;
    const int SIZE = Global.SIZE;
    const float BIAS = Global.BIAS;

    public List<GameObject> gameObjectsList = new List<GameObject>();

    public int[,] array;

    void Awake()
    {
        array = Creator();
        while (!Judge())
        {
            array = Creator();
        }
        Global.array = array;
        for (int i = 0; i < HEIGHT / SIZE; i++)
        {
            for (int j = 0; j < WIDTH / SIZE; j++)
            {
                Instantiate(gameObjectsList[array[i, j]], map(i, j), Quaternion.Euler(90, 0, 0));
            }
        }
    }

    //随机数创建二维数组
    int[,] Creator()
    {
        int[,] array = new int[HEIGHT / SIZE, WIDTH / SIZE];
        for (int i = 0; i < HEIGHT / SIZE; i++)
        {
            for (int j = 0; j < WIDTH / SIZE; j++)
            {
                array[i, j] = Random.Range(0, gameObjectsList.Count);
            }
        }

        return array;
    }

    bool Judge()
    {
        List <int> counts = new List <int>(gameObjectsList.Count);
        for (int i = 0; i < gameObjectsList.Count; i++) counts.Add(0);
        for (int i = 0; i < HEIGHT / SIZE; i++)
        {
            for (int j = 0; j < WIDTH / SIZE; j++)
            {
                 counts[array[i, j]]++;
            }
        }
        for (int i = 0; i < gameObjectsList.Count; i++)
        {
            if (counts[i] % 2 != 0) return false;
        }
        return true;
    }

    //建立数组到plane的映射
    public static Vector3 map(float x, float y)
    { 
        return new Vector3(x * SIZE - BIAS, 0.2f, y * SIZE - BIAS);
    }
    
    //建立plane到数组的映射
    public static Vector2Int map(Vector3 v3)
    {
        //正数负数均向下取整
        double x = (v3.x + 0.5 * SIZE + BIAS) / SIZE;
        double y = (v3.z + 0.5 * SIZE + BIAS) / SIZE;
        if (x < 0)
        {
            x -= SIZE;
        }
        if (y < 0)
        {
            y -= SIZE;
        }
        return new Vector2Int((int)x, (int)y);
    }
}
