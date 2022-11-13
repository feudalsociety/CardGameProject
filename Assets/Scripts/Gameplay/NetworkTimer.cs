using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using System.Collections;
using TMPro;

public class NetworkTimer : NetworkBehaviour
{
    private NetworkManager _netManager => NetworkManager.Singleton;

    [SerializeField] private TMP_Text _timerText;

    [SerializeField] private float _delayedGamePlayStartTime = 3.0f;
    [SerializeField] private float _timeForOneTurn = 15.0f;

    // only server can update this timetilZero, responsible for client timer visual countdown
    // also used for starting game
    private float _timeTillZero = 0.0f;

    private bool _clientStartCountdown;
    private NetworkVariable<bool> _countdownStarted = new NetworkVariable<bool>(false);
    public bool CountDownStarted => _countdownStarted.Value;

    private bool _clientGameStarted;
    private NetworkVariable<bool> _hasGameStarted = new NetworkVariable<bool>(false);

    private void Awake() 
    { 
        if (IsServer) 
        {
            _hasGameStarted.Value = false;
            _clientStartCountdown = false; 
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            _clientStartCountdown = false;
            _clientGameStarted = false;
            _countdownStarted.OnValueChanged += (oldValue, newValue) => { _clientStartCountdown = newValue; };
            _hasGameStarted.OnValueChanged += (oldValue, newValue) => { _clientGameStarted = newValue; };
        }
        base.OnNetworkSpawn();
    }

    [ServerRpc]
    public void StartTimerForGameStartServerRpc()
    {
        _countdownStarted.Value = true;
        _timeTillZero = _delayedGamePlayStartTime;
        UI_Utilities.Instance.LogClientRpc($"Game Start in {_delayedGamePlayStartTime} seconds...");
    }

    [ServerRpc]
    public void StartTimerForNextTurnServerRpc()
    {
        _countdownStarted.Value = true;
        _timeTillZero = _timeForOneTurn;
        SetReplicatedTimeTillZeroClientRPC(_timeTillZero);
    }

    [ServerRpc]
    public void StopTimerServerRpc() => _countdownStarted.Value = false;

    [ClientRpc]
    private void SetReplicatedTimeTillZeroClientRPC(float timeTillZero) 
        => _timeTillZero = timeTillZero;

    private bool ShouldStartCountdown()
    {
        if (IsServer) return _countdownStarted.Value;
        else return _clientStartCountdown;
    }

    private bool HasGameStarted()
    {
        if (IsServer) return _hasGameStarted.Value;
        return _clientGameStarted;
    }

    private void UpdateTimer()
    {
        if (!ShouldStartCountdown()) return;

        if(_timeTillZero > 0.0f)
        {
            _timeTillZero -= Time.deltaTime;

            if (IsServer && _timeTillZero <= 0.0f)
            {
                StopTimerServerRpc();

                if (!_hasGameStarted.Value)
                {
                    UI_Utilities.Instance.LogClientRpc("Game Started!");
                    _hasGameStarted.Value = true;
                    TurnManager.Instance.DecideWhoPlaysFirstServerRpc();
                    StartTimerForNextTurnServerRpc();
                }
                else TurnManager.Instance.TakeNextTurnServerRpc();
            }
        }
        if (HasGameStarted() && _timerText != null) _timerText.text = ToString();
    }

    private void Update()
    {
        UpdateTimer();
    }

    public override string ToString()
    {
        int inSeconds = Mathf.RoundToInt(_timeTillZero);
        string justSeconds = (inSeconds % 60).ToString();
        if (justSeconds.Length == 1)
            justSeconds = "0" + justSeconds;
        string justMinutes = (inSeconds / 60).ToString();
        if (justMinutes.Length == 1)
            justMinutes = "0" + justMinutes;

        return string.Format("{0}:{1}", justMinutes, justSeconds);
    }
}
