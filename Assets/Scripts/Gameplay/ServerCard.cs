using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all the information about the card
/// </summary>
public class ServerCard 
{
    private static int _uid = 0;
    private int _cardUid = -1;

    public int NewCardUid()
    {
        _cardUid = _uid++;
        return _cardUid;
    }

    public int CardUid => _cardUid;
    private ServerCardBaseData _cardData; public ServerCardBaseData CardData => _cardData;

    // TODO : original owner

    public ServerCard(ServerCardBaseData cardData)
    {
        _cardUid = NewCardUid();
        _cardData = cardData;
    }
}
