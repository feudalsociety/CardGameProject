using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class ClientCardBase : MonoBehaviour
{
    public abstract int CardId { get; }

    public Tween PosTween { get; set; }
    public Tween RotTween { get; set; }
    public Tween ScaleTween { get; set; }

    protected void InitTransform()
    {
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
    }

    public void KillTween()
    {
        PosTween.Kill();
        RotTween.Kill();
        ScaleTween.Kill();
    }

    private void OnDestroy() { KillTween(); }
}
