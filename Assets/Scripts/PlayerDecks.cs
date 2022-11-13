using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.IO;
using Newtonsoft.Json;

public class PlayerDecks : NetworkBehaviour
{
    private NetworkManager _netManager => NetworkManager.Singleton;
    [SerializeField] private PlayerHands _playerHands;
    [SerializeField] private UI_Deck _deckUI;

    private string _sharedDeckListPath;

    // int : cardId, [0] : player0, [1] : player1
    List<int>[] _playerDeckLists = new List<int>[NetworkPlayersData.MaxPlayerCount];
    List<ServerCard>[] _playerDecks = new List<ServerCard>[NetworkPlayersData.MaxPlayerCount];

    // awake - start - onNetworkspawn
    private void Awake()
    {
        _sharedDeckListPath = Path.Combine(Application.dataPath, "../../SharedDeckList/sharedDeckList.json");
        
        for(int i = 0; i < _playerDeckLists.Length; i++)
        {
            _playerDeckLists[i] = new List<int>();
            _playerDecks[i] = new List<ServerCard>();
        }
    }

    public void GeneratePlayerDecks()
    {
        LoadPlayersDeckList();
        for (int i = 0; i < _playerDeckLists.Length; i++) _playerDeckLists[i] = ShuffleDeckList(_playerDeckLists[i]);
        InstantiateServerCardsFromPlayerDeckLists();

        for (int i = 0; i < _playerDeckLists.Length; i++) _playerDeckLists[i] = ShuffleDeckList(_playerDeckLists[i]);
        UI_Utilities.Instance.Log("Generated Player Decks");
    }
    
    // TODO : networkserialize deckList data
    [ClientRpc]
    public void InitializeDeckListClientRpc(int[] deckListArray, ClientRpcParams clientRpcParams = default)
    {
        _deckUI.InitializeDeckList(deckListArray);
    }

    private void LoadPlayersDeckList()
    {
        string jsonData = File.ReadAllText(_sharedDeckListPath);
        for (int i = 0; i < _playerDeckLists.Length; i++)
            _playerDeckLists[i] = JsonConvert.DeserializeObject<List<int>>(jsonData);

        foreach (var clientId in _netManager.ConnectedClientsIds)
        {
            (int playerNumber, ClientRpcParams clientRpcParams) t = 
                GameMananger.Instance.GetPlayerNumberAndClientRpcParam(clientId);

            if (t.playerNumber == -1) return;

            var deckListArray = _playerDeckLists[t.playerNumber].ToArray();
            InitializeDeckListClientRpc(deckListArray, t.clientRpcParams);
        }
    }

    private void InstantiateServerCardsFromPlayerDeckLists()
    {
        foreach (var clientId in _netManager.ConnectedClientsIds)
        {
            (int playerNumber, ClientRpcParams clientRpcParams) t = GameMananger.Instance.GetPlayerNumberAndClientRpcParam(clientId);
            if (t.playerNumber == -1) return;

            for (int i = 0; i < _playerDeckLists[0].Count; i++)
            {
                var serverCardData = CardDB.Instance.GetCardData(cardId: _playerDeckLists[t.playerNumber][i]);
                _playerDecks[t.playerNumber].Add(new ServerCard(serverCardData));
                switch (serverCardData.CardType)
                {
                    case Define.CardType.Unit:
                        var serverUnitData = serverCardData as ServerUnitBaseData;

                        // convert serverdata into clientdata
                        var clientCardData = new ClientUnitBaseData(
                                serverUnitData.CardId,
                                serverUnitData.CardName,
                                serverUnitData.CardType,
                                serverUnitData.Cost,
                                serverUnitData.Attack,
                                serverUnitData.Hp,
                                serverUnitData.Agility);

                        InstantiateClientUnitCardClientRpc(clientCardData, t.clientRpcParams);
                        break;
                }
            }
        }
    }

    [ClientRpc]
    private void InstantiateClientUnitCardClientRpc(ClientUnitBaseData unitBaseData, ClientRpcParams clientRpcParams = default)
    {
        _deckUI.InstantiateClientUnitCard(unitBaseData);
    }

    private List<int> ShuffleDeckList(List<int> _deckList)
    {
        for (int i = _deckList.Count - 1; i > 0; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i);

            int temp = _deckList[i];
            _deckList[i] = _deckList[rnd];
            _deckList[rnd] = temp;
        }

        return _deckList;
    }

    public ServerCard PlayerRemoveTopCardFromDeck(int playerNumber)
    {
        if (_playerDecks[playerNumber].Count <= 0)
            throw new Exception($"PlayerRemoveCardFromDeck error, No more cards left in player{playerNumber} deck");

        var serverCard = _playerDecks[playerNumber][_playerDecks[playerNumber].Count - 1];
        _playerDecks[playerNumber].RemoveAt(_playerDecks[playerNumber].Count - 1);
        _playerDeckLists[playerNumber].Remove(_playerDeckLists[playerNumber].Find(cardId => cardId == serverCard.CardData.CardId));

        return serverCard;
    }

    // TODO : This is reqeust for now 
    [ServerRpc(RequireOwnership = false)]
    public void RequestDrawCardFromDeckServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        (int playerNumber, ClientRpcParams clientRpcParams) t = GameMananger.Instance.GetPlayerNumberAndClientRpcParam(clientId);
        if (t.playerNumber == -1) return;

        try
        {
            var serverCard = PlayerRemoveTopCardFromDeck(t.playerNumber);
            _playerHands.PlayerAddCardToHand(t.playerNumber, serverCard);
            DrawACardFromDeckClientRpc(t.clientRpcParams);
        }
        catch (Exception ex)
        {
            UI_Utilities.Instance.LogErrorClientRpc($"RequestDrawCardFromDeckServerRpc denied : {ex.Message}");
        }
    }

    [ClientRpc]
    private void DrawACardFromDeckClientRpc(ClientRpcParams clientRpcParams = default)
    {
        _deckUI.DrawCardFromDeck();
    }
}
