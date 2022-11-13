using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_CardDisplay : UI_Base
{
    private static DeckBuilderScene deckBuilderScene;
    private static CardIconSlot cardIconslot;

    public int CardId { get; private set; } = -1;
    public string CardName { get; private set; } = null;

    enum Texts
    {
        CardInfo
    }

    // Start override되면서 init을 별도로 호출해줘야한다.
    private void Start()
    {
        
    }

    public override void Init()
    {
        if (deckBuilderScene == null) { deckBuilderScene = SceneLoadManager.Instance.CurrentScene as DeckBuilderScene; }
        if (cardIconslot == null) { cardIconslot = Managers.UI.CurrentSceneUI.gameObject.findChild<CardIconSlot>("CardIconSlot", true); }

        Bind<TMP_Text>(typeof(Texts));
        AddUIEvents();
    }

    public void AddUIEvents()
    {
        gameObject.AddUIEvent(SelectCard, Define.UIEvent.Click);
        gameObject.AddUIEvent(BeginDragCard, Define.UIEvent.BeginDrag);
        gameObject.AddUIEvent(DragCard, Define.UIEvent.Drag);
        gameObject.AddUIEvent(EndDragCard, Define.UIEvent.EndDrag);
    }

    public void RemoveEvents()
    {
        gameObject.RemoveUIEvent(Define.UIEvent.Click);
        gameObject.RemoveUIEvent(Define.UIEvent.BeginDrag);
        gameObject.RemoveUIEvent(Define.UIEvent.Drag);
        gameObject.RemoveUIEvent(Define.UIEvent.EndDrag);
    }

    public void ActivateDisplay()
    {
        gameObject.GetComponent<Button>().interactable = true;
        AddUIEvents();
    }

    public void DeactivateDisplay()
    {
        gameObject.GetComponent<Button>().interactable = false;
        RemoveEvents();
    }

    public void SetCardDisplayData(ServerCardBaseData data)
    {
        // 먼저 bind가 되고 나서 실행되는게 보장되어야함
        CardId = data.CardId;
        CardName = data.CardName;
        TMP_Text dataText = Get<TMP_Text>((int)Texts.CardInfo);
        dataText.text = data.ToString();
    }

    void SelectCard(PointerEventData data)
    {
        cardIconslot.AddCardIcon(CardId);
    }

    void BeginDragCard(PointerEventData data)
    {
        deckBuilderScene.SetHoveringCardTransform(gameObject.transform.position);
        deckBuilderScene.ShowHoveringCard(true);
    }

    void DragCard(PointerEventData data)
    {
    }

    void EndDragCard(PointerEventData data)
    {
        deckBuilderScene.ShowHoveringCard(false);
    }
}
