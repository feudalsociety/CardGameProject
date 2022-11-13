using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UI_CardIcon : UI_Base
{
    private static DeckBuilderScene _deckBuilderScene;
    public int IconCardId { get; private set; } = -1;
    public string CardIconName { get; private set; } = null;

    enum Texts
    {
        CardInfo
    }

    private void Start()
    {

    }

    // 추상 클래스 구현
    public override void Init()
    {
        if (_deckBuilderScene == null) { _deckBuilderScene = SceneLoadManager.Instance.CurrentScene as DeckBuilderScene; }

        Bind<TMP_Text>(typeof(Texts));

        // Event 추가
        gameObject.AddUIEvent(SelectCardIcon, Define.UIEvent.Click);
        gameObject.AddUIEvent(BeginDragCardIcon, Define.UIEvent.BeginDrag);
        gameObject.AddUIEvent(EndDragCardIcon, Define.UIEvent.EndDrag);
    }

    // 데이터 설정 이전에 bind가 먼저 성공적으로 이루어져야한다.
    // Start함수를 override해서 init을 별도로 호출하도록 함
    public void SetCardIconData(int cardId, string cardName)
    {
        TMP_Text dataText = Get<TMP_Text>((int)Texts.CardInfo);
        IconCardId = cardId;
        CardIconName = cardName;
        dataText.text = "[" + IconCardId.ToString() + "] " + CardIconName;
    }

    void SelectCardIcon(PointerEventData data)
    {
        gameObject.GetComponentInParent<CardIconSlot>().RemoveCardIcon(IconCardId);
    }

    void BeginDragCardIcon(PointerEventData data)
    {
        _deckBuilderScene.SetHoveringCardTransform(gameObject.transform.position);
        _deckBuilderScene.ShowHoveringCard(true);
    }

    void EndDragCardIcon(PointerEventData data)
    {
        _deckBuilderScene.ShowHoveringCard(false);
    }


}
