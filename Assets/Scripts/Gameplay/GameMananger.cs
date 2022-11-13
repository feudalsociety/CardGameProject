using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameMananger : NetworkSingleton<GameMananger>
{
    [Header("Player Color")]
    [SerializeField] public Color Player0OutlineColor;
    [SerializeField] public Color Player1OutlineColor;

    [SerializeField] public Gradient Player0lineRendererColor;
    [SerializeField] public Gradient Player1lineRendererColor;

    [SerializeField] public Color Player0HealthbarColor;
    [SerializeField] public Color Player1HealthbarColor;

    private NetworkManager _netManager => NetworkManager.Singleton;

    [SerializeField] private NetworkPlayersData _networkPlayersData;
    public NetworkPlayersData NetworkPlayersData => _networkPlayersData;

    [SerializeField] private Player _playerPrefab;

    [SerializeField] public PlayerDecks PlayerDecks;
    [SerializeField] public PlayerHands PlayerHands;
    [SerializeField] public PlayerGraves PlayerGraves;

    // TODO : HasGameStarted
    // TODO : IsGameOver

    private void Awake()
    {
        Managers.Input.KeyAction -= OnkeyPressed;
        Managers.Input.KeyAction += OnkeyPressed;
    }

    public override void OnNetworkSpawn()
    {
        MyHandManager.Instance.MyPlayerUI.Init();

        if (IsServer)
        {
            // TODO : OnClientDisconnectCallback -> Remove NetworkPlayersData
            _netManager.SceneManager.OnSceneEvent += GameManager_OnSceneEvent;
            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();
            UI_Utilities.Instance.Log("Session started");
            UI_Utilities.Instance.LogSession(true);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);

        if (sessionPlayerData.HasValue)
        {
            var playerData = sessionPlayerData.Value;
            var playerNumber = playerData.PlayerNumber;
            var playerName = playerData.PlayerName;
            UI_Utilities.Instance.Log($"Player_{playerNumber} Spawned");

            // spawn my player object
            var heroSpawnPos = GameNetPortal.Instance.GetHeroSpawnPos(playerNumber);
            var tile = MapGenerator.Instance.Tiles[heroSpawnPos];
            Vector3 playerPos = tile.transform.position + new Vector3(0, MapGenerator.TileHeight / 2, 0);
            Quaternion playerRot = Quaternion.Euler(0f, GameNetPortal.Instance.GetPlayerSpawnPos(playerNumber).Rotation.y, 0f);

            var player = Instantiate(_playerPrefab, playerPos, playerRot);
            player.GetComponent<NetworkObject>().Spawn();
            player.Init(playerNumber);

            var playerUnit = player.GetComponent<Unit>();

            // TODO :
            // var playerUnit = player.GetComponent<Unit>();
            // playerUnit.Init(heroSpawnPos, playerNumber);
            // tile.PlaceUnitServerRpc(player.NetworkObjectId);

            playerName = string.IsNullOrEmpty(playerName) ? $"Player_{playerNumber}" : playerName;
            SetPlayerNameDisplaysClientRpc(clientId, playerName);

            ClientRpcParams clientRpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }};
            SetPlayerUITransformClientRpc(playerNumber, clientRpcParams);
        }
    }

    [ClientRpc]
    private void SetPlayerNameDisplaysClientRpc(ulong clientId, string playerName)
    {
        if (_netManager.LocalClientId == clientId)
            MyHandManager.Instance.MyPlayerUI.SetMyPlayerNameDisplay(playerName);
        else MyHandManager.Instance.MyPlayerUI.SetEnemyPlayerNameDisplay(playerName);
    }

    [ClientRpc]
    private void SetPlayerUITransformClientRpc(int playerNumber, ClientRpcParams clientRpcParams = default)
    {
        MyUIController.Instance.SetMyPlayerUITransform(GameNetPortal.Instance.GetPlayerSpawnPos(playerNumber));
        MyUIController.Instance.SetEnemyPlayerUITransform(GameNetPortal.Instance.GetPlayerSpawnPos(1 - playerNumber));
    }

    private void SeatNewPlayers()
    {
        foreach (var kvp in _netManager.ConnectedClients)
        {
            // session has started (guaranteed)
            SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(kvp.Key);

            if (sessionPlayerData.HasValue)
            {
                var playerData = sessionPlayerData.Value;
                if (playerData.PlayerNumber == -1 || !IsPlayerNumberAvailable(playerData.PlayerNumber))
                {
                    // If no player num already assigned or if player num is no longer available, get an available one.
                    playerData.PlayerNumber = GetAvailablePlayerNumber();
                }
                if (playerData.PlayerNumber == -1)
                {
                    throw new Exception($"We shouldn't be here, connection approval should have refused this connection already for client ID {kvp.Key} and player num {playerData.PlayerNumber}");
                }

                NetworkPlayersData.NetworkPlayers.Add(
                    new NetworkPlayersData.NetworkPlayerState(kvp.Key, playerData.PlayerName, playerData.PlayerNumber));
                SessionManager<SessionPlayerData>.Instance.SetPlayerData(kvp.Key, playerData);
            }
        }
    }

    private int GetAvailablePlayerNumber()
    {
        for(int possiblePlayerNumber = 0; possiblePlayerNumber < NetworkPlayersData.MaxPlayerCount; ++possiblePlayerNumber)
            {
            if (IsPlayerNumberAvailable(possiblePlayerNumber))
            {
                return possiblePlayerNumber;
            }
        }
        // we couldn't get a Player# for this person... which means the lobby is full!
        return -1;
    }

    private bool IsPlayerNumberAvailable(int playerNumber)
    {
        bool found = false;
        foreach (NetworkPlayersData.NetworkPlayerState playerState in NetworkPlayersData.NetworkPlayers)
        {
            if (playerState.PlayerNumber == playerNumber)
            {
                found = true;
                break;
            }
        }
        return !found;
    }

    [ServerRpc(RequireOwnership = false)]
    public void GoToMainMenuServerRpc()
    {
        SceneLoadManager.Instance.LoadScene(Define.Scene.MainMenu, useNetworkSceneManager: true);
        SessionManager<SessionPlayerData>.Instance.OnSessionEnded();
        UI_Utilities.Instance.LogSession(false);
    }

    // server에서만 실행, Server에서만 handler를 추가함
    private void GameManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;

        var sceneName = SceneLoadManager.GetSceneName(Define.Scene.GamePlay);
        var sceneBuildIndex = SceneManager.GetSceneByName(sceneName).buildIndex;

        if (GameNetPortal.Instance.AreAllClientsInSameScene(sceneBuildIndex)) // 한번만 실행됨
        {
            MapGenerator.Instance.GenerateMapServerRpc();

            // also set network player datas
            SeatNewPlayers();

            // SpawnPlayer가 Tile에 의존하고 있기 때문에 client가 server 보다 먼저 sceneLoad할 경우 문제가 생길 수 있다.
            // 따라서 Server를 포함한 모든 client가 성공적으로 scene을 loaded했을 때 player prefab을 spawn한다.
            foreach (var id in _netManager.ConnectedClientsIds) SpawnPlayer(id);

            PlayerDecks.GeneratePlayerDecks();
            PlayerHands.DrawPlayersOpeningHand();
            TurnManager.Instance.StartGameServerRpc();
        }
    }

    public (int, ClientRpcParams) GetPlayerNumberAndClientRpcParam(ulong clientId)
    {
        SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
        if(sessionPlayerData.HasValue)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };
            return (sessionPlayerData.Value.PlayerNumber, clientRpcParams);
        }
        return (-1, new ClientRpcParams());
    }

    // TODO : GameManager로 옮기기
    void OnkeyPressed()
    {
        // TODO : only server can trigger drawing cards but this is for testing purpose
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            try
            { 
                // first it needs to check if client side is ready for drawing card (client side check)
                if (MyHandManager.Instance.CardNum >= MyHandManager.ClientMaxHandCardNum)
                    throw new Exception("Your hand is full");

                if (MyHandManager.Instance.SelectedIndex != -1)
                    throw new Exception("You can't add a card to the hand card while selecting other card");

                PlayerDecks.RequestDrawCardFromDeckServerRpc();
            }
            catch (Exception ex)
            {
                UI_Utilities.Instance.LogError($"Drawing card error : {ex.Message}");
            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            PlayerHands.RequestMoveCardToGraveFromHandServerRpc(MyHandManager.Instance.SelectedIndex);
        }
    }

    public override void OnDestroy()
    {
        if(_netManager != null && _netManager.IsServer)
            _netManager.SceneManager.OnSceneEvent -= GameManager_OnSceneEvent;

        base.OnDestroy();
    }
}