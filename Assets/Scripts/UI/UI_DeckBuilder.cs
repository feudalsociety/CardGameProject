using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using DG.Tweening;


public class UI_DeckBuilder : UI_Scene
{
    private string _sharedDeckListPath;

    List<int> _deckList = new List<int>();
    List<int> _originalDeckList = new List<int>();

    private List<UI_CardDisplay> _cardDisplayList = new List<UI_CardDisplay>();

    public UI_CardDisplay getCardDisplay(int id) { return _cardDisplayList[id]; }

    enum Buttons
    {
        Save,
        Exit
    }

    enum Images
    {
        Back,
        View,
        DeckListPanel,
        CardDisplayPanel,
    }
    enum Texts
    {
        Save,
        Exit
    }

    // 모든 enum을 GameObject로 합쳐도 됨
    enum GameObjects
    {
        CardIconSlot,
        CardDBScroll,
        Content,
        Scrollbar,
        SlidingArea,
        Handle,
        HoverPanel
    }

    private void Start()
    {
        _sharedDeckListPath = Path.Combine(Application.dataPath, "../../SharedDeckList/sharedDeckList.json");
        Init();
    }

    public override void Init()
    {
        Managers.UI.SetCanvus(gameObject, false, RenderMode.ScreenSpaceCamera, Camera.main);

        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));

        // Event 추가
        GetButton((int)Buttons.Exit).gameObject.AddUIEvent(GoBackToMainMenu);
        GetButton((int)Buttons.Save).gameObject.AddUIEvent(SaveDeckToJson);

        GridLayoutGroup _deckListContent = Get<GameObject>((int)GameObjects.CardIconSlot).GetComponent<GridLayoutGroup>();
        GridLayoutGroup _dbContent = Get<GameObject>((int)GameObjects.Content).GetComponent<GridLayoutGroup>();

        int cardNum = CardDB.Instance.GetCardsNum();

        for (int i = 0; i < cardNum; i++)
        {
            GameObject item = Managers.UI.MakeSubItem<UI_CardDisplay>(parent: _dbContent.transform).gameObject;
            var cardData = CardDB.Instance.GetCardData(i);
            item.name = $"ID[{cardData.CardId}]";
            UI_CardDisplay cardDisplay = item.GetOrAddComponent<UI_CardDisplay>();
            cardDisplay.Init();
            cardDisplay.SetCardDisplayData(cardData);

            // Only Allowing UI_Deckbiuilder change CardDisplayList
            _cardDisplayList.Add(cardDisplay);
        }

        LoadDeckListFromJson();
        CardIconSlot cardIconSlot = GetObject((int)GameObjects.CardIconSlot).GetComponent<CardIconSlot>();
        for (int i = 0; i < _originalDeckList.Count; i++)
        {
            _cardDisplayList[_originalDeckList[i]].DeactivateDisplay();
            cardIconSlot.AddCardIcon(_originalDeckList[i]);
        }

        _deckListContent.gameObject.GetOrAddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void GoBackToMainMenu(PointerEventData data) { SceneLoadManager.Instance.LoadScene(Define.Scene.MainMenu, useNetworkSceneManager: false); }

    private void LoadDeckListFromJson()
    {
        string jsonData = File.ReadAllText(_sharedDeckListPath);
        _originalDeckList = JsonConvert.DeserializeObject<List<int>>(jsonData);
    }

    public bool AddtoDeckList(int id)
    {
        // safety check
        if (_deckList.Contains(id))
        {
            UI_Utilities.Instance.LogError("Card ID[" + id + "] has already added to the decklist");
            return false;
        }

        _deckList.Add(id);
        return true;
    }

    public bool RemoveFromDeckList(int id)
    {
        if (!_deckList.Contains(id))
        {
            UI_Utilities.Instance.LogError("Card ID[" + id + "] is not in the decklist");
            return false;
        }

        _deckList.Remove(id);
        return true;
    }

    private void SaveDeckToJson(PointerEventData data)
    {
        if (_deckList.Count < 5)
        {
            UI_Utilities.Instance.LogError("Deck should be include at least 5 cards");
            return;
        }

        string jsonData = JsonConvert.SerializeObject(_deckList);
        File.WriteAllText(_sharedDeckListPath, jsonData);

        _originalDeckList = _deckList.ToList();
        ShowIfDecklistChanged();

        UI_Utilities.Instance.Log("Deck Saved");
    }

    public void ShowIfDecklistChanged()
    {
        Button save = GetButton((int)Buttons.Save);
        TMP_Text saveText = GetTMPText((int)Texts.Save);

        if (_originalDeckList.SequenceEqual(_deckList))
        {
            saveText.text = "Saved";
            save.gameObject.RemoveUIEvent(Define.UIEvent.Click);
            save.interactable = false;
        }
        else
        {
            saveText.text = "Save";
            save.interactable = true;
            save.gameObject.AddUIEvent(SaveDeckToJson);
        }
    }
}
