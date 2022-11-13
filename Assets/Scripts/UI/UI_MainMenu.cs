using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using Unity.Netcode;
using System;

public class UI_MainMenu : UI_Scene
{
    private NetworkManager _netManager => NetworkManager.Singleton;

    enum Images
    {
        BackImage
    }

    enum Buttons
    {
        Play,
        Lobby,
        BuildDecks,
        Options,
        Quit
    }

    enum Texts
    {
        PlayText,
        Title,
        LobbyText,
        BuildText,
        OptionText,
        QuitText
    }

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        // reflection을 이용하여 enum을 넘겨준다.
        // 이름과 <T> component를 갖고 있는 object를 찾는 것이 목표
        Bind<Image>(typeof(Images));
        //Bind<Button>(typeof(Buttons));
        //Bind<TMP_Text>(typeof(Texts));

        // Event 추가
        //GetButton((int)Buttons.Play).gameObject.AddUIEvent(JumptoGamePlay);
        //GetButton((int)Buttons.Lobby).gameObject.AddUIEvent(GotoLobbyScene);
        //GetButton((int)Buttons.BuildDecks).gameObject.AddUIEvent(BuildDecks);
        //GetButton((int)Buttons.Options).gameObject.AddUIEvent(OpenOptionPopup);
        //GetButton((int)Buttons.Quit).gameObject.AddUIEvent(QuitGame);
    }

    public void OpenOptionPopup()
    {
        UI_Options optionPopup = Managers.UI.ShowPopupUI<UI_Options>("UI_Options");
        optionPopup.gameObject.GetComponent<UIFadeScript>().FadeIn(fadeInDuration: 0.06f);
    }

    public void JumptoGamePlay() 
    {
        if(!_netManager.IsServer)
        {
            UI_Utilities.Instance.LogError("Only Server can start the Game");
            return;
        }

        int playerNum = _netManager.ConnectedClientsIds.Count;
        if (playerNum > 2 || playerNum < 2)
        {
            UI_Utilities.Instance.LogError("You need two connected clients to play the game");
            return;
        }

        SceneLoadManager.Instance.LoadScene(Define.Scene.GamePlay, useNetworkSceneManager: true);
    }
    public void GotoLobbyScene() { SceneLoadManager.Instance.LoadScene(Define.Scene.Lobby, useNetworkSceneManager: false); }
    public void BuildDecks() { SceneLoadManager.Instance.LoadScene(Define.Scene.Deckbuilder, useNetworkSceneManager: false); }
    public void QuitGame()
    {
        ApplicationController.Instance.QuitGame();
    }
}
