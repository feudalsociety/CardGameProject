using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    // �ֱٿ� ����� UI�� order
    int _order = 10; // �ȿ��� ����ϴ� UI_Popup������ order
    public static int LoadingOrder = 50;

    // ���� �������� ��� ���� ���� ���� - stack ����
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

    // �ܺο��� Popup�� ���� �� UIManger���� Canvus order�� ������
    // sort�� �ʿ���� SceneUI�� ��� sortOrder�� �ڵ����� 0���� ������
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

        // go�� Canvus Component�� ������ ����
        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = renderMode;
        if (renderMode == RenderMode.ScreenSpaceCamera || renderMode == RenderMode.WorldSpace)
            canvas.worldCamera = camera;

        // ��ø�� �� canvas�� �θ��� sorting order�� ������� �ڽ��� sorting order�� �����ڴ�
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else // Poupup�̶� ���� ���� UI
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

        // �θ� ����
        if (parent == null) go.transform.SetParent(Root.transform);
        else go.transform.SetParent(parent.transform);

        return sceneUI;
    }

    // name�� prefab�� �̸�, T�� script�� �ǳ��ٰ���,  
    // script �̸��� prefab�� �̸��� ������ �������� �̸��� ������� ������ T�� �״�� ���
    public T ShowPopupUI<T>(string name = null, GameObject parent = null) where T : UI_Popup
    {
        // �־��� type�� �̸� ����
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        // �ش� component�� ������ �߰��� get
        T popup = go.GetOrAddComponent<T>();
        _popupStack.Push(popup);
        // _order++;  // Scene�� �ٰ� drag&drop�ؼ� ���� UI�� ó���ȵ�
        // UI_Popup�� start�� �� �־��� ��

        // �θ� ����
        if (parent == null) go.transform.SetParent(Root.transform);
        else go.transform.SetParent(parent.transform);

        return popup;
    }

    // ���� ������ ����ؼ� ������ �����Ǵ� �ְ� �´��� test
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

        // ���� �ֱٿ� ��� Popup
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
