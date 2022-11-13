using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System;
using System.Linq;
using DG.Tweening;
using Unity.Netcode;

// 여기에 의존하고 있는 코드가 많으므로 runtime instantiate는 하지 않는것으로 한다.
// TODO : Network부분과 visual 부분을 나눈다.
public class MyHandManager : Singleton<MyHandManager>
{
    [SerializeField] public UI_MyPlayer MyPlayerUI;
    [SerializeField] private GameObject _handUIPrefab;
    public Transform WorldHandTransform { get; private set; }

    [SerializeField] float _handYOffset = -370f;
    [SerializeField] float _handZOffset = 5f;

    [SerializeField][Range(0f, 5000f)] float _curvRadius = 2000f;
    [SerializeField][Range(0f, 180f)] float _maxDegree = 20.0f;
    [SerializeField][Range(0f, 10f)] float _maxDegreeBetween = 3.0f;
    [SerializeField][Range(0f, 5f)] float _frontrearSpacing = 0.2f;

    float _firstDegree;
    float _degreeBetween;

    public List<Vector3> CardPositions { get; private set; } = new List<Vector3>();
    public List<Vector3> CardRotations { get; private set; } = new List<Vector3>();

    // TODO : NetworkList로 별도의 class에 visual과 분리한다.
    private List<ClientCardBase> _clientCards = new List<ClientCardBase>();
    private List<HandSlot> _handSlots = new List<HandSlot>();
    private ObjectPool<HandSlot> _handSlotPool;

    public int SelectedIndex { get; set; } = -1;
    public int CardNum => _clientCards.Count;

    public const int ClientMaxHandCardNum = 10;
    private static readonly int _openingDrawCardNum = 5;

    [SerializeField] float _openingDrawDuration = 0.7f;
    [SerializeField] float _moveDuration = 1.0f;
    [SerializeField] float _openingHandDelay = 0.05f;

    public bool BlockAdding { get; private set; } = false;

    void Awake()
    {
        Managers.Input.MouseAction -= OnMouseClicked;
        Managers.Input.MouseAction += OnMouseClicked;

        for (int i = 0; i < 10; i++)
        {
            CardPositions.Add(Vector3.zero);
            CardRotations.Add(Vector3.zero);
        }

        CreateHandUIPool();
        InitializeWorldHandPos();
    }

    // TODO : 초반에 DrawOpening이 일어나지 않는 상태에서 draw가능하게함?
    public void DrawOpeningHand()
    {
        try
        {
            for (int i = 0; i < _openingDrawCardNum; i++)
            {
                ClientCardBase clientCard = MyPlayerUI.Deck.RemoveCardFromDeck();
                AddCardToHand(clientCard);
            }
        }
        catch (Exception ex)
        {
            UI_Utilities.Instance.LogError($"DrawOpeningHand error : {ex.Message}");
        }

        CalculateHandTransform();
        OpeningDrawAnimation();
    }

    // clickedObject는 Tile Type이라는것이 보장되어있다.
    public void PlayCardFromHand(int selectedIndex, ulong tileNetId)
    {
        ClientCardBase clientCard = null;
        try
        {
            clientCard = RemoveCardFromHand(selectedIndex);
        }
        catch (Exception ex)
        {
            UI_Utilities.Instance.LogError($"PlayCardFromHand error : {ex.Message}");
        }

        // TODO : clientrpc, reference networkobject reference
        // Or use ClienTiles, clientRpcs might need to be queued with command wrapper later
        var tile = NetworkManager.Singleton.SpawnManager.SpawnedObjects[tileNetId];
        clientCard.transform.SetParent(tile.transform);
        SelectedIndex = -1;
        CalculateHandTransform();
        UpdateHandAnimation();
    }

    private void CreateHandUIPool()
    {
        _handSlotPool = new ObjectPool<HandSlot>(() =>
        {
            GameObject button = Instantiate(_handUIPrefab);
            button.name = $"unassigned";
            HandSlot UI = button.GetOrAddComponent<HandSlot>();
            return UI;
        },
        handSlot =>
        {
            handSlot.gameObject.SetActive(true);
        },
        handSlot =>
        {
            handSlot.gameObject.name = $"unassigned";
            handSlot.gameObject.SetActive(false);
        },
        handSlot =>
        {
            Managers.Resource.Destroy(handSlot.gameObject);
        }, false, 10, 10);
    }

