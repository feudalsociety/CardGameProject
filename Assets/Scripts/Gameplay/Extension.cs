using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// extension class는 static이어야함
public static class Extension
{

    public static void ChangeLayerRecursively(this GameObject go, int layer)
    {
        go.layer = layer;

        foreach(Transform child in go.transform)
        {
            child.gameObject.layer = layer;
        }
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Util.GetOrAddComponent<T>(go);
    }

    public static void AddUIEvent(this GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.BindEvent(go, action, type);
    }

    public static void RemoveUIEvent(this GameObject go, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.RemoveEvent(go, type);
    }

    public static GameObject findChild(this GameObject go, string name = null, bool recursive = false)
    {
       return Util.findChild(go, name, recursive);
    }

    public static T findChild<T>(this GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        return Util.findChild<T>(go, name, recursive);
    }

    public static bool IsObjectTypeOf<T>(this GameObject go) where T : UnityEngine.Component
    {
        return go.GetComponent<T>() != null;
    }
}
