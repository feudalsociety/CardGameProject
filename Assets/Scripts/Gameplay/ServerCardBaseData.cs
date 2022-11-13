using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Structs don't provide inheritance.
public abstract class ServerCardBaseData
{
    // general card Infos
    public int CardId { get; protected set; }
    public string CardName { get; protected set; }
    public Define.CardType CardType { get; protected set; }

    public int Cost { get; protected set; }

    protected ServerCardBaseData(int cardId, string cardName, Define.CardType cardType, int cost)
    { 
        CardId = cardId; 
        CardName = cardName; 
        Cost = cost; 
        CardType = cardType;
    }

    public override string ToString()
    {
        return "[General Card Data]\n"
             + $"ID : {CardId}\n"
             + $"Name : {CardName}\n"
             + $"Type : {CardType}\n"
             + $"Cost : {Cost}\n";
    }
}
