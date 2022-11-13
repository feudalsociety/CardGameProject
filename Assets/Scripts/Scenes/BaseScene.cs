using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;

    private void Awake()
    {
        Init();
    }

    // EventSystem이 없으면 생성
    // Managers를 참조하면서 Managers 자동 생성
    protected virtual void Init()
    {
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
        {
            Managers.Resource.Instantiate("EventSystem").name = "@EventSystem";
        }
    }

    public abstract void Clear();
}
