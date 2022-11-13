using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections.Generic;

public class SceneLoadManager : NetworkSingleton<SceneLoadManager>
{
    private NetworkManager _netManager => NetworkManager.Singleton;
    private Scene _loadedScene;

    bool IsNetworkSceneManagementEnabled =>
        _netManager != null &&
        _netManager.SceneManager != null &&
        _netManager.NetworkConfig.EnableSceneManagement;

    public BaseScene CurrentScene
    {
        get { return GameObject.FindObjectOfType<BaseScene>(); }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void AddOnSceneEventCallback()
    {
        if (IsNetworkSceneManagementEnabled)
        {
            _netManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
        }
    }

    public async void LoadScene(Define.Scene type, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (useNetworkSceneManager)
        {
            using (new Load($"Loading..."))
            {
                Clear();
                Managers.Clear();
                await Task.Delay(150);
                _netManager.SceneManager.LoadScene(GetSceneName(type), loadSceneMode);
                await Task.Delay(100);
            }
        }
        else
        {
            // Load using SceneManager
            using (new Load($"Loading..."))
            {
                Clear();
                Managers.Clear();
                await Task.Delay(150);
                SceneManager.LoadSceneAsync(GetSceneName(type), loadSceneMode);
                await Task.Delay(100);
            }
        }
    }

    // Scene 이동은 어쩌다가 한번하는 것이므로 Wrapping 한다고 해서 성능 부하가 눈에 보이지 않을 것이다.
    public static string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        // Only additively loaded scenes can be unloaded

        var clientOrServer = sceneEvent.ClientId == NetworkManager.ServerClientId ? "server" : "client";
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.LoadComplete:
            {
                if (sceneEvent.ClientId == NetworkManager.ServerClientId)
                {
                    // Keep track of the loaded scene, you need this to unload it
                    _loadedScene = sceneEvent.Scene;
                }
                UI_Utilities.Instance.Log($"Loaded the {sceneEvent.SceneName} scene on " +
                    $"{clientOrServer}-({sceneEvent.ClientId}).");
                break;
            }
            case SceneEventType.LoadEventCompleted:
            {
                var load = sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted ? "Load" : "Unload";
                    UI_Utilities.Instance.Log($"{load} event completed for the following client " +
                    $"identifiers : ({string.Join(",", sceneEvent.ClientsThatCompleted)})");
                if (sceneEvent.ClientsThatTimedOut.Count > 0)
                {
                        UI_Utilities.Instance.LogError($"{load} event timed out for the following client " +
                        $"identifiers : ({string.Join(",", sceneEvent.ClientsThatTimedOut)})");
                }
                break;
            }
        }
    }

    public void Clear()
    {
        if(CurrentScene != null)
            CurrentScene.Clear();
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager != null && NetworkManager.SceneManager != null)
        {
            _netManager.SceneManager.OnSceneEvent -= SceneManager_OnSceneEvent;
        }
    }
}
