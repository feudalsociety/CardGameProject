using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GamePlayScene : BaseScene
{
    [SerializeField] public GameObject Bloom;
    [SerializeField] public GameObject Dof;
    [SerializeField] TurnNotification _turnNotification;

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.GamePlay;
    }

    // override baseScene's Awake method
    private void Awake()
    {
        Init();
        Managers.Input.KeyAction -= OnkeyPressed;
        Managers.Input.KeyAction += OnkeyPressed;
    }

    public void OpenSurrenderPopup()
    {
        // Stop camera movement
        MyUIController.Instance.CameraControl = false;
        // UI_Root은 Main camera의 자식이어야한다. 
        (SceneLoadManager.Instance.CurrentScene as GamePlayScene).Dof.SetActive(true);
        UI_Surrender surrenderPanel = Managers.UI.ShowPopupUI<UI_Surrender>("UI_Surrender");
        surrenderPanel.gameObject.GetComponent<UIFadeScript>().FadeIn(fadeInDuration: 0.06f);
    }

    void OnkeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Managers.UI.PopupNum == 0)
            {
                OpenSurrenderPopup();
            }
            else
            {
                Managers.UI.GetPopupUI().ClosePopupUI();
            }
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            var selectedCoord = SelectionManager.Instance.SelectedCoord;
            if (selectedCoord.HasValue)
            {
                var currentTarget = SelectionManager.Instance.CurrentTarget;
                if(currentTarget && currentTarget.IsObjectTypeOf<Tile>())
                {
                    UnitManager.Instance.RequestMoveUnitServerRpc(selectedCoord.Value, currentTarget.GetComponent<Tile>().Coord);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            var selectedCoord = SelectionManager.Instance.SelectedCoord;
            if (selectedCoord.HasValue)
            {
                var unit = MapGenerator.Instance.Tiles[selectedCoord.Value].GetUnitOnTile();
                if(unit != null)
                {
                    unit.RequestUnitAttackServerRpc();
                }
            }
        }
    }

    private void OnDestroy()
    {
        Managers.Input.KeyAction -= OnkeyPressed;
    }

    public override void Clear()
    {

    }
}
