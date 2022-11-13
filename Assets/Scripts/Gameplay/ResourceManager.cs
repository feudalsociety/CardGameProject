using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            // TODO : Pool에서 original(원본)도 이미 들고 있으면 사용
        }

        T obj = Resources.Load<T>(path);
        if(!obj)
        {
            Debug.LogError("Load error! path = " + path);
            return null;
        }

        return obj;
    }


    // TODO : 이름을 지정해주는 argument를 생성한다.
    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Falied to Load prefab : {path}");
            return null;
        }

        // TODO : 혹시 Pooling으로 관리되고 있다면 거기가져 온다.

        // Object를 붙여준 이유는 재귀적 호출을 막기위해
        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null) return;

        // TODO : 만약에 풀링이 필요한 아이라면 -> pooling에서 제거

        Object.Destroy(go);
    }
}
