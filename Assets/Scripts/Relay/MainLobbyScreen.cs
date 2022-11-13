using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

public class MainLobbyScreen : UI_Base
{
    private readonly List<UI_LobbyRoomPanel> _currentLobbySpawns = new();
    [SerializeField] private Transform _lobbyParent;
    [SerializeField] private float _lobbyRefreshRate = 2;
    private float _nextRefreshTime;

    enum Buttons
    {
        Exit,
        Create
    }

    enum Texts
    {
        NoLobbies,
        Exit,
        Create
    }

    enum Images
    {
        Scroll,
        View
    }

    enum GameObjects
    {
        Content,
        Scrollbar,
        SlidingArea,
        Handle
    }
    

    private void Start() { }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));

        GetButton((int)Buttons.Exit).gameObject.AddUIEvent(GotoMainMenu);
    }

    private void GotoMainMenu(PointerEventData data)
    {
        SceneLoadManager.Instance.LoadScene(Define.Scene.MainMenu, useNetworkSceneManager: false);
    }

    private void OnEnable()
    {
        foreach (Transform child in _lobbyParent) Destroy(child.gameObject);
        _currentLobbySpawns.Clear();
    }

    private void Update()
    {
        if (Time.time >= _nextRefreshTime) FetchLobbies();
    }

    private async void FetchLobbies()
    {
        try
        {
            _nextRefreshTime = Time.time + _lobbyRefreshRate;

            // Grab all current lobbies, List<Lobby>
            var allLobbies = await MatchmakingService.GatherLobbies();

            // Destroy all the current lobby panels which don't exist anymore.
            // Exclude our own homes as it'll show for a brief moment after closing the room
            var lobbyIds = allLobbies.Where(l => l.HostId != Authentication.PlayerId).Select(l => l.Id); // IEnumerable<string>
            var notActive = _currentLobbySpawns.Where(l => !lobbyIds.Contains(l.Lobby.Id)).ToList(); // List<LobbyRoomPanel>

            foreach (var panel in notActive)
            {
                Destroy(panel.gameObject);
                _currentLobbySpawns.Remove(panel);
            }

            // Update or spawn the remaining active lobbies
            foreach (var lobby in allLobbies)
            {
                var current = _currentLobbySpawns.FirstOrDefault(p => p.Lobby.Id == lobby.Id);
                if (current != null)
                {
                    current.UpdateDetails(lobby);
                }
                else // current == null
                {
                    var panel = Managers.UI.MakeSubItem<UI_LobbyRoomPanel>(parent: _lobbyParent);
                    panel.Init(lobby);
                    _currentLobbySpawns.Add(panel);
                }
            }

            GetTMPText((int)Texts.NoLobbies).gameObject.SetActive(!_currentLobbySpawns.Any());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}