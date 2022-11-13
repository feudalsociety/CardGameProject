using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class DeckBuilderScene : BaseScene
{
    [SerializeField] GameObject _hoveringCard;

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Deckbuilder;

        _hoveringCard.SetActive(false);
        Managers.UI.ShowSceneUI<UI_DeckBuilder>();
    }

    public void ShowHoveringCard(bool isActive) { _hoveringCard.SetActive(isActive); }
    public void SetHoveringCardTransform(Vector3 pos) 
    {
        _hoveringCard.transform.localRotation = Quaternion.Euler(Vector3.zero);
        _hoveringCard.transform.position = pos; 
    }

    public override void Clear()
    {
    }
}
