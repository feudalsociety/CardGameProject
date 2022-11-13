using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Object _lock = new Object();
    static Managers _instance;
    static Managers Instance 
    { 
        get 
        {
            if (_instance == null && Time.timeScale != 0)
            {
                lock(_lock) CreateInstance();
            }
            return _instance; 
        } 
    }

    InputManager _input = new InputManager();
    UIManager _ui = new UIManager();
    ResourceManager _resource = new ResourceManager();

    public static InputManager Input { get { return Instance._input; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static ResourceManager Resource { get { return Instance._resource; } }

    void Start()
    {
        CreateInstance();
    }

    void Update()
    {
        _input.OnUpdate();
    }

    static void CreateInstance()
    {
        GameObject go = GameObject.Find("@Managers");

        if (go != null)
        {
            _instance = go.GetOrAddComponent<Managers>();
            DontDestroyOnLoad(go);

        }
        else
        {
            go = new GameObject { name = "@Managers" };
            go.GetOrAddComponent<Managers>();

            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
        }
    }

    public static void Clear()
    {
        Input.Clear();
        UI.Clear();

        // 다른 곳에서 Pooling된 object를 사용할 수 있으므로 마지막에
        // Pool.Clear();
    }

    private void OnApplicationQuit()
    {
        Time.timeScale = 0;
    }
}
