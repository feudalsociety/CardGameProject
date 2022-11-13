using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardIconSlot : MonoBehaviour
{
    [SerializeField] private CardDisplayPanel _cardDisplayPanel;
    private DeckBuilderScene _deckBuilderScene = null;
    [SerializeField] private UI_DeckBuilder _deckBuilderUI;

    // 외부에서 수정가능
    public SortedDictionary<int, UI_CardIcon> Slots { get; set; } = new SortedDictionary<int, UI_CardIcon>();

    private void Awake()
    {
        _deckBuilderScene = SceneLoadManager.Instance.CurrentScene as DeckBuilderScene;
    }

    private void SortChildren()
    {
        int index = 0;
        foreach (var child in Slots)
        {
            child.Value.transform.SetSiblingIndex(index);
            index++;
        }
    }

    public void AddCardIcon(int id)
    {
        // check if cardId is in CardDB
        if(id < 0 || id > CardDB.Instance.GetCardsNum() - 1)
        {
            Debug.Log("Error : id[" + id + "] data doesn't exist in DB");
        }

        if(_deckBuilderUI.AddtoDeckList(id))
        {
            UI_CardIcon cardIcon = Managers.UI.MakeSubItem<UI_CardIcon>(parent: gameObject.transform);
            ServerCardBaseData cardData = CardDB.Instance.GetCardData(id);

            // 먼저 bind와 event 추가
            cardIcon.Init();
            cardIcon.SetCardIconData(id, cardData.CardName);

            Slots.Add(cardIcon.IconCardId, cardIcon);
            cardIcon.name = "ID[" + cardIcon.IconCardId + "]";

            // Sort in Hierarchy
            SortChildren();

            _deckBuilderScene.ShowHoveringCard(false);
            _deckBuilderUI.getCardDisplay(id).DeactivateDisplay();
            _deckBuilderUI.ShowIfDecklistChanged();
        }
    }

    public void RemoveCardIcon(int id)
    {
        if(_deckBuilderUI.RemoveFromDeckList(id))
        {
            UI_CardIcon cardIcon = Slots[id];
            Slots.Remove(id);
            // TODO : Pooling
            Destroy(cardIcon.gameObject);

            _deckBuilderScene.ShowHoveringCard(false);
            _deckBuilderUI.getCardDisplay(id).ActivateDisplay();
            _deckBuilderUI.ShowIfDecklistChanged();
        }
    }
}
