using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : Singleton<ApplicationController>
{
    private NetworkManager _netManager => NetworkManager.Singleton;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // don't want loading sceen to be appear
        SceneManager.LoadScene("Auth");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        // shutdown will automatically happens in editor script
#else
        _netManager.Shutdown();
        UI_Utilities.Instance.LogServer(false);
        UI_Utilities.Instance.Log("Network Shutdowned");
        SessionManager<SessionPlayerData>.Instance.OnServerEnded();
        UI_Utilities.Instance.SessionText.gameObject.SetActive(false);
        Application.Quit();
#endif
    }
}
