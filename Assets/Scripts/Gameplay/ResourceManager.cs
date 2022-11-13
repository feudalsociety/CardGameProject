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

            // TODO : Pool���� original(����)�� �̹� ��� ������ ���
        }

        T obj = Resources.Load<T>(path);
        if(!obj)
        {
            Debug.LogError("Load error! path = " + path);
            return null;
        }

        return obj;
    }


    // TODO : �̸��� �������ִ� argument�� �����Ѵ�.
    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Falied to Load prefab : {path}");
            return null;
        }

        // TODO : Ȥ�� Pooling���� �����ǰ� �ִٸ� �űⰡ�� �´�.

        // Object�� �ٿ��� ������ ����� ȣ���� ��������
        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null) return;

        // TODO : ���࿡ Ǯ���� �ʿ��� ���̶�� -> pooling���� ����

        Object.Destroy(go);
    }
}
