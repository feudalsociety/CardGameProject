using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_DeckPanel : UI_Popup
{
    static public float FadeDuration = 0.13f;

    private List<UI_PCardDisplay> _cardDisplayList = new List<UI_PCardDisplay>();
    private UI_Deck _deck;

    enum Buttons
    {
        BackImage,
        Return
    }

    enum Texts
    {
        Return
    }

    enum Images
    {
        Scroll,
        View
    }

    enum GameObjects
    {
        Content,
        Scrollbar,
        SlidingArea,
        Handle
    }

    public override void Init()
    {
        Managers.UI.SetCanvus(gameObject, true, RenderMode.ScreenSpaceOverlay);

        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));

        GetButton((int)Buttons.BackImage).gameObject.AddUIEvent(CloseDeckPanelByBackImage);
        GetButton((int)Buttons.Return).gameObject.AddUIEvent(CloseDeckPanelByButton);

        GridLayoutGroup _layoutGroup = GetObject((int)GameObjects.Content).GetComponent<GridLayoutGroup>();


        // TODO : Reference ¼öÁ¤
        _deck = FindObjectOfType<UI_Deck>();
        _deck.OnCardLeave += RemoveCardDisplay;

        var deckList = _deck.DeckList;

        for (int i = 0; i < deckList.Count; i++)
        {
            GameObject item = Managers.UI.MakeSubItem<UI_PCardDisplay>(parent: _layoutGroup.transform).gameObject;
            item.name = $"ID[{CardDB.Instance.GetCardData(i).CardId}]";
            UI_PCardDisplay cardDisplay = item.GetOrAddComponent<UI_PCardDisplay>();
            cardDisplay.Init();
            cardDisplay.SetPCardDisplayData(CardDB.Instance.GetCardData(deckList[i]));

            _cardDisplayList.Add(cardDisplay);
        }

        _layoutGroup.gameObject.GetOrAddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    // Remove & Add cardDisplay 
    void RemoveCardDisplay(int cardId)
    {
        var cardDisplay = _cardDisplayList.Find(cardDisplay => cardDisplay.CardId == cardId);
        _cardDisplayList.Remove(cardDisplay);
        Managers.Resource.Destroy(cardDisplay.gameObject);
    }

    void CloseDeckPanelByButton(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            ClosePopupUI();
        }
    }

    void CloseDeckPanelByBackImage(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Right)
        {
            ClosePopupUI();
        }
    }

    public override void ClosePopupUI()
    {
        _deck.OnCardLeave -= RemoveCardDisplay;
        MyUIController.Instance.CameraControl = true;
        (SceneLoadManager.Instance.CurrentScene as GamePlayScene).Dof.SetActive(false);
        gameObject.GetComponent<UIFadeScript>().FadeOut(fadeOutDuration: FadeDuration, () => { base.ClosePopupUI(); });
    }
}
