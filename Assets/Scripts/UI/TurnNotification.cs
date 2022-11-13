using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnNotification : MonoBehaviour
{
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private float _fadeOutDuration = 0.5f;
    [SerializeField] private float _fadeOutDelay = 1f;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _notification;

    private Sequence _s;

    public void Play(string playerName)
    {
        if(_s != null) _s.Kill(false);
        _s = DOTween.Sequence();

        _notification.text = $"{playerName}'s Turn";

        _s.Append(_canvasGroup.DOFade(1, _fadeInDuration));
        _s.AppendInterval(_fadeOutDelay);
        _s.Append(_canvasGroup.DOFade(0, _fadeOutDuration));
    }
}
