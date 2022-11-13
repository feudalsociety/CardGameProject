using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFadeScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup _myUIGroup;

    public void FadeIn(float fadeInDuration, Action onComplete = null)
    {
        StartCoroutine(FadeCanvusGroup(_myUIGroup, _myUIGroup.alpha, 1, fadeInDuration, onComplete));
        _myUIGroup.interactable = true;
        _myUIGroup.blocksRaycasts = true;
    }

    public void FadeOut(float fadeOutDuration, Action onComplete = null)
    {
        StartCoroutine(FadeCanvusGroup(_myUIGroup, _myUIGroup.alpha, 0, fadeOutDuration, onComplete));
        _myUIGroup.interactable = false;
        _myUIGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeCanvusGroup(CanvasGroup cg, float start, float end, float lerpTime = 0.5f, Action OnComplete = null)
    {

        float timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;

        while(true)
        {
            timeSinceStarted = Time.time - timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            float currentValue = Mathf.Lerp(start, end, percentageComplete);

            cg.alpha = currentValue;

            if (percentageComplete >= 1)
            {
                OnComplete?.Invoke();
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
