using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerHands : NetworkBehaviour
{
    [SerializeField] private PlayerDecks _playerDecks;
    [SerializeField] private PlayerGraves _playerGraves;
    [SerializeField] private MyHandManager _myHandManager;

    List<ServerCard>[] _playerHands = new List<ServerCard>[NetworkPlayersData.MaxPlayerCount];

    private static readonly int _openingDrawCardNum = 5;
    private static readonly int _maxHandCardNum = 10;

    private void Awake()
    {
        for (int i = 0; i < _playerHands.Length; i++)
            _playerHands[i] = new List<ServerCard>();
    }

    public void PlayerAddCardToHand(int playerNumber, ServerCard serverCard)
    {
        if (_playerHands[playerNumber].Count >= _maxHandCardNum)
            throw new Exception($"PlayerAddCardToHand error, player{playerNumber} hand is full");

        _playerHands[playerNumber].Add(serverCard);
    }

    private ServerCard PlayerRemoveCardFromHand(int playerNumber, int selectedIndex)
    {
        if(selectedIndex < 0 || selectedIndex >= _playerHands[playerNumber].Count)
            throw new Exception($"PlayerRemoveCardFromHand error, Player{playerNumber} hand index[{selectedIndex}] range error");

        var serverCard = _playerHands[playerNumber][selectedIndex];
        _playerHands[playerNumber].RemoveAt(selectedIndex);
        return serverCard;
    }

    public void DrawPlayersOpeningHand()
    {
        try
        {
            for (int i = 0; i < _playerHands.Length; i++)
            {
                for (int j = 0; j < _openingDrawCardNum; j++)
                {
                    var playerCard = _playerDecks.PlayerRemoveTopCardFromDeck(i);
                    if (playerCard != null) _playerHands[i].Add(playerCard);
                }
            }

            DrawOpeningHandClientRpc();
        }
        catch (Exception ex)
        {
            UI_Utilities.Instance.LogErrorClientRpc($"DrawPlayersOpeningHand denied : {ex.Message}");
        }
    }

    [ClientRpc]
    private void DrawOpeningHandClientRpc()
    {
        _myHandManager.DrawOpeningHand();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayCardFromHandServerRpc(int selectedIndex, HexCoords coord, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        (int playerNumber, ClientRpcParams clientRpcParams) t = GameMananger.Instance.GetPlayerNumberAndClientRpcParam(clientId);
        if (t.playerNumber == -1) return;

        try
        {
            if (TurnManager.Instance.WhosTurn != t.playerNumber)
                throw new Exception("Its is not your turn");

            var serverCard = PlayerRemoveCardFromHand(t.playerNumber, selectedIndex);
            var cardData = serverCard.CardData;

            if (cardData.CardType == Define.CardType.Unit)
            {
                if (!MapGenerator.Instance.Tiles[coord].Walkable) 
                    throw new Exception("Unable to place unit in this tile");

                UnitManager.Instance.SpawnUnit(t.playerNumber, serverCard, coord);
                PlayCardFromHandClientRpc(selectedIndex, MapGenerator.Instance.Tiles[coord].NetworkObjectId, t.clientRpcParams);
            }
        }
        catch (Exception ex)
        {
            ReleaseSelectedCardClientRpc(t.clientRpcParams);
            UI_Utilities.Instance.LogErrorClientRpc($"RequestPlayCardFromHandServerRpc denied : {ex.Message}", t.clientRpcParams);
        }
    }

    // can use reference networkobject reference, instead of coord
    // TODO : Or use SpawnedManager to get reference to the tile
    [ClientRpc]
    private void PlayCardFromHandClientRpc(int selectedIndex, ulong tileNetId, ClientRpcParams clientRpcParams = default)
    {
        _myHandManager.PlayCardFromHand(selectedIndex, tileNetId);
    }

    [ClientRpc]
    private void ReleaseSelectedCardClientRpc(ClientRpcParams clientRpcParams = default)
    {
        _myHandManager.ReleaseSelectedCard();
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveCardToGraveFromHandServerRpc(int selectedIndex, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        (int playerNumber, ClientRpcParams clientRpcParams) t = GameMananger.Instance.GetPlayerNumberAndClientRpcParam(clientId);
        if (t.playerNumber == -1) return;

        try
        {
            var serverCard = PlayerRemoveCardFromHand(t.playerNumber, selectedIndex);
            _playerGraves.PlayerAddCardToGrave(t.playerNumber, serverCard);
            MoveCardToGraveFromHandClientRpc(selectedIndex, t.clientRpcParams);
        }
        catch (Exception ex)
        {
            UI_Utilities.Instance.LogErrorClientRpc($"RequestMoveCardToGraveFromHandServerRpc denied : {ex.Message}");
        }
    }

    [ClientRpc]
    private void MoveCardToGraveFromHandClientRpc(int selectedIndex, ClientRpcParams clientRpcParams = default)
    {
        _myHandManager.MoveCardToGraveFromHand(selectedIndex);
    }
}
