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

    // �߻� Ŭ���� ����
    public override void Init()
    {
        if (_deckBuilderScene == null) { _deckBuilderScene = SceneLoadManager.Instance.CurrentScene as DeckBuilderScene; }

        Bind<TMP_Text>(typeof(Texts));

        // Event �߰�
        gameObject.AddUIEvent(SelectCardIcon, Define.UIEvent.Click);
        gameObject.AddUIEvent(BeginDragCardIcon, Define.UIEvent.BeginDrag);
        gameObject.AddUIEvent(EndDragCardIcon, Define.UIEvent.EndDrag);
    }

    // ������ ���� ������ bind�� ���� ���������� �̷�������Ѵ�.
    // Start�Լ��� override�ؼ� init�� ������ ȣ���ϵ��� ��
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
