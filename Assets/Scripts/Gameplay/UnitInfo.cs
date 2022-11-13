using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class UnitInfo : NetworkSingleton<UnitInfo>
{
    private CanvasGroup _unitInfo;
    [SerializeField] private RectTransform _content;
    [SerializeField] private TMP_Text _infoText;
    [SerializeField] private RectTransform _myPlayerUI;
    [SerializeField] private Camera _playerCamera;

    [SerializeField] private float _thresDis = 4500f;
    [SerializeField] private float _movexScale = 5f;

    [SerializeField] private float _fadeInDelay = 0.25f;
    [SerializeField] private float _fadeInDuration = 0.25f;

    bool _IsPlaying = false;
    private Rect _rect;
    private Sequence _s;

    private void Awake()
    {
        _unitInfo = GetComponent<CanvasGroup>();
    }

    [ClientRpc]
    public void PlayClientRpc(HexCoords coord, float unitHeight, int attack, int hp, int agility, ClientRpcParams clientRpcParams = default)
    {
        if (MyUIController.Instance.IsCameraMoving || _IsPlaying) return;
        _IsPlaying = true;

        if (_s != null) _s.Kill(false);
        _s = DOTween.Sequence();

        _infoText.text = $"ATK : {attack}\n" +
                         $"HP : {hp}\n" +
                         $"AGI : {agility}";

        var pos = MapGenerator.CoordsToWorldPos(coord) + new Vector3(0, unitHeight, 0);
        _unitInfo.transform.position = WorldPosToCoordsInCanvus(pos, _playerCamera, _myPlayerUI);

        var heading = pos - _playerCamera.transform.position;
        var distance = Vector3.Dot(heading, _playerCamera.transform.forward);

        _content.ForceUpdateRectTransforms();
        _rect = _content.rect;
        var frustumHeight = 2.0f * distance * Mathf.Tan(_playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        _unitInfo.transform.localPosition += new Vector3(0, _rect.height * 0.5f, 0);

        _s.AppendInterval(_fadeInDelay);
        _s.Append(_unitInfo.DOFade(1, _fadeInDuration))
            .Join(gameObject.transform.DOLocalMoveY(gameObject.transform.localPosition.y + _movexScale * (_thresDis / frustumHeight), _fadeInDuration));

        // Vector3 offset = new Vector3(0, (_rect.height * 0.5f) + _movexScale * (_thresDis / frustumHeight), 0);
        // _unitInfo.transform.localPosition += offset;
    }

    public void Stop()
    {
        _IsPlaying = false;
        if (_s != null) _s.Kill(false);
        _unitInfo.alpha = 0;
    }

    private Vector3 GetMouseCoordsInCanvas(Camera camera, RectTransform canvas)
    {
        // If the canvas render mode is in World Space,
        // We need to convert the mouse position into this rect coords.
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas,
            Input.mousePosition,
            camera,
            out Vector3 mousePosition);
        return mousePosition;
    }

    private Vector3 WorldPosToCoordsInCanvus(Vector3 worldPos, Camera camera, RectTransform canvas)
    {
        var worldToScreenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPos);

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas,
            worldToScreenPoint,
            camera,
            out Vector3 worldPointInRect);
        return worldPointInRect;
    }

    private void Update()
    {
        if (MyUIController.Instance.IsCameraMoving) Stop();
    }
}
