using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

public class MainMenuScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.MainMenu;

        Managers.UI.ShowSceneUI<UI_MainMenu>();
        Managers.UI.MakeSubItem<UI_Network>();
    }

    public override void Clear()
    {
    }
}
