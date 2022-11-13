#if UNITY_EDITOR
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class DefaultSceneLoader
{
    static DefaultSceneLoader()
    {
        EditorApplication.playModeStateChanged += ModeChanged;

        var pathOfFirstScene = EditorBuildSettings.scenes[0].path;
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);
        EditorSceneManager.playModeStartScene = sceneAsset;
    }

    static void ModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // �ٸ� client�� disconnect �Ѱ����� �����ϴ°��� �����ϱ� ����
            NetworkManager.Singleton.Shutdown();
            UI_Utilities.Instance.LogServer(false);
            UI_Utilities.Instance.Log("Network Shutdowned");
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();

            // if in the lobby scene
            RoomScreen roomScreen = Object.FindObjectOfType<RoomScreen>();
            if (roomScreen != null)
                roomScreen.OnLeaveLobby();
        }
    }
}
#endif