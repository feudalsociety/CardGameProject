using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreateLobbyScreen : UI_Base
{
    [SerializeField] private TMP_InputField _nameInput;

    public static event Action<LobbyData> LobbyCreated;
    private static readonly int _maxPlayers = 2;

    enum Buttons
    {
        Create
    }

    enum Texts
    {
        Create
    }

    private void Start() { }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        GetButton((int)Buttons.Create).gameObject.AddUIEvent(OnCreateClicked);
    }

    public void OnCreateClicked(PointerEventData data)
    {
        var lobbyData = new LobbyData
        {
            Name = _nameInput.text,
            MaxPlayers = _maxPlayers
        };

        LobbyCreated?.Invoke(lobbyData);
    }
}

public struct LobbyData
{
    public string Name;
    public int MaxPlayers;
}