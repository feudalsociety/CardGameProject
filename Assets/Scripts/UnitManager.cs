using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

// The default behavior is that an object is owned by the server.
public class UnitManager : NetworkSingleton<UnitManager>
{
    private NetworkManager _netManager => NetworkManager.Singleton;
    // NetworkObjectId(key)
    private Dictionary<ulong, Unit> _serverSpawnedUnits;
    private Dictionary<ulong, Unit> _clientSpawnedUnits;

    public Dictionary<ulong, Unit> SpawnedUnits
    {
        get
        {
            if(IsServer) return _serverSpawnedUnits;
            else return _clientSpawnedUnits;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _serverSpawnedUnits = new Dictionary<ulong, Unit>();
        }
        else if(!IsServer && IsClient)
        {
            _clientSpawnedUnits = new Dictionary<ulong, Unit>();
        }
        base.OnNetworkSpawn();
    }

    // TODO : Object pooling unit prefab

    // can only be called by the server
    public void SpawnUnit(int playerNumber, ServerCard serverCard, HexCoords coord)
    {
        var prefabPath = (serverCard.CardData as ServerUnitBaseData).UnitPrefabPath;
        GameObject go = Managers.Resource.Load<GameObject>("Prefabs/" + prefabPath);

        var tile = MapGenerator.Instance.Tiles[coord];

        Vector3 spawnPos = MapGenerator.CoordsToWorldPos(coord);
        Quaternion spawnRot =
            Quaternion.Euler(0f, GameNetPortal.Instance.GetPlayerSpawnPos(playerNumber).Rotation.y, 0f);

        Unit unit = Instantiate(go, spawnPos, spawnRot).GetComponent<Unit>();
        // By default a newly spawned network prefab instance is owned by the server
        unit.GetComponent<NetworkObject>().Spawn();

        _serverSpawnedUnits.Add(unit.NetworkObjectId, unit);
        SpawnUnitClientRpc(unit);
        unit.Init(coord, playerNumber, serverCard);

        tile.PlaceUnitServerRpc(unit.NetworkObjectId);

        UI_Utilities.Instance.LogClientRpc("Placed a unit on " + tile.Coord);
    }

    [ClientRpc]
    private void SpawnUnitClientRpc(NetworkBehaviourReference obj)
    {
        if(!IsServer && IsClient)
        {
            if (obj.TryGet(out Unit unit)) _clientSpawnedUnits.Add(unit.NetworkObjectId, unit);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveUnitServerRpc(HexCoords startCoord, HexCoords targetCoord, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        (int playerNumber, ClientRpcParams clientRpcParams) t = GameMananger.Instance.GetPlayerNumberAndClientRpcParam(clientId);
       
        if (t.playerNumber == -1) return;

        if (!TurnManager.Instance.CheckCurrentTurn(t.playerNumber))
        {
            UI_Utilities.Instance.LogErrorClientRpc($"It's not your turn", t.clientRpcParams);
            return;
        }

        try
        {
            var unitNetIdOnTile = MapGenerator.Instance.Tiles[startCoord].UnitNetId;
            if (!unitNetIdOnTile.HasValue) throw new Exception($"There is no spawned unit in this tile {startCoord}");
            var unit = _serverSpawnedUnits[unitNetIdOnTile.Value];
            if (unit.OwnerPlayerNumber == t.playerNumber) unit.Move(targetCoord, clientId);
            else throw new Exception($"You don't own this unit, move request denied");
        }
        catch (Exception ex)
        {
            UI_Utilities.Instance.LogErrorClientRpc($"RequestMoveUnitServerRpc error : {ex.Message}", t.clientRpcParams);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestShowWakableTilesServerRpc(HexCoords coord, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (!_netManager.ConnectedClients.ContainsKey(clientId)) return;

        var unitNetId = MapGenerator.Instance.Tiles[coord].UnitNetId;
        if (unitNetId.HasValue)
        {
            var unit = _serverSpawnedUnits[unitNetId.Value];
            if (unit.State != Define.UnitState.Idle) return;

            var cardData = unit.ServerCard.CardData;
            // var cardData = _serverUnitDatas[unit.ServerCardUid].CardData;
            var mobility = (cardData as ServerUnitBaseData).Agility;
            unit.ShowWalkableTilesServerRpc(clientId, mobility);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestHideWalkableTilesServerRpc(HexCoords coord, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (!_netManager.ConnectedClients.ContainsKey(clientId)) return;

        var unitNetId = MapGenerator.Instance.Tiles[coord].UnitNetId;
        if (unitNetId.HasValue)
            SpawnedUnits[unitNetId.Value].HideWalkableTilesServerRpc(clientId);
    }

    // NetworkBehaviour.IsSpawned is false
    // do not expect netcode distinguishing properties (like IsClient, IsServer, IsHost, etc)
    // to be accurate while within the those two methods (Awake and Start).
}
