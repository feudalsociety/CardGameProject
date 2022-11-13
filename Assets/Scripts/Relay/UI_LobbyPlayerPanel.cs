using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_LobbyPlayerPanel : UI_Base
{
    enum Texts
    {
        Name,
        Status
    }

    public ulong PlayerId { get; private set; }

    public override void Init()
    {
    }

    public void Init(ulong playerId)
    {
        Bind<TMP_Text>(typeof(Texts));

        PlayerId = playerId;
        GetTMPText((int)Texts.Name).text = $"Player {playerId}";
    }

    public void SetReady()
    {
        GetTMPText((int)Texts.Status).text = "Ready";
        GetTMPText((int)Texts.Status).color = Color.green;
    }
}
