using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;

// SessionPlayerData는 server만 참조 가능하지만,
// NetworkPlayerData는 Game에서 모든 Client와 공유하고 싶은 정보들을 가지고 있다.

public class NetworkPlayersData : NetworkBehaviour
{
    public struct NetworkPlayerState : INetworkSerializable, IEquatable<NetworkPlayerState>
    {
        public ulong ClientId;
        public FixedPlayerName PlayerName;
        public int PlayerNumber; // this player's assigned "P#". (0=P1, 1=P2, etc.)

        public NetworkPlayerState(ulong clientId, string name, int playerNumber)
        {
            ClientId = clientId;
            PlayerNumber = playerNumber;
            PlayerName = new FixedPlayerName();
            PlayerName = name;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref PlayerNumber);
        }

        public bool Equals(NetworkPlayerState other)
        {
            return ClientId == other.ClientId &&
                   PlayerName.Equals(other.PlayerName) &&
                   PlayerNumber == other.PlayerNumber;
        }
    }

    public const int MaxPlayerCount = 2;

    // Current state of all players in the lobby
    private NetworkList<NetworkPlayerState> _networkPlayers;
    public NetworkList<NetworkPlayerState> NetworkPlayers => _networkPlayers;

    public int getPlayerNumber(ulong clientId)
    {
        for (int i = 0; i < _networkPlayers.Count; ++i)
        {
            var playerData = _networkPlayers[i];
            if (playerData.ClientId == clientId)
            {
                return playerData.PlayerNumber;
            }
        }
        UI_Utilities.Instance.LogError($"Cannot find data in NetworkPlayersData of clientid : {clientId}");
        return -1;
    }

    private void Awake()
    {
        _networkPlayers = new NetworkList<NetworkPlayerState>();
    }
}

// Wrapping FixedString so that if we want to change player name max size in the future, we only do it once here
public struct FixedPlayerName : INetworkSerializable
{
    FixedString64Bytes _name;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _name);
    }

    public override string ToString()
    {
        return _name.Value.ToString();
    }

    public static implicit operator string(FixedPlayerName s) => s.ToString();
    public static implicit operator FixedPlayerName(string s) => new FixedPlayerName() { _name = new FixedString32Bytes(s) };
}