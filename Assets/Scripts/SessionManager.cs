using System.Collections.Generic;
using UnityEngine;

public interface ISessionPlayerData
{
    bool IsConnected { get; set; }
    ulong ClientID { get; set; }
    void Reinitialize();
}

public class SessionManager<T> where T : struct, ISessionPlayerData
{
    // Maps a given client player id to the data for a given client player.
    Dictionary<string, T> _clientData;

    // Map to allow us to cheaply map from player id to player data
    Dictionary<ulong, string> _clientIDToPlayerId;

    static SessionManager<T> _instance;
    public static SessionManager<T> Instance => _instance ??= new SessionManager<T>();

    bool _hasSessionStarted;

    SessionManager()
    {
        _clientData = new Dictionary<string, T>();
        _clientIDToPlayerId = new Dictionary<ulong, string>();
    }

    // Handles client disconnect
    public void DisconnectClient(ulong clientId)
    {
        if (!_hasSessionStarted)
        {
            // Mark client as disconnected, but keep their data so they can reconnect
            if (_clientIDToPlayerId.TryGetValue(clientId, out var playerId))
            {
                if (GetPlayerData(playerId)?.ClientID == clientId)
                {
                    var clientData = _clientData[playerId];
                    clientData.IsConnected = false;
                    _clientData[playerId] = clientData;
                }
            }
        }
        else
        {
            // Session has not started, no need to keep their data
            if (_clientIDToPlayerId.TryGetValue(clientId, out var playerId))
            {
                _clientIDToPlayerId.Remove(clientId);
                if (GetPlayerData(playerId)?.ClientID == clientId)
                {
                    _clientData.Remove(playerId);
                }
            }
        }
    }

    // playerId : This is the playerId that is unique to this client
    // and persists across multiple logins from the same client
    // returns True if a player with this ID is already connected.
    public bool IsDuplicateConnection(string playerId)
    {
        var isDuplicate = _clientData.ContainsKey(playerId) && _clientData[playerId].IsConnected;
        if (isDuplicate) UI_Utilities.Instance.LogError("This is Duplicate Connection");
        return isDuplicate;
    }

    // Adds a connecting player's session data if it is a new connection,
    // or updates their session data in case of a reconnection
    // clientId : Netcode assigned us on login. It does not persist across multiple logins from the same client.
    // playerId : unique to this client and persists across multiple logins from the same client
    // sessionPlayerData : The player's initial data
    public void SetupConnectingPlayerSessionData(ulong clientId, string playerId, T sessionPlayerData)
    {
        var isReconnecting = false;

        // Test for duplicate connection
        if (IsDuplicateConnection(playerId))
        {
            UI_Utilities.Instance.LogError($"Player ID {playerId} already exists. This is a duplicate connection.");
            return;
        }

        // If another client exists with the same playerId
        if (_clientData.ContainsKey(playerId))
        {
            if (!_clientData[playerId].IsConnected)
            {
                // If this connecting client has the same player Id as a disconnected client, this is a reconnection.
                isReconnecting = true;
            }
        }

        // Reconnecting. Give data from old player to new player
        if (isReconnecting)
        {
            // Update player session data
            sessionPlayerData = _clientData[playerId];
            sessionPlayerData.ClientID = clientId;
            sessionPlayerData.IsConnected = true;
        }

        //Populate our dictionaries with the SessionPlayerData
        _clientIDToPlayerId[clientId] = playerId;
        _clientData[playerId] = sessionPlayerData;
    }

    public string GetPlayerId(ulong clientId)
    {
        if (_clientIDToPlayerId.TryGetValue(clientId, out string playerId))
        {
            return playerId;
        }

        UI_Utilities.Instance.Log($"No client player ID found mapped to the given client ID: {clientId}");
        return null;
    }

    // id of the client whose data is requested
    // returns Player data struct matching the given ID
    public T? GetPlayerData(ulong clientId)
    {
        // First see if we have a playerId matching the clientID given.
        var playerId = GetPlayerId(clientId);
        if (!string.IsNullOrEmpty(playerId))
        {
            return GetPlayerData(playerId);
        }

        UI_Utilities.Instance.Log($"No client player ID found mapped to the given client ID: {clientId}");
        return null;
    }

    public T? GetPlayerData(string playerId)
    {
        if (_clientData.TryGetValue(playerId, out T data))
        {
            return data;
        }

        Debug.Log($"No PlayerData of matching player ID found: {playerId}");
        return null;
    }

    // Updates player data
    public void SetPlayerData(ulong clientId, T sessionPlayerData)
    {
        if (_clientIDToPlayerId.TryGetValue(clientId, out string playerId))
        {
            _clientData[playerId] = sessionPlayerData;
        }
        else
        {
            Debug.LogError($"No client player ID found mapped to the given client ID: {clientId}");
        }
    }

    // Onlt can server start the session, still has the connected player data
    // Marks the current session as started, so from now on we keep the data of disconnected players
    public void OnSessionStarted()
    {
        _hasSessionStarted = true;
    }

    // Reinitializes session data from connected players,
    // and clears data from disconnected players,
    // so that if they reconnect in the next game, they will be treated as new players
    public void OnSessionEnded()
    {
        ClearDisconnectedPlayersData();
        ReinitializePlayersData();
        _hasSessionStarted = false;
    }

    public void OnServerEnded()
    {
        _clientData.Clear();
        _clientIDToPlayerId.Clear();
        _hasSessionStarted = false;
    }

    void ReinitializePlayersData()
    {
        foreach (var id in _clientIDToPlayerId.Keys)
        {
            string playerId = _clientIDToPlayerId[id];
            T sessionPlayerData = _clientData[playerId];
            sessionPlayerData.Reinitialize();
            _clientData[playerId] = sessionPlayerData;
        }
    }

    void ClearDisconnectedPlayersData()
    {
        List<ulong> idsToClear = new List<ulong>();
        foreach (var id in _clientIDToPlayerId.Keys)
        {
            var data = GetPlayerData(id);
            if (data is { IsConnected: false })
            {
                idsToClear.Add(id);
            }
        }

        foreach (var id in idsToClear)
        {
            string playerId = _clientIDToPlayerId[id];
            if (GetPlayerData(playerId)?.ClientID == id)
            {
                _clientData.Remove(playerId);
            }

            _clientIDToPlayerId.Remove(id);
        }
    }
}
