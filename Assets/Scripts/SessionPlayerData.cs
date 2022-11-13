using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SessionPlayerData : ISessionPlayerData
{
    public string PlayerName;
    public int PlayerNumber;
    public bool HasPlayerSpawned;

    public SessionPlayerData(ulong clientID, string name, bool isConnected = false, bool hasPlayerSpawned = false)
    {
        ClientID = clientID;
        PlayerName = name;
        PlayerNumber = -1;

        IsConnected = isConnected;
        HasPlayerSpawned = hasPlayerSpawned;
    }

    public bool IsConnected { get; set; }
    public ulong ClientID { get; set; }

    public void Reinitialize()
    {
        HasPlayerSpawned = false;
    }
}