    private void InitializeWorldHandPos()
    {
        GameObject go = new GameObject("WorldHand");
        go.transform.SetParent(MyUIController.Instance.PlayerCamera.transform);
        WorldHandTransform = go.transform;
        WorldHandTransform.localPosition = new Vector3(0f, _handYOffset, UI_MyPlayer.DistanceFromCamera + _handZOffset);
        WorldHandTransform.localEulerAngles = new Vector3(-90.0f, 0.0f, 0.0f);
    }

    public void CalculateHandTransform()
    {
        _degreeBetween = _maxDegree / ((float)_clientCards.Count - 1);
        _firstDegree = _maxDegree / 2;

        if (_clientCards.Count <= 7 && _degreeBetween >= _maxDegreeBetween)
        {
            _degreeBetween = _maxDegreeBetween;
            _firstDegree = _maxDegreeBetween * ((float)_clientCards.Count - 1) / 2;
        }

        for (int i = 0; i < _clientCards.Count; i++)
        {
            CardPositions[i] =
                new Vector3
                (_curvRadius * Mathf.Cos(Mathf.PI / 2 + (_firstDegree - (i * _degreeBetween)) * Mathf.Deg2Rad),
                    i * _frontrearSpacing,
                   _curvRadius * (Mathf.Sin(Mathf.PI / 2 + (_firstDegree - (i * _degreeBetween)) * Mathf.Deg2Rad)
                   - Mathf.Sin(Mathf.PI / 2 + (_firstDegree * Mathf.Deg2Rad)))
                );

            CardRotations[i] = new Vector3(0f, -_firstDegree + (i * _degreeBetween), 0f);
        }
    }

    private void AddEventTriggers(int index)
    {
        _handSlots[index].AddEventTriggers();
        _handSlots[index].StartMouseOverEvent();
    }

    public void AddCardToHand(ClientCardBase clientCard)
    {
        if (BlockAdding) throw new Exception("You can't add a card to the hand card while adding other card to hand");
        if (clientCard == null) throw new Exception("Null exception, the card you are trying to add does not exist");

        // BlockAdding = true; Opening Draw에서도 사용하므로 block하지 않는다.
        clientCard.gameObject.SetActive(true);
        clientCard.transform.SetParent(WorldHandTransform);
        _clientCards.Add(clientCard);

        HandSlot handSlot = _handSlotPool.Get();
        handSlot.transform.position = WorldHandTransform.position; // handUI start position
        _handSlots.Add(handSlot);

        handSlot.Card = clientCard;
        handSlot.Index = _clientCards.Count - 1;
        handSlot.name = "hand_" + handSlot.Index;
        handSlot.transform.SetParent(transform);
    }

    public void MoveCardToGraveFromHand(int selectedIndex)
    {
        try
        {
            ClientCardBase clientCard = RemoveCardFromHand(selectedIndex);
            MyPlayerUI.Grave.AddCard(clientCard);
        } 
        catch (Exception ex)
        {
            UI_Utilities.Instance.LogError($"MoveCardToGraveFromHand error : {ex.Message}");
        }

        SelectedIndex = -1;
        CalculateHandTransform();
        UpdateHandAnimation();
    }

    private ClientCardBase RemoveCardFromHand(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= _clientCards.Count)
            throw new Exception("RemoveCardFromHand, index range error");

        ClientCardBase clientCard = _clientCards[selectedIndex];
        clientCard.gameObject.SetActive(false);
        _clientCards.RemoveAt(selectedIndex);

        // return handUI to the pool
        HandSlot temp = _handSlots[selectedIndex];
        temp.StopMouseOVerEvent();
        temp.Card = null;
        temp.Index = -1;
        _handSlots.RemoveAt(selectedIndex);
        _handSlotPool.Release(temp);

        for (int i = selectedIndex; i < _clientCards.Count; i++)
        {
            _handSlots[i].Index -= 1;
            _handSlots[i].name = "handSlot_" + _handSlots[i].Index;
        }

