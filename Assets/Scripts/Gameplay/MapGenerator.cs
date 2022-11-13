using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Linq;

public class MapGenerator : NetworkSingleton<MapGenerator>
{
    private NetworkManager _netManager => NetworkManager.Singleton;

    [SerializeField] private Tile _tilePrefab;

    public static readonly float TileSize = 100.0f;
    public static readonly float TileHeight = 50.0f;

    [SerializeField] private int _mapSize = 7;

    // responsible for important game logic on the server side
    private Dictionary<ICoords, Tile> _serverTiles;
    // have reference only for the visual parts on the clients
    private Dictionary<ICoords, Tile> _clientTiles;

    public Dictionary<ICoords, Tile> Tiles
    {
        get
        {
            if(IsServer) return _serverTiles;
            else return _clientTiles;
        }
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer) _serverTiles = new Dictionary<ICoords, Tile>();
        else if(!IsServer && IsClient) _clientTiles = new Dictionary<ICoords, Tile>();
        base.OnNetworkSpawn();
    }

    [ServerRpc]
    public void GenerateMapServerRpc()
    {
        // Allocate RowLength
        int[] _rowLengths = new int[_mapSize * 2 - 1];
        for (int i = 0; i < _mapSize * 2 - 1; i++)
        {
            _rowLengths[i] = 2 * _mapSize - 1 - Mathf.Abs(i - _mapSize + 1);
        }

        // Spawn Tiles
        for (int i = 0; i < _rowLengths.Length; i++)
        {
            for (int j = 0; j < _rowLengths[i]; j++)
            {
                Tile newTile = Instantiate(_tilePrefab,
                new Vector3(j * (TileSize * Mathf.Sqrt(3)) + (Mathf.Abs(i - _rowLengths.Length / 2) - _rowLengths.Length / 2) * (TileSize * 0.5f * Mathf.Sqrt(3)),
                            -TileHeight * 0.5f,
                             i * TileSize * 1.5f),
                             Quaternion.identity);

                int x = j + (i < _mapSize ? -i : - (_mapSize - 1));
                int y = i;

                var coord = new HexCoords((short)x, (short)y);
                newTile.GetComponent<NetworkObject>().Spawn();
                newTile.InitServerRpc(true, coord);

                _serverTiles.Add(coord, newTile);
                AddToClientTilesClientRpc(coord, newTile);
            }
        }

        foreach (var tile in _serverTiles.Values)
        {
            tile.transform.parent = transform;
            tile.Neighbors = _serverTiles.Where(t => tile.Coord.GetDistance(t.Value.Coord) == 1).Select(t => t.Value).ToList();
        }

        UI_Utilities.Instance.Log("Map generation completed");
    }

    [ClientRpc]
    private void AddToClientTilesClientRpc(HexCoords coord, NetworkBehaviourReference obj)
    {
        if(!IsServer && IsClient)
        {
            if (obj.TryGet(out Tile tile))
            {
                _clientTiles.Add(coord, tile);
            }
        }
    }

    public Vector3 GetCenterPosition()
    {
        return new Vector3((_mapSize * 2 - 2) * TileSize * Mathf.Sqrt(3) * 0.25f,
                            -TileHeight * 0.5f,
                            (_mapSize * 2 - 2) * 1.5f * TileSize * 0.5f);
    }

    static public Vector3 CoordsToWorldPos(HexCoords coords) => TileSize * coords.GetPos();
}
