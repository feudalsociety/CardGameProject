using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Loading : UI_Base
{
    enum Images
    { 
        BackImage
    }

    enum Texts
    {
        LoadingText
    }

    enum GameObjects
    { 
        Slider
    }

    public override void Init()
    {
        Canvas canvas = gameObject.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = UIManager.LoadingOrder;

        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
    }

    public void SetProgressBar(float value)
    {
        if (value > 1 || value < 0)
        {
            Debug.Log("ProgressBar value should be between 0 and 1");
            return;
        }

        Slider slider = GetObject((int)GameObjects.Slider).GetComponent<Slider>();
        slider.value = value;
    }
}
