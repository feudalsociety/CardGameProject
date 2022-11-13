using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class UI_Surrender : UI_Popup
{
    private NetworkManager _netManager => NetworkManager.Singleton;

    enum Images
    {
        BackImage,
        PanelImage
    }

    enum Buttons
    {
        Yes,
        No
    }

    enum Texts
    {
        Title,
        MainText,
        Yes,
        No
    }

    public override void Init()
    {
        Managers.UI.SetCanvus(gameObject, true, RenderMode.ScreenSpaceOverlay);

        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        // Add Event
        GetButton((int)Buttons.Yes).gameObject.AddUIEvent(ExitGame);
        GetButton((int)Buttons.No).gameObject.AddUIEvent(CloseSurrenderPopup);
    }

    public void ExitGame(PointerEventData data)
    {
        GameMananger.Instance.GoToMainMenuServerRpc();
    }

    public void CloseSurrenderPopup(PointerEventData data) { ClosePopupUI(); }

    public override void ClosePopupUI()
    {
        MyUIController.Instance.CameraControl = true;
        (SceneLoadManager.Instance.CurrentScene as GamePlayScene).Dof.SetActive(false);
        gameObject.GetComponent<UIFadeScript>().FadeOut(fadeOutDuration: 0.08f, () => { base.ClosePopupUI(); });
    }
}
