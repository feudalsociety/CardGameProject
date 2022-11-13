using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Michsky.MUIP;

public class UI_Options : UI_Popup
{
    [SerializeField] private Toggle _fullscreenToggle, _vsyncToggle;
    [SerializeField] private List<ResItem> _resolutions = new List<ResItem>();
    [SerializeField] HorizontalSelector _resSelector;
    private int _selectedResolution;

    enum Buttons
    {
        Close,
        Apply
    }

    public override void Init()
    {
        Managers.UI.SetCanvus(gameObject, true, RenderMode.ScreenSpaceOverlay);

        Bind<Button>(typeof(Buttons));

        // Add Event
        //GetButton((int)Buttons.ResLeft).gameObject.AddUIEvent(ResLeft);
        //GetButton((int)Buttons.ResRight).gameObject.AddUIEvent(ResRight);
        GetButton((int)Buttons.Close).gameObject.AddUIEvent(CloseOptionPopup);
        GetButton((int)Buttons.Apply).gameObject.AddUIEvent(ApplyGraphcis);

        _fullscreenToggle.isOn = Screen.fullScreen;
        if (QualitySettings.vSyncCount == 0) _vsyncToggle.isOn = false;
        else _vsyncToggle.isOn = true;

        bool foundRes = false;
        for (int i = 0; i < _resolutions.Count; i++)
        {
            if (Screen.width == _resolutions[i].Horizontal && Screen.height == _resolutions[i].Vertical)
            {
                foundRes = true;
            }
            AddNewResItem(_resolutions[i].Horizontal, _resolutions[i].Vertical);
        }
        if (!foundRes)
        {
            ResItem newRes = new ResItem();
            newRes.Horizontal = Screen.width;
            newRes.Vertical = Screen.height;
            _resolutions.Add(newRes);

            AddNewResItem(Screen.width, Screen.height);
            _resSelector.defaultIndex = _resolutions.Count - 1;
            _resSelector.SetupSelector();
        }
    }

    private void AddNewResItem(int width, int height)
    {
        string resText = width.ToString() + " x " + height.ToString();
        _resSelector.CreateNewItem(resText, null);
    }

    private void CloseOptionPopup(PointerEventData data)
    {
        gameObject.GetComponent<UIFadeScript>().FadeOut(fadeOutDuration: 0.08f, () => { base.ClosePopupUI(); });
    }

    private void ApplyGraphcis(PointerEventData data)
    {
        Screen.fullScreen = _fullscreenToggle.isOn;
        if (_vsyncToggle.isOn) QualitySettings.vSyncCount = 1;
        else QualitySettings.vSyncCount = 0;
        Screen.SetResolution(_resolutions[_selectedResolution].Horizontal, _resolutions[_selectedResolution].Vertical, _fullscreenToggle.isOn);
    }

    //public void UpdateResLabel()
    //{
    //    _resolutionLabel.text = _resolutions[_selectedResolution].Horizontal.ToString()
    //        + " x " + _resolutions[_selectedResolution].Vertical.ToString();
    //}

    //public void ResLeft(PointerEventData data)
    //{
    //    _selectedResolution--;
    //    if (_selectedResolution < 0) _selectedResolution = 0;
    //    UpdateResLabel();
    //}

    //public void ResRight(PointerEventData data)
    //{
    //    _selectedResolution++;
    //    if (_selectedResolution > _resolutions.Count - 1) _selectedResolution = _resolutions.Count - 1;
    //    UpdateResLabel();
    //}
}

[System.Serializable]
public class ResItem
{
    public int Horizontal;
    public int Vertical;
}