using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDB : Singleton<CardDB>
{
    private void Awake()
    {
        InitializeDB();
        DontDestroyOnLoad(this);
    }

    private Dictionary<int, ServerCardBaseData> _cardDataDB = new Dictionary<int, ServerCardBaseData>();
    public int GetCardsNum() { return _cardDataDB.Count; }
    public ServerCardBaseData GetCardData(int cardId) { return _cardDataDB[cardId]; }

    public void InitializeDB()
    {
        // Only allowing to create UnitBaseData on this class
        // -> Solved make UnitBaseData nested on CarDB class

        for(int i = 0; i < 50; i++)
        {
            _cardDataDB.Add(i, new ServerUnitBaseData(
                cardId : i, 
                cardName: $"Name_{i}", 
                cost: Random.Range(1, 10), 
                attack : Random.Range(1, 10), 
                hp : Random.Range(1, 10), 
                agility : Random.Range(1, 4), 
                unitPrefabPath: "Units/Unit001"));
        }
    }
}
