using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    // gameobject 받는 버전
    public static GameObject findChild(GameObject go, string name = null, bool recursive = false)
    {
        // 모든 Gameobject는 transform component를 가지고 있으므로
        Transform transform = findChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }


    // go : 최상위 부모 object, name = 이름 이름을 입력하지 않으면 type에만 해당하면 return
    // T는 찾고 싶은 component 
    public static T findChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null) return null;

        if (recursive == false)
        {
            // 바로 밑에 있는 child 하나씩 scan
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    // T compnent가 없으면 최종 line에가서 null을 return한다.
                    T component = transform.GetComponent<T>();
                    if (component != null) return component;
                }
            }
        }
        else
        {
            // Returns all components of Type type in the GameObject or any of its children using depth first search.
            // Works recursively.
            // recursive하게 하위 child 모두 scan
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                // null이면 첫번째로 찾은것 return
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

}
