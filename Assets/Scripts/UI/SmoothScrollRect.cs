using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmoothScrollRect : ScrollRect
{
    public bool SmoothScrolling { get; set; } = true;
    public float SmoothScrollTime { get; set; } = 0.3f;
    public float InertiaTime { get; set; } = 0.15f;
    public float VerticalBoundary { get; } = 0.10f; // viewSize 기준

    private float _scrollTime = 0.0f;
    private Vector2 _pointerStartLocalCursor = Vector2.zero;
    private bool _dragging = false;
    private bool _scrolling = false;
    private Rect _viewBound { get { return GetComponent<RectTransform>().rect; } }
    private float _velocity = 0.0f;
    private float _elasticity = 0.1f; // 얼마나 빠르게 돌아오는지에 대한 시간
    private float _decelRate = 0.03f;
    private float _prevPosition = 0;

    private float _lowerLimit { get { return _viewBound.size.y * 0.5f; } }
    private float _upperLimit { get { return content.rect.size.y - (_viewBound.size.y * 0.5f); } }


    public override void OnBeginDrag(PointerEventData data)
    {
        if (!IsActive()) return;
        if (content.rect.size.y < _viewBound.size.y) return;
        if (Managers.Input.IsPointerOverUIElement("UI_CardDisplay")) return;
        if (data.button != PointerEventData.InputButton.Left) return;

        _pointerStartLocalCursor = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, data.position, data.pressEventCamera, out _pointerStartLocalCursor);
        m_ContentStartPosition = content.anchoredPosition;
        _dragging = true;
    }

    public override void OnDrag(PointerEventData data)
    {
        if (!IsActive()) return;
        if (!_dragging) return;
        if (content.rect.size.y < _viewBound.size.y) return;
        if (data.button != PointerEventData.InputButton.Left) return;

        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, data.position, data.pressEventCamera, out localCursor)) return;

        // 마우스 시작 위치와 현재 위치의 차이
        var pointerDelta = localCursor - _pointerStartLocalCursor;
        Vector2 position = m_ContentStartPosition + pointerDelta;

        float offsetY = 0.0f;
        if(position.y > _upperLimit) offsetY = _upperLimit - position.y; // +
        else if (position.y < _lowerLimit) offsetY = _lowerLimit - position.y; // -
        
        position.y += offsetY;
        position.y = position.y - RubberDelta(offsetY, _viewBound.size.y);
        position.y = Mathf.Clamp(position.y, _lowerLimit + _viewBound.y * VerticalBoundary, _upperLimit - _viewBound.y * VerticalBoundary);

        SetContentAnchoredPosition(position);
        base.OnDrag(data);
    }

    public override void OnEndDrag(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left)
            return;

        _dragging = false;
    }

    public override void OnScroll(PointerEventData data)
    {
        if (!IsActive()) return;
        if (content.rect.size.y < _viewBound.size.y) return;
        if (data.IsScrolling()) _scrolling = true;

        if (SmoothScrolling)
        {
            Vector2 positionBefore = normalizedPosition;
            content.DOKill(complete: true);
            this.DOKill(complete: true);

            base.OnScroll(data);

            // 위치 제한
            if (normalizedPosition.y > 1.0f)
            {
                normalizedPosition = new Vector2(0, 1.0f + (VerticalBoundary * _viewBound.size.y / content.sizeDelta.y));
                if (Mathf.Abs(normalizedPosition.y - positionBefore.y) < 0.12f) _scrollTime = InertiaTime;
                else _scrollTime = SmoothScrollTime;
            }
            else if (normalizedPosition.y < 0.0f)
            {
                normalizedPosition = new Vector2(0, 0.0f - (VerticalBoundary * _viewBound.size.y / content.sizeDelta.y));
                if (Mathf.Abs(normalizedPosition.y - positionBefore.y) < 0.12f) _scrollTime = InertiaTime;
                else _scrollTime = SmoothScrollTime;
            }
            else _scrollTime = SmoothScrollTime;

            Vector2 positionAfter = normalizedPosition;
            normalizedPosition = positionBefore;

            this.DONormalizedPos(positionAfter, _scrollTime).OnComplete((TweenCallback)(() =>
            {
                if (content.anchoredPosition.y < _lowerLimit)
                {
                    this.DOKill(complete: true);
                    content.DOAnchorPosY(_lowerLimit, InertiaTime).SetEase(Ease.OutSine);
                }
                if (content.anchoredPosition.y > _upperLimit)
                {
                    this.DOKill(complete: true);
                    content.DOAnchorPosY(_upperLimit, InertiaTime).SetEase(Ease.OutSine);
                }
            }));
        }
        else
        {
            base.OnScroll(data);
        }
    }

    protected override void LateUpdate()
    {
        // for layout rebuild
        base.LateUpdate();

        Vector2 position = content.anchoredPosition;
        float deltaTime = Time.unscaledDeltaTime;
        float offsetY = 0;
        if (position.y < _lowerLimit) offsetY = _lowerLimit - position.y; // + 
        else if (position.y > _upperLimit) offsetY = _upperLimit - position.y; // -

        if (!_dragging && _velocity != 0.0f)
        {
            if (offsetY != 0)
            {
                float speed = _velocity;
                float smoothTime = _elasticity;
                if (_scrolling) smoothTime *= 3.0f;

                if(offsetY > 0) position.y = Mathf.SmoothDamp(content.anchoredPosition[1], _lowerLimit, ref speed, smoothTime, Mathf.Infinity, deltaTime);
                else position.y = Mathf.SmoothDamp(content.anchoredPosition[1], _upperLimit, ref speed, smoothTime, Mathf.Infinity, deltaTime);

                if (Mathf.Abs(speed) < 1) speed = 0;
                _velocity = speed;
            }

            // Inertia
            if (_scrolling) _velocity = 0.0f;
            _velocity *= Mathf.Pow(_decelRate, deltaTime);
            if (Mathf.Abs(_velocity) < 1) _velocity = 0;
            position.y += _velocity * deltaTime;
        }
        if (_dragging)
        {
            float newVelocity = (content.anchoredPosition.y - _prevPosition) / deltaTime;
            _velocity = Mathf.Lerp(_velocity, newVelocity, deltaTime * 10);
        }
        if (content.anchoredPosition.y != _prevPosition) _prevPosition = content.anchoredPosition.y;

        SetContentAnchoredPosition(position);
        _scrolling = false;
    }

    public override void Rebuild(CanvasUpdate executing)
    {
        base.Rebuild(executing);
        _prevPosition = content.anchoredPosition.y;
    }

    private static float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - (1 / ((Mathf.Abs(overStretching) * 0.1f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.DOKill(complete: true);
    }
}