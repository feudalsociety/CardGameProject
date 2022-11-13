using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckListPanel : MonoBehaviour, IDropHandler
{
    [SerializeField] private CardIconSlot _cardIconSlot;

    public void OnDrop(PointerEventData eventData)
    {
        UI_CardDisplay cardDisplay = eventData.pointerDrag.GetComponent<UI_CardDisplay>();

        if (cardDisplay != null && eventData.pointerDrag.GetComponent<Button>().interactable != false)
        {
            _cardIconSlot.AddCardIcon(cardDisplay.CardId);
        }
    }
}
