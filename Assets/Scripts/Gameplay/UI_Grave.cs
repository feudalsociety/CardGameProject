using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Grave : UI_Base
{
    List<ClientCardBase> _clientCards = new List<ClientCardBase>();
    TMP_Text _cardCount;

    enum Texts
    {
        Label,
        CardCount
    }

    private void Awake()
    {
        Bind<TMP_Text>(typeof(Texts));
        _cardCount = Get<TMP_Text>((int)Texts.CardCount);
    }

    public override void Init() { }

    // TODO : This needs to be checked by Server
    public void AddCard(ClientCardBase card)
    {
        _clientCards.Add(card);
        card.transform.SetParent(transform);
        UpdateCardCount();
    }

    void UpdateCardCount()
    {
        _cardCount.text = _clientCards.Count.ToString();
    }
}
