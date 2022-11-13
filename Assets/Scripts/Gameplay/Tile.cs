using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using System;
using TMPro;

public class Tile : NetworkBehaviour
{
    private NetworkManager _netManager => NetworkManager.Singleton;
    public float GetDistance(Tile other) => Coord.GetDistance(other.Coord);

    private NetworkVariable<HexCoords> _coords = new NetworkVariable<HexCoords>();
    // This implies if unit is placed on this tile (check if NetworkObjectId value was setted)
    private NetworkVariable<bool> _walkable = new NetworkVariable<bool>(true);
    // unit�� ��� �� �� ���� ��찡 ���� ���� �����Ƿ� �и���
    private NetworkVariable<ulong> _unitNetId = new NetworkVariable<ulong>(0);
    public HexCoords Coord => _coords.Value;
    public bool Walkable => _walkable.Value;
    // TODO : ���� Walkable�� true�� �� ������ ������ ����Ǿ� �ִ�.
    public ulong? UnitNetId => !Walkable ? _unitNetId.Value : null;

    public Unit GetUnitOnTile()
    {
        if (!UnitNetId.HasValue) return null;
        return UnitManager.Instance.SpawnedUnits[UnitNetId.Value];
    }

    // CacheNeighbors�� server������ Ȱ��, client�� ������� �����ϴ� �������.
    // Path finding logic�� ���̴� data��
    // TODO : Logic�� Cloud code�� �ø��� �ٷ� �����͸� client���� �޾ƿ��� ���ŷӰ�
    // server�� ��ġ�� �ʰ� �ٷ� ����� �� �ֵ��� �ٲ� �� ���� ������?
    public List<Tile> Neighbors { get; set; }
    public Tile Connection { get; private set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;
    public void SetConnection(Tile node) => Connection = node;
    public void SetG(float g) => G = g;
    public void SetH(float h) => H = h;

    [SerializeField] TMP_Text _coordText;
    [SerializeField] Material _normalMat;
    [SerializeField] Material _walkableMat;

    public override int GetHashCode() => Coord.GetHashCode();
    public override bool Equals(object other) => Coord == (other as Tile).Coord;

    [ClientRpc]
    public void ShowWalkableClientRpc(ClientRpcParams clientRpcParams = default) => GetComponent<Renderer>().material = _walkableMat;
    [ClientRpc]
    public void RevertToNormalClientRpc(ClientRpcParams clientRpcParams = default)  => GetComponent<Renderer>().material = _normalMat;

    [ServerRpc]
    public void InitServerRpc(bool walkable, HexCoords coord)
    {
        _walkable.Value = walkable;
        _coords.Value = coord;
        SetTileNameAndVisualClientRpc(coord);
    }

    // TODO : Use NetworkBehaviourReference
    [ServerRpc]
    public void PlaceUnitServerRpc(ulong unitNetId) 
    {
        _unitNetId.Value = unitNetId;
        _walkable.Value = false;
    }

    [ServerRpc]
    public void RemoveUnitServerRpc()
    {
        _unitNetId.Value = 0;
        _walkable.Value = true;
    }

    [ClientRpc]
    private void SetTileNameAndVisualClientRpc(HexCoords coord)
    {
        gameObject.name = coord.ToString();
        _coordText.text = coord.ToString();
    }
}

public interface ICoords
{
    public float GetDistance(ICoords other);
    public Vector3 GetPos();
}

[System.Serializable]
public struct HexCoords : INetworkSerializable, IEquatable<HexCoords>, ICoords
{
    private short _x;
    private short _y;

    public HexCoords(short x, short y)
    {
        _x = x;
        _y = y;
    }

    public float GetDistance(ICoords other) => (this - (HexCoords)other).AxialLength();

    private static readonly float Sqrt3 = Mathf.Sqrt(3);

    public Vector3 GetPos()
    {
        return _x * new Vector3(Sqrt3, 0, 0)
            + _y * new Vector3(Sqrt3 / 2, 0, 1.5f);
    }

    // odd-r horizontal layout
    private int AxialLength()
    {
        if (Mathf.Sign(_x) == Mathf.Sign(_y)) return Mathf.Abs(_x + _y);
        else return Mathf.Max(Mathf.Abs(_x), Mathf.Abs(_y));
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _x);
        serializer.SerializeValue(ref _y);
    }
    public override string ToString() => $"[{_x},{_y}]";

    public override int GetHashCode()
    {
        return HashCode.Combine(_x, _y);
    }

    public bool Equals(HexCoords other)
    {
        return _x == other._x && _y == other._y;
    }

    public override bool Equals(object obj)
    {
        // Check for null and compare run-time types
        if ((obj == null) || !this.GetType().Equals(obj.GetType())) return false;
        else return Equals((HexCoords)obj);
    }

    public static HexCoords operator -(HexCoords a, HexCoords b)
    {
        return new HexCoords((short)(a._x - b._x), (short)(a._y - b._y));
    }

    public static bool operator ==(HexCoords a, HexCoords b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(HexCoords a, HexCoords b) => !(a == b);
}
