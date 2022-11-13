using DG.Tweening;
using System.Linq;
using System;
using TMPro;
using UnityEngine;
using Unity.Netcode;

/// <summary>
///     Handles the load and error screens
/// </summary>
public class UI_Utilities : NetworkBehaviour
{
    public static UI_Utilities Instance;
    private NetworkManager _netManager => NetworkManager.Singleton;

    [SerializeField] private UIFadeScript _fader;
    [SerializeField] private float _fadeInDuration, _fadeOutDuration;
    [SerializeField] private float _errorFadeInDuration, _errorFadeOutDuration;
    [SerializeField] private float _errorDuration;
    [SerializeField] private TMP_Text _loaderText, _logText;
    [SerializeField] private TMP_Text _serverText;
    [SerializeField] private TMP_Text _playersInGameText;
    [SerializeField] public TMP_Text SessionText;

    [SerializeField] private int _maxLines = 30;
    [SerializeField] private Color _planeColor, _errorColor, _highlightColor;

    private void Awake()
    {
        Instance = this;

        Canvas canvas = gameObject.GetOrAddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = UIManager.LoadingOrder;

        DontDestroyOnLoad(gameObject);
        Toggle(false, instant: true);
        LogServer(false);

        SessionText.gameObject.SetActive(false);
    }

    void Update()
    {
        _playersInGameText.text = $"players in game : {GameNetPortal.Instance.PlayersInGame}";

        if (!_netManager.IsServer)
        {
            if (!_netManager.IsConnectedClient)
            {
                _playersInGameText.text = $"players in game : 0";
                LogServer(false);
            }
        }
    }

    public void Toggle(bool on, string text = null, bool instant = false)
    {
        _loaderText.text = text;

        if (on) _fader.FadeIn(fadeInDuration: instant? 0 : _fadeInDuration);
        else _fader.FadeOut(fadeOutDuration: instant? 0 : _fadeOutDuration);
    }

    public void Log(string log)
    {
        ClearLines();
        _logText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(_planeColor)}>{log}</color>\n";
    }

    public void LogError(string error)
    {
        ClearLines();
        _logText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(_errorColor)}>{error}</color>\n";
    }

    [ClientRpc]
    public void LogClientRpc(string log, ClientRpcParams clientRpcParams = default) => Log(log);

    [ClientRpc]
    public void LogErrorClientRpc(string error, ClientRpcParams clientRpcParams = default) => LogError(error);
    

    public void LogServer(bool on)
    {
        if(on) _serverText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(_highlightColor)}>[Server On]</color>\n";
        else _serverText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(_errorColor)}>[Server Off]</color>\n";
    }

    // only Server can invoke this
    public void LogSession(bool on)
    {
        if (on) SessionText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(_highlightColor)}>[Session On]</color>\n";
        else SessionText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(_errorColor)}>[Session Off]</color>\n";
    }

    private void ClearLines()
    {
        if (_logText.text.Split('\n').Count() >= _maxLines)
        {
            _logText.text = string.Empty;
        }
    }
}

public class Load : IDisposable
{
    public Load(string text)
    {
        UI_Utilities.Instance.Toggle(true, text);
    }

    public void Dispose()
    {
        UI_Utilities.Instance.Toggle(false);
    }
}