using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_LobbyRoomPanel : UI_Base
{
    enum Texts
    {
        RoomName,
        PlayerCount
    }

    public Lobby Lobby { get; private set; }
    public static event Action<Lobby> LobbySelected;

    public override void Init()
    {
    }

    public void Init(Lobby lobby)
    {
        Bind<TMP_Text>(typeof(Texts));
        gameObject.AddUIEvent(Clicked);
        UpdateDetails(lobby);
    }

    public void UpdateDetails(Lobby lobby)
    {
        Lobby = lobby;

        GetTMPText((int)Texts.RoomName).text = lobby.Name;
        GetTMPText((int)Texts.PlayerCount).text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void Clicked(PointerEventData data)
    {
        LobbySelected?.Invoke(Lobby);
    }
   
}
