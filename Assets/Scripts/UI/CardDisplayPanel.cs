using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDisplayPanel : MonoBehaviour, IDropHandler
{
    [SerializeField] private CardIconSlot _cardIconSlot;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.GetComponent<UI_CardIcon>() != null)
        {
            UI_CardIcon cardIcon = eventData.pointerDrag.GetComponent<UI_CardIcon>();
            _cardIconSlot.RemoveCardIcon(cardIcon.IconCardId);
        }
    }
}
