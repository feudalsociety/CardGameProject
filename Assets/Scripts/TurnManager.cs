using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class TurnManager : NetworkSingleton<TurnManager>
{
    private NetworkManager _netManager => NetworkManager.Singleton;
    private NetworkTimer _netTimer;

    // false(0) is player0 turn, true(1) is player1 turn
    private NetworkVariable<int> _whosTurn = new NetworkVariable<int>(-1);
    public int WhosTurn => _whosTurn.Value;

    [SerializeField] private TMP_Text _whosTurnText;
    [SerializeField] private Button _endTurnButton;
    [SerializeField] private TurnNotification _turnNotification;
    [SerializeField] private TMP_Text _myPlayerName;
    [SerializeField] private TMP_Text _enemyPlayerName;

    private void Awake()
    {
        _netTimer = GetComponent<NetworkTimer>();
        _whosTurn.OnValueChanged += OnTurnChanged;
    }

    [ServerRpc]
    public void StartGameServerRpc() => _netTimer.StartTimerForGameStartServerRpc();

    [ServerRpc]
    public void DecideWhoPlaysFirstServerRpc()
    {
        _whosTurn.Value = Random.Range(0, 2);
        UI_Utilities.Instance.LogClientRpc($"Game Starts with Player_{_whosTurn.Value} Turn");
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        (int playerNumber, ClientRpcParams clientRpcParams) t = GameMananger.Instance.GetPlayerNumberAndClientRpcParam(clientId);
        if (t.playerNumber == -1) return;

        if (CheckCurrentTurn(t.playerNumber)) TakeNextTurnServerRpc();
        else UI_Utilities.Instance.LogErrorClientRpc($"It's not your turn", t.clientRpcParams);
    }

    [ServerRpc]
    public void TakeNextTurnServerRpc()
    {
        _netTimer.StopTimerServerRpc();
        _whosTurn.Value = (_whosTurn.Value == 1 ? 0 : 1);
        _netTimer.StartTimerForNextTurnServerRpc();

        // TODO : ClientRpc -> StartATurnCommand
    }

    public bool CheckCurrentTurn(int turn) => _whosTurn.Value == turn;

    private void OnTurnChanged(int previous, int current)
    {
        UI_Utilities.Instance.Log($"Turn Changed, current Turn : Player_{current}");
        var myPlayerId = GameMananger.Instance.NetworkPlayersData.getPlayerNumber(_netManager.LocalClientId);
        if (myPlayerId == current)
        {
            _turnNotification.Play(_myPlayerName.text);
            _whosTurnText.text = "END TURN";
            _endTurnButton.interactable = true;
        }
        else
        {
            _turnNotification.Play(_enemyPlayerName.text);
            _whosTurnText.text = "ENEMY TURN";
            _endTurnButton.interactable = false;
        }
    }
}
