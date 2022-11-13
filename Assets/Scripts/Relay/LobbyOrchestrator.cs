using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyOrchestrator : NetworkBehaviour
{
    private NetworkManager _netManager => NetworkManager.Singleton;

    [SerializeField] private MainLobbyScreen _mainLobbyScreen;
    [SerializeField] private CreateLobbyScreen _createScreen;
    [SerializeField] private RoomScreen _roomScreen;

    void Start()
    {
        _mainLobbyScreen.Init();
        _createScreen.Init();
        _roomScreen.Init();

        _mainLobbyScreen.gameObject.SetActive(true);
        _createScreen.gameObject.SetActive(false);
        _roomScreen.gameObject.SetActive(false);

        CreateLobbyScreen.LobbyCreated += CreateLobby;
        UI_LobbyRoomPanel.LobbySelected += OnLobbySelected;
        RoomScreen.LobbyLeft += OnLobbyLeft;
        RoomScreen.StartPressed += OnGameStart;

        NetworkObject.DestroyWithScene = true;
    }

    #region Main Lobby

    private async void OnLobbySelected(Lobby lobby)
    {
        using (new Load("Joining Lobby..."))
        {
            try
            {
                await MatchmakingService.JoinLobbyWithAllocation(lobby.Id);

                _mainLobbyScreen.gameObject.SetActive(false);
                _roomScreen.gameObject.SetActive(true);

                GameNetPortal.Instance.ConnectClient();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UI_Utilities.Instance.LogError("Failed joining lobby");
            }
        }
    }
    #endregion

    #region Create

    private async void CreateLobby(LobbyData data)
    {
        using (new Load("Creating Lobby..."))
        {
            try
            {
                await MatchmakingService.CreateLobbyWithAllocation(data);

                _createScreen.gameObject.SetActive(false);
                _roomScreen.gameObject.SetActive(true);

                // Starting the host immediately will keep the relay server alive
                _netManager.StartHost();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UI_Utilities.Instance.LogError("Failed creating lobby");
            }
        }
    }
    #endregion

    #region Room

    private readonly Dictionary<ulong, bool> _playersInLobby = new();
    public static event Action<Dictionary<ulong, bool>> LobbyPlayersUpdated;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _netManager.OnClientConnectedCallback += OnClientConnectedCallback;
            _playersInLobby.Add(_netManager.LocalClientId, false);
            UpdateInterface();
        }

        // Client uses this in case host destroys the lobby
        _netManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientConnectedCallback(ulong playerId)
    {
        if (!IsServer) return;

        // Add locally (Server)
        if (!_playersInLobby.ContainsKey(playerId)) _playersInLobby.Add(playerId, false);

        PropagateToClients();

        UpdateInterface();
    }

    private void PropagateToClients()
    {
        foreach (var player in _playersInLobby) UpdatePlayerClientRpc(player.Key, player.Value);
    }

    [ClientRpc]
    private void UpdatePlayerClientRpc(ulong clientId, bool isReady)
    {
        if (IsServer) return;

        if (!_playersInLobby.ContainsKey(clientId)) _playersInLobby.Add(clientId, isReady);
        else _playersInLobby[clientId] = isReady;
        UpdateInterface();
    }

    private void OnClientDisconnectCallback(ulong playerId)
    {
        if (IsServer)
        {
            // Handle locally
            if (_playersInLobby.ContainsKey(playerId)) _playersInLobby.Remove(playerId);

            // Propagate all clients
            RemovePlayerClientRpc(playerId);

            UpdateInterface();
        }
        else
        {
            // This happens when the host disconnects the lobby
            _roomScreen.gameObject.SetActive(false);
            _mainLobbyScreen.gameObject.SetActive(true);
            OnLobbyLeft();
        }
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(ulong clientId)
    {
        if (IsServer) return;

        if (_playersInLobby.ContainsKey(clientId)) _playersInLobby.Remove(clientId);
        UpdateInterface();
    }

    public void OnReadyClicked()
    {
        SetReadyServerRpc(_netManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ulong playerId)
    {
        _playersInLobby[playerId] = true;
        PropagateToClients();
        UpdateInterface();
    }

    private void UpdateInterface()
    {
        LobbyPlayersUpdated?.Invoke(_playersInLobby);
    }

    private async void OnLobbyLeft()
    {
        using (new Load("Leaving Lobby..."))
        {
            _playersInLobby.Clear();
            _netManager.Shutdown();
            await MatchmakingService.LeaveLobby();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        CreateLobbyScreen.LobbyCreated -= CreateLobby;
        UI_LobbyRoomPanel.LobbySelected -= OnLobbySelected;
        RoomScreen.LobbyLeft -= OnLobbyLeft;
        RoomScreen.StartPressed -= OnGameStart;

        // We only care about this during lobby
        if (_netManager != null)
        {
            _netManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
    }

    private async void OnGameStart()
    {
        using (new Load("Starting the game..."))
        {
            await MatchmakingService.LockLobby();
            _netManager.SceneManager.LoadScene("GamePlay", LoadSceneMode.Single);
        }
    }
    #endregion
}
