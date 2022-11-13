using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Linq;

// TODO : UI_Deck과 NetworkObject인 Deck과 분리
public class UI_Deck : UI_Base
{
    private List<int> _deckList = new List<int>();
    List<ClientCardBase> _clientCards = new List<ClientCardBase>();
    public Action<int> OnCardEnter = null;
    public Action<int> OnCardLeave = null;

    public List<int> DeckList => _deckList;

    enum Texts
    {
        Label,
        CardCount
    }

    private void Start() { }

    // 바로 아래 method Generate Deck에서 호출됨
    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
    }

    public void InitializeDeckList(int[] deckListArray)
    {
        // Instaniate의 Update count에서 bind가 되지 않으면 error이므로 bind가 먼저 일어남을 보장
        Init();
        _deckList = deckListArray.ToList();

        // this is for deckpanel showing random order
        _deckList = ShuffleDeckList(_deckList);

        gameObject.AddUIEvent(OpenDeckPanel);
        UpdateCardCount(_clientCards.Count);
    }

    public void InstantiateClientUnitCard(ClientUnitBaseData unitBaseData)
    {
        var card = Managers.Resource.Instantiate("Cards/UnitCard", transform).GetComponent<ClientUnitCard>();
        card.Init(unitBaseData);
        card.gameObject.SetActive(false);
        _clientCards.Add(card);
    }

    public ClientCardBase RemoveCardFromDeck()
    {
        if (_clientCards.Count <= 0)
            throw new Exception("No more clientCard left in my deck");

        var clientCard = _clientCards[_clientCards.Count - 1];
        _clientCards.RemoveAt(_clientCards.Count - 1);
        _deckList.Remove(_deckList.Find(id => id == clientCard.CardId));
        OnCardLeave?.Invoke(clientCard.CardId);

        UpdateCardCount(_clientCards.Count);
        return clientCard;
    }

    public void DrawCardFromDeck()
    {
        try
        {
            ClientCardBase clientCard = RemoveCardFromDeck();
            MyHandManager.Instance.AddCardToHand(clientCard);
        }
        catch (Exception ex)
        {
            UI_Utilities.Instance.Log($"DrawCardFromDeck error : {ex.Message}");
        }

        MyHandManager.Instance.CalculateHandTransform();
        MyHandManager.Instance.DrawAnimation();
    }

    private List<int> ShuffleDeckList(List<int> _deckList)
    {
        for (int i = _deckList.Count - 1; i > 0; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i);

            int temp = _deckList[i];
            _deckList[i] = _deckList[rnd];
            _deckList[rnd] = temp;
        }

        return _deckList;
    }

    private void OpenDeckPanel(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
        {
            MyUIController.Instance.CameraControl = false;
            (SceneLoadManager.Instance.CurrentScene as GamePlayScene).Dof.SetActive(true);
            UI_DeckPanel deckPanel = Managers.UI.ShowPopupUI<UI_DeckPanel>("UI_DeckPanel");
            deckPanel.gameObject.GetComponent<UIFadeScript>().FadeIn(fadeInDuration: UI_DeckPanel.FadeDuration);
        }
    }

    public void UpdateCardCount(int cardCount)
    {
        var cardCountText = Get<TMP_Text>((int)Texts.CardCount);
        cardCountText.text = cardCount.ToString();
    }
}
