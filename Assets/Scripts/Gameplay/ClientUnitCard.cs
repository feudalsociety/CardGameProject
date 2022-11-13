using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClientUnitCard : ClientCardBase
{
    public ClientUnitBaseData CardData;
    [SerializeField] private TMP_Text _cardText;

    public override int CardId => CardData.CardId;

    public void Init(ClientUnitBaseData cardData)
    {
        InitTransform();
        CardData = cardData;
        gameObject.name = "ID[" + cardData.CardId.ToString() + "]";
        _cardText.text = cardData.ToString();
    }
}
