using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerGraves : NetworkBehaviour
{
    List<ServerCard>[] _playerGraves = new List<ServerCard>[NetworkPlayersData.MaxPlayerCount];

    private void Awake()
    {
        for(int i = 0; i < _playerGraves.Length; i++)
            _playerGraves[i] = new List<ServerCard>();
    }

    public void PlayerAddCardToGrave(int playerNumber, ServerCard serverCard)
    {
        _playerGraves[playerNumber].Add(serverCard);
    }

    [ClientRpc]
    private void AddCardToGraveClientRpc(ClientRpcParams clientRpcParams = default)
    {

    }
}
