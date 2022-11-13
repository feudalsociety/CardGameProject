using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    float _timer, _refresh, _avgFramerate;
    string _display = "FPS : {0}";
    [SerializeField] TMP_Text _fpsText;

    private void Update()
    {
        float timelapse = Time.smoothDeltaTime;
        _timer = _timer <= 0 ? _refresh : _timer -= timelapse;

        if (_timer <= 0) _avgFramerate = (int)(1f/timelapse);
        _fpsText.text = string.Format(_display, _avgFramerate.ToString());
    }
}