        return clientCard;
    }

    public void ReleaseSelectedCard() => SelectedIndex = -1;

    #region Animations
    private void UpdateHandAnimation()
    {
        // Card는 HandUI를 따라가게 되어있다.
        for (int i = 0; i < _clientCards.Count; i++)
        {
            _handSlots[i].gameObject.transform.DOLocalMove(
                new Vector3(CardPositions[i].x, CardPositions[i].z, 0) + new Vector3(0f, _handYOffset, 0f), _moveDuration);
            _handSlots[i].gameObject.transform.DOLocalRotate(new Vector3(0f, 0f, -CardRotations[i].y), _moveDuration);
        }
    }

    private void OpeningDrawAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        Tween[] tweens = new Tween[_openingDrawCardNum];

        sequence.onPlay = () => { BlockAdding = true; };
        sequence.onComplete = () => { BlockAdding = false; };

        foreach (var t in tweens.Select((value, index) => (value, index)))
        {
            sequence.Insert(_openingHandDelay * t.index, tweens[t.index] = _handSlots[t.index].gameObject.transform.DOLocalMove(
            new Vector3(CardPositions[t.index].x, CardPositions[t.index].z, 0) + new Vector3(0f, _handYOffset, 0f), _openingDrawDuration)
            .OnComplete(() => { AddEventTriggers(t.index); }));

            sequence.Insert(_openingHandDelay * t.index, _handSlots[t.index].gameObject.transform.DOLocalRotate(new Vector3(0f, 0f, -CardRotations[t.index].y), _openingDrawDuration));

            sequence.Insert(_openingHandDelay * t.index, _clientCards[t.index].PosTween = _clientCards[t.index].transform.DOLocalMove(CardPositions[t.index], _openingDrawDuration));
            sequence.Insert(_openingHandDelay * t.index, _clientCards[t.index].RotTween = _clientCards[t.index].transform.DOLocalRotate(CardRotations[t.index], _openingDrawDuration));
        }
    }

    public void DrawAnimation()
    {
        // add Event trigger to new drawn card's ui
        _handSlots[_clientCards.Count - 1].gameObject.transform.DOLocalMove(
                new Vector3(CardPositions[_clientCards.Count - 1].x, CardPositions[_clientCards.Count - 1].z, 0) + new Vector3(0f, _handYOffset, 0f), _moveDuration)
            .OnPlay(() => { BlockAdding = true; })
            .OnComplete(() => { AddEventTriggers(_clientCards.Count - 1); BlockAdding = false; });

        _handSlots[_clientCards.Count - 1].gameObject.transform.DOLocalRotate(new Vector3(0f, 0f, -CardRotations[_clientCards.Count - 1].y), _moveDuration);

        // MoveHandUI
        for (int i = 0; i < _clientCards.Count - 1; i++)
        {
            _handSlots[i].gameObject.transform.DOLocalMove(
                new Vector3(CardPositions[i].x, CardPositions[i].z, 0) + new Vector3(0f, _handYOffset, 0f), _moveDuration);
            _handSlots[i].gameObject.transform.DOLocalRotate(new Vector3(0f, 0f, -CardRotations[i].y), _moveDuration);
        }

        // Card는 HandUI를 따라가게 되어있다.
        // 단, 여기서는 eventTrigger가 추가되지 않았기때문에 직접 이동시켜줘야한다.

        _clientCards[_clientCards.Count - 1].KillTween();
        _clientCards[_clientCards.Count - 1].PosTween = _clientCards[_clientCards.Count - 1].transform.DOLocalMove(CardPositions[_clientCards.Count - 1], _moveDuration);
        _clientCards[_clientCards.Count - 1].RotTween = _clientCards[_clientCards.Count - 1].transform.DOLocalRotate(CardRotations[_clientCards.Count - 1], _moveDuration);
    }
    #endregion

    void OnMouseClicked(Define.MouseEvent evt, Define.MouseButton button)
    {
        switch (button)
        {
            case Define.MouseButton.Left:
                break;
            case Define.MouseButton.Right:
                ReleaseSelectedCard();
                break;
        }
    }

    private void OnDestroy()
    {
        Managers.Input.MouseAction -= OnMouseClicked;
    }
}
