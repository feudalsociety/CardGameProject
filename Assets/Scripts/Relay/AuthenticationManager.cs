using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Collections;
using TMPro;
using DG.Tweening;
using System.Text;

public class AuthenticationManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _log;
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private float _logFadeInDuration, _logFadeOutDuration;

    private void Awake()
    {
        _log.gameObject.SetActive(false);
    }

    public async void LoginAnonymously()
    {
        _log.text = "Logging your in...";
        byte[] bytesInUni = Encoding.UTF8.GetBytes(_playerNameInput.text);
        if(bytesInUni.Length > FixedString64Bytes.UTF8MaxLengthInBytes)
        {
            UI_Utilities.Instance.LogError("Player Name is too long");
            return;
        }

        GameNetPortal.Instance.PlayerName = _playerNameInput.text;

        _log.gameObject.SetActive(true);
        _log.DOFade(1, _logFadeInDuration).OnComplete(() => { _log.DOFade(0, _logFadeOutDuration).SetDelay(1); });

        await Authentication.Login();
        SceneLoadManager.Instance.LoadScene(Define.Scene.MainMenu, useNetworkSceneManager: false);

        _log.gameObject.SetActive(false);
    }
}