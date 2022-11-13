using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    // 최근에 사용한 UI의 order
    int _order = 10; // 안에서 사용하는 UI_Popup끼리의 order
    public static int LoadingOrder = 50;

    // 가장 마지막에 띄운 것이 먼저 삭제 - stack 구조
    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    public UI_Scene CurrentSceneUI { get; private set; } = null;
    public int PopupNum { get { return _popupStack.Count; } }

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject { name = "@UI_Root" };

            return root;
        }
    }

    // 외부에서 Popup이 켜질 때 UIManger에서 Canvus order을 지정함
    // sort가 필요없는 SceneUI인 경우 sortOrder를 자동으로 0으로 지정함
    public void SetCanvus(GameObject go, bool sort = true, RenderMode renderMode = RenderMode.ScreenSpaceOverlay, Camera camera = null)
    {
        switch (renderMode)
        {
            case RenderMode.ScreenSpaceOverlay:
                if (camera != null)
                {
                    Debug.Log($"You should not specify camera in {renderMode} RenderMode");
                    return;
                }
                break;
            case RenderMode.ScreenSpaceCamera:
            case RenderMode.WorldSpace:
                if (camera == null)
                {
                    Debug.Log($"You should specify camera in {renderMode} RenderMode");
                    return;
                }
                break;
        }

        // go에 Canvus Component가 없으면 부착
        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = renderMode;
        if (renderMode == RenderMode.ScreenSpaceCamera || renderMode == RenderMode.WorldSpace)
            canvas.worldCamera = camera;

        // 중첩이 된 canvas라도 부모의 sorting order과 상관없이 자신의 sorting order을 가지겠다
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else // Poupup이랑 연관 없는 UI
        {
            canvas.sortingOrder = 0;
        }
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name)) name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}");

        if (parent != null)
            go.transform.SetParent(parent, false);

        return go.GetOrAddComponent<T>();
    }

    public T ShowSceneUI<T>(string name = null, GameObject parent = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name)) name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
        T sceneUI = go.GetOrAddComponent<T>();
        CurrentSceneUI = sceneUI;

        // 부모 지정
        if (parent == null) go.transform.SetParent(Root.transform);
        else go.transform.SetParent(parent.transform);

        return sceneUI;
    }

    // name은 prefab의 이름, T로 script를 건내줄거임,  
    // script 이름과 prefab의 이름을 맞춰줄 것이지만 이름을 명시하지 않으면 T를 그대로 사용
    public T ShowPopupUI<T>(string name = null, GameObject parent = null) where T : UI_Popup
    {
        // 넣어준 type의 이름 추출
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        // 해당 component가 없으면 추가후 get
        T popup = go.GetOrAddComponent<T>();
        _popupStack.Push(popup);
        // _order++;  // Scene에 다가 drag&drop해서 상성한 UI가 처리안됨
        // UI_Popup이 start될 때 넣어줄 것

        // 부모 지정
        if (parent == null) go.transform.SetParent(Root.transform);
        else go.transform.SetParent(parent.transform);

        return popup;
    }

    // 내가 누군지 명시해서 실제로 삭제되는 애가 맞는지 test
    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed");
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0) return;

        // 가장 최근에 띄운 Popup
        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public UI_Popup GetPopupUI()
    {
        return _popupStack.Peek();
    }

    public void Clear()
    {
        CloseAllPopupUI();
        CurrentSceneUI = null;
    }
}
