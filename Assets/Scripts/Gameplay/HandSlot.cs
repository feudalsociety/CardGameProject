using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;

// only responsible for tweening card object
public class HandSlot : MonoBehaviour
{
    [SerializeField] public float PopupFoward = 75.0f;
    [SerializeField] public float PopupUpward = 18.0f;
    [SerializeField] public float PopupDuration = 0.06f;
    [SerializeField] public float DropDuration = 0.09f;

    [SerializeField] public float SelectPopupFoward = 50.0f;
    [SerializeField] public float SelectPopupUpward = 30f;
    [SerializeField] public float SelectDuration = 0.03f;
    [SerializeField] public float SelectScaleFactor = 1.2f;

    [SerializeField] private Vector3 _originalScale = new Vector3(1f, 1f, 1f);
    private Vector3 _popupPos;
    private Vector3 _selectPos;

    public ClientCardBase Card { get; set; }
    public int Index { get; set; } = -1;

    Coroutine mouseOverCoroutine;

    // Use Coroutine, enter에서만 움직이는것이 아니고 mouse가 올려져있을 경우 매번 확인해야한다.
    IEnumerator MouseOver()
    {
        while (true)
        {
            if (Managers.Input.IsPointerOverUIElement(gameObject.name))
            {
                if (MyHandManager.Instance.SelectedIndex == -1 && !MyUIController.Instance.IsCameraMoving)
                {
                    MoveToPopupPos();
                }
                // 카메라가 움직이면 원위치
                else if (MyHandManager.Instance.SelectedIndex == -1 && MyUIController.Instance.IsCameraMoving)
                {
                    MoveToOriginPos();
                }
                yield return null;
            }
            else // 마우스가 ui위에 올려져 있지 않다면
            {
                if (MyHandManager.Instance.SelectedIndex != Index)
                {
                    // check if card is off its own placement
                    if (Vector3.SqrMagnitude(Card.transform.localPosition - MyHandManager.Instance.CardPositions[Index]) < 0.01f)
                    {
                        yield return null;
                    }

                    MoveToOriginPos();
                }
                yield return null;
            }

            yield return null;
        }
    }

    public void StartMouseOverEvent()
    {
        // if there is coroutine already running stops it and run
        if (mouseOverCoroutine != null) StopCoroutine(mouseOverCoroutine);
        mouseOverCoroutine = StartCoroutine(MouseOver());
    }

    public void StopMouseOVerEvent()
    {
        if (mouseOverCoroutine != null) StopCoroutine(mouseOverCoroutine);
    }

    void OnPointerDown(PointerEventData data)
    {
        // left button
        if (data.button == 0 && MyHandManager.Instance.SelectedIndex == -1 && MyHandManager.Instance.BlockAdding == false && !MyUIController.Instance.IsCameraMoving)
        {
            MyHandManager.Instance.SelectedIndex = Index;

            // object deselection, selectedUnit이 있다면
            if (SelectionManager.Instance.SelectedCoord.HasValue)
            {
                // SelectionManager.Instance.SelectedUnit.GetComponent<Outline>().enabled = false;
                SelectionManager.Instance.Deselect();
            }

            MoveToSelectPos();
        }
    }

    public void AddEventTriggers()
    {
        EventTrigger eventTrigger = gameObject.GetOrAddComponent<EventTrigger>();

        EventTrigger.Entry entry_PointerDown = new EventTrigger.Entry();
        entry_PointerDown.eventID = EventTriggerType.PointerDown;
        entry_PointerDown.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
        eventTrigger.triggers.Add(entry_PointerDown);
    }

    private void MoveToPopupPos()
    {
        Card.KillTween();

        _popupPos = new Vector3(transform.position.x, transform.position.y, transform.position.z)
                            + MyHandManager.Instance.WorldHandTransform.forward * (PopupFoward - Mathf.Abs(MyHandManager.Instance.CardPositions[Index].z))
                            + MyHandManager.Instance.WorldHandTransform.up * PopupUpward;

        Card.GetComponent<ClientCardBase>().PosTween = Card.transform.DOMove
            (_popupPos, PopupDuration);

        Card.GetComponent<ClientCardBase>().RotTween = Card.transform.DOLocalRotate(Vector3.zero, PopupDuration);
        Card.GetComponent<ClientCardBase>().ScaleTween = Card.transform.DOScale(_originalScale, PopupDuration);
    }

    public void MoveToSelectPos()
    {
        Card.KillTween();

        _selectPos = new Vector3(transform.position.x, transform.position.y, transform.position.z)
                           + MyHandManager.Instance.WorldHandTransform.forward * (SelectPopupFoward - Mathf.Abs(MyHandManager.Instance.CardPositions[Index].z))
                           + MyHandManager.Instance.WorldHandTransform.up * SelectPopupUpward;

        Card.GetComponent<ClientCardBase>().PosTween = Card.transform.DOMove
           (_selectPos, SelectDuration);

        Card.GetComponent<ClientCardBase>().RotTween = Card.transform.DOLocalRotate(Vector3.zero, SelectDuration);

        Card.GetComponent<ClientCardBase>().ScaleTween = Card.transform.DOScale(_originalScale * SelectScaleFactor, SelectDuration);
    }

    public void MoveToOriginPos()
    {
        Card.KillTween();

        Card.GetComponent<ClientCardBase>().PosTween = Card.transform.DOLocalMove(MyHandManager.Instance.CardPositions[Index], DropDuration);
        Card.GetComponent<ClientCardBase>().RotTween = Card.transform.DOLocalRotate(MyHandManager.Instance.CardRotations[Index], DropDuration);
        Card.GetComponent<ClientCardBase>().ScaleTween = Card.transform.DOScale(_originalScale, DropDuration);
    }
}
