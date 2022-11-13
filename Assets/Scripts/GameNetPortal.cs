using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

[Serializable]
public class ConnectionPayload
{
    public string playerId;
    public string playerName;
    public int clientScene = -1;
}

public class GameNetPortal : NetworkSingleton<GameNetPortal>
{
    private NetworkManager _netManager => NetworkManager.Singleton;

    // light protection against DOS attacks 
    const int _maxConnectPayload = 1024;

    // Keeps a list of what clients are in what scenes.
    Dictionary<ulong, int> _clientSceneMap = new Dictionary<ulong, int>();

    public string PlayerName;

    private NetworkVariable<int> _playersInGame =
        new NetworkVariable<int>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);
    public int PlayersInGame => _playersInGame.Value;

    // TODO : 이 정보를 다른 곳으로 옮길까? : 아마도 PlayerScene에 옮기지 않을까
    private HexCoords[] _heroSpawnCoords = new HexCoords[2]
    {
       new HexCoords(0, 0),
       new HexCoords(0, 12)
    };

    private UITransform[] _playerSpawnPos = new UITransform[2]
    {
        new UITransform(new Vector3(30f, 30f, 0f), 500f, 800f),
        new UITransform(new Vector3(30f, -150f, 0f), 500f, 800f)
    };

    public HexCoords GetHeroSpawnPos(int index) 
    {
        return _heroSpawnCoords[index];
    }

    public UITransform GetPlayerSpawnPos(int index)
    {
        return _playerSpawnPos[index];
    }

    private void Awake()
    {
        _netManager.ConnectionApprovalCallback += ApprovalCheck;
        _netManager.OnServerStarted += OnServerStarted;
        _netManager.OnClientConnectedCallback += OnClientConnect;
        _netManager.OnClientDisconnectCallback += OnClientDisconnect;

        DontDestroyOnLoad(gameObject);
    }


    // The initial request contains, among other things, binary data passed into StartClient.
    // In our case, this is the client's GUID,
    // which is a unique identifier for their install of the game that persists across app restarts.
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var connectionData = request.Payload;
        var clientId = request.ClientNetworkId;
        if (connectionData.Length > _maxConnectPayload)
        {
            response.Approved = false;
            return;
        }

        // Approval check happens for Host too, but obviously we want it to be approved
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, Authentication.PlayerId,
                new SessionPlayerData(clientId, PlayerName, true));

            response.Approved = true;
            response.CreatePlayerObject = false;
            return;
        }

        var payload = System.Text.Encoding.UTF8.GetString(connectionData);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

        if(CanClientConnect(connectionPayload))
        {
            SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                new SessionPlayerData(clientId, connectionPayload.playerName, true));

            _clientSceneMap.Add(clientId, connectionPayload.clientScene);

            response.Approved = true;
            response.CreatePlayerObject = false;
            return;
        }

        response.Approved = false;
    }

    private bool CanClientConnect(ConnectionPayload connectionPayload)
    {
        if (_netManager.ConnectedClientsIds.Count > NetworkPlayersData.MaxPlayerCount) return false;
        return !SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId);
    }

    public void ConnectClient()
    {
        var payload = JsonUtility.ToJson(new ConnectionPayload()
        {
            playerId = Authentication.PlayerId,
            playerName = PlayerName,
            clientScene = SceneManager.GetActiveScene().buildIndex
        });

        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        _netManager.NetworkConfig.ConnectionData = payloadBytes;

        if (_netManager.StartClient())
        {
            SceneLoadManager.Instance.AddOnSceneEventCallback();
            UI_Utilities.Instance.Log("Client started");
        }
        else UI_Utilities.Instance.LogError("Unable to start client");
    }

    // This will disconnect (on the client) or shutdown the server (on the host)
    // It's a local signal (not from the network), indicating that the user has requested a disconnect.
    public void RequestDisconnect()
    {
        if (_netManager.IsServer)
        {
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();
            UI_Utilities.Instance.LogSession(false);
        }

        if (_netManager.IsClient && !_netManager.IsServer)
        {
            _netManager.Shutdown();
            UI_Utilities.Instance.Log("Network Shutdowned");
            UI_Utilities.Instance.LogServer(false);
        }

        if (_netManager.IsHost)
        {
            _netManager.Shutdown();
            UI_Utilities.Instance.LogServer(false);
            UI_Utilities.Instance.Log("Network Shutdowned");
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();
            UI_Utilities.Instance.SessionText.gameObject.SetActive(false);
        }
    }

    private void OnServerStarted()
    {
        SceneLoadManager.Instance.AddOnSceneEventCallback();
        UI_Utilities.Instance.LogServer(true);
        UI_Utilities.Instance.SessionText.gameObject.SetActive(true);
    }

    // Only called if server started
    private void OnClientConnect(ulong clientId)
    {
        _clientSceneMap.Remove(clientId);

        UI_Utilities.Instance.Log($"ClientId : [{clientId}] just connected");
        UI_Utilities.Instance.LogServer(true);

        if (_netManager.IsServer)
        {
            _netManager.SceneManager.OnSceneEvent += OnClientSceneChanged;
            _clientSceneMap.Add(clientId, SceneManager.GetActiveScene().buildIndex);
            _playersInGame.Value += 1;
        }
    }

    // Handles the case where NetworkManager has told us a client has disconnected.
    // This includes ourselves, if we're the host, and the server is stopped
    private void OnClientDisconnect(ulong clientId)
    {   
        UI_Utilities.Instance.Log($"ClientId : [{clientId}] just disconnected");

        if (_netManager.IsServer)
        {
            _clientSceneMap.Remove(clientId);
            _playersInGame.Value -= 1;
        }
        if (clientId == _netManager.LocalClientId)
        {
            _clientSceneMap.Clear();

            _netManager.SceneManager.OnSceneEvent -= OnClientSceneChanged;
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();

            if(MatchmakingService.CurrentLobby != null)
                MatchmakingService.DeleteLobbyAsync();
        }
        else
        {
            // session이 시작되지 않아도 데이터는 얻을 수 있다. 
            // 하지만 server만 아래 코드를 처리한다.
            var playerId = SessionManager<SessionPlayerData>.Instance.GetPlayerId(clientId);
            if(!string.IsNullOrEmpty(playerId))
            {
                if (MatchmakingService.CurrentLobby != null)
                    MatchmakingService.RemovePlayerFromLobbyAsync(playerId);

                var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(playerId);
                if (sessionData.HasValue)
                    UI_Utilities.Instance.Log($"Player Name : {sessionData.Value.PlayerName} disconnected");
                SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientId);
            }
        }
    }

    private void OnClientSceneChanged(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
        _clientSceneMap[sceneEvent.ClientId] = SceneManager.GetSceneByName(sceneEvent.SceneName).buildIndex;
    }

    public bool AreAllClientsInSameScene(int sceneBuildIndex)
    {
        foreach(var kvp in _clientSceneMap)
        {
            if(kvp.Value != sceneBuildIndex)
            {
                // Debug.Log("Not all clients are in the same scene");
                return false;
            }
        }
        return true;
    }

    public void resetPlayerNumber()
    {
        if (_netManager.IsServer) _playersInGame.Value = 0;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (_netManager != null)
        {
            _netManager.ConnectionApprovalCallback -= ApprovalCheck;
            _netManager.OnServerStarted -= OnServerStarted;
            _netManager.OnClientConnectedCallback -= OnClientConnect;
            _netManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
}
