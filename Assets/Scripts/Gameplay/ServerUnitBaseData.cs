using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerUnitBaseData : ServerCardBaseData
{
    public int Attack { get; private set; }
    public int Hp { get; private set; }
    public int Agility { get; private set; }

    public string UnitPrefabPath { get; private set; }

    public ServerUnitBaseData(int cardId, string cardName, int cost, int attack, int hp, int agility, string unitPrefabPath)
        : base(cardId, cardName, Define.CardType.Unit, cost)
    {
        Attack = attack;
        Hp = hp;
        Agility = agility;
        UnitPrefabPath = unitPrefabPath;
    }

    public override string ToString()
    {
        return base.ToString()
               + "[Unit Data]\n"
               + $"Attack : {Attack}\n"
               + $"Hp : {Hp}\n"
               + $"Agility : {Agility}";
    }
}
