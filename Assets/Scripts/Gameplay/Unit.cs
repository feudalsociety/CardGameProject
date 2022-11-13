using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
using System;
using UnityEngine.VFX;


// Owned by server, only server is authorized to change network variable
public class Unit : NetworkBehaviour
{
    [SerializeField] private float _oneStepDuration = 0.6f;
    [SerializeField] private Outline _outline;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _lineRenderHoverHeight = 5f;
    [SerializeField] private SkinnedMeshRenderer _renderer;
    [SerializeField] private Healthbar _healthbar;
    [SerializeField] private VisualEffect _slash;

    public float UnitHeight { get; private set; }

    private NetworkVariable<HexCoords> _coord = new NetworkVariable<HexCoords>();
    private NetworkVariable<Define.UnitState> _state = new NetworkVariable<Define.UnitState>() { Value = Define.UnitState.Idle };
    private NetworkVariable<int> _ownerPlayerNumber = new NetworkVariable<int>(-1);
    private ServerCard _serverCard;
    // 매번 downcasting하는것을 피하기 위해서 reference를 저장한다. 
    private ServerUnitBaseData _unitData;

    private Animator _animator;

    // Caching
    private List<HexCoords> _walkableCoords = new List<HexCoords>();
    private Define.UnitState _oldUnitState = Define.UnitState.Idle;

    public HexCoords Coord => _coord.Value;
    public Define.UnitState State => _state.Value;
    public int OwnerPlayerNumber => _ownerPlayerNumber.Value;
    public ServerCard ServerCard => _serverCard;

    public override void OnNetworkSpawn()
    {
        _animator = GetComponent<Animator>();
        UnitHeight = _renderer.bounds.extents.x * 2.0f;

        // TODO for the player unit
        if (_healthbar != null) _healthbar.Init(UnitHeight);

        _state.OnValueChanged += OnUnitStateChanged;
        _ownerPlayerNumber.OnValueChanged += SetUnitColorClientRpc;
        base.OnNetworkSpawn();
    }

    // can only be called by the server
    public void Init(HexCoords coord, int playerNumber, ServerCard serverCard)
    {
        _coord.Value = coord;
        _state.Value = Define.UnitState.Idle;
        _ownerPlayerNumber.Value = playerNumber;
        _serverCard = serverCard;
        _unitData = serverCard.CardData as ServerUnitBaseData;
    }

    private void OnUnitStateChanged(Define.UnitState oldUnitState, Define.UnitState currentUnitState)
    {
        if (_oldUnitState != currentUnitState)
        {
            _oldUnitState = currentUnitState;

            switch(currentUnitState)
            { 
                case Define.UnitState.Idle:
                    _lineRenderer.enabled = false;
                    break;
                case Define.UnitState.Move:
                    _lineRenderer.enabled = true;
                    break;
            }

            // animation
            _animator.SetTrigger($"{currentUnitState}");
        }
    }

    [ClientRpc]
    private void SetUnitColorClientRpc(int oldPlayerNumber, int currentPlayerNumber)
    {
        if(currentPlayerNumber == 0)
        {
            _lineRenderer.colorGradient = GameMananger.Instance.Player0lineRendererColor;
            _outline.OutlineColor = GameMananger.Instance.Player0OutlineColor;
            _healthbar.SetHealthbarColor(GameMananger.Instance.Player0HealthbarColor);
        }
        else if (currentPlayerNumber == 1)
        {
            _lineRenderer.colorGradient = GameMananger.Instance.Player1lineRendererColor;
            _outline.OutlineColor = GameMananger.Instance.Player1OutlineColor;
            _healthbar.SetHealthbarColor(GameMananger.Instance.Player1HealthbarColor);
        }
    }

    [ClientRpc]
    private void DrawPathLineClientRpc(HexCoords[] pathCoords)
    {
        _lineRenderer.positionCount = pathCoords.Length;
        for (int i = 0; i < pathCoords.Length; i++)
        {
            var linePos = MapGenerator.CoordsToWorldPos(pathCoords[i]) + new Vector3(0, _lineRenderHoverHeight, 0);
            _lineRenderer.SetPosition(i, linePos);
        }
    }

    public void Move(HexCoords targetCoord, ulong clientId)
    {
        if(!MapGenerator.Instance.Tiles[targetCoord].Walkable)
            throw new Exception($"Tile{targetCoord} is not walkable");

        if (State != Define.UnitState.Idle) throw new Exception("This unit is currently on other action");

        var startTile = MapGenerator.Instance.Tiles[Coord];
        var endTile = MapGenerator.Instance.Tiles[targetCoord];
        var path = Pathfinding.FindPath(startTile, endTile);

        if (path.Count > 0)
        {
            if(path.Count > _unitData.Agility + 1)
                throw new Exception($"Tile{targetCoord} is so far away from this unit");

            HexCoords[] pathCoords = new HexCoords[path.Count];
            for (int i = 0; i < path.Count; i++) pathCoords[i] = path[i].Coord;
            DrawPathLineClientRpc(pathCoords);

            Sequence seq = DOTween.Sequence();
            seq.OnStart(() => 
            {
                HideWalkableTilesServerRpc(clientId);
                _state.Value = Define.UnitState.Move;
            });
            seq.OnComplete(() => { _state.Value = Define.UnitState.Idle; });

            for (int i = path.Count - 2; i >= 0; i--)
            {
                var currentTile = path[i + 1];
                var nextTile = path[i];

                var coordDiff = nextTile.Coord - currentTile.Coord;
                Vector3 nextRot = Vector3.zero;

                if (coordDiff == new HexCoords(1, 0)) nextRot = new Vector3(0f, 90f, 0f);
                else if (coordDiff == new HexCoords(0, 1)) nextRot = new Vector3(0f, 30f, 0f);
                else if (coordDiff == new HexCoords(-1, 1)) nextRot = new Vector3(0f, -30f, 0f);
                else if (coordDiff == new HexCoords(-1, 0)) nextRot = new Vector3(0f, -90f, 0f);
                else if (coordDiff == new HexCoords(0, -1)) nextRot = new Vector3(0f, -150f, 0f);
                else if (coordDiff == new HexCoords(1, -1)) nextRot = new Vector3(0f, 150f, 0f);

                seq.Append(transform.DOMove(MapGenerator.CoordsToWorldPos(nextTile.Coord), _oneStepDuration)
                    .OnComplete(() =>
                    {
                        currentTile.RemoveUnitServerRpc();
                        _coord.Value = nextTile.Coord;
                        nextTile.PlaceUnitServerRpc(NetworkObjectId);
                    }).SetEase(Ease.Linear));
                seq.Join(transform.DORotate(nextRot, _oneStepDuration));
            }
        }
        else
        {
            throw new Exception($"Cannot find path towards Tile{targetCoord}");
        }
    }

    [ServerRpc]
    public void ShowWalkableTilesServerRpc(ulong clientId, int mobility)
    {
        // TODO : How to get UnitData
        _walkableCoords.Clear();
        var coords = Pathfinding.FindAllTilesWithinPathLength(MapGenerator.Instance.Tiles[Coord], mobility);
        foreach (var coord in coords) _walkableCoords.Add(coord);
        ClientRpcParams clientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };
        foreach (var coord in _walkableCoords) MapGenerator.Instance.Tiles[coord].ShowWalkableClientRpc(clientRpcParams);
    }

    [ServerRpc]
    public void HideWalkableTilesServerRpc(ulong clientId)
    {
        if(_walkableCoords.Count == 0) return;
        ClientRpcParams clientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };
        foreach (var coord in _walkableCoords) MapGenerator.Instance.Tiles[coord].RevertToNormalClientRpc(clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestShowUnitInfoServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        (int playerNumber, ClientRpcParams clientRpcParams) t = GameMananger.Instance.GetPlayerNumberAndClientRpcParam(clientId);
        if (t.playerNumber == -1) return;

        var unitData = ServerCard.CardData as ServerUnitBaseData;

        UnitInfo.Instance.PlayClientRpc(Coord, UnitHeight, unitData.Attack, unitData.Hp, unitData.Agility, t.clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestUnitAttackServerRpc()
    {
        if(State == Define.UnitState.Idle)
        {
            var animationClips = _animator.runtimeAnimatorController.animationClips;
            float attackClipLength = 0.0f;
            foreach(var animClip in animationClips)
            {
                if(animClip.name == "Attack") attackClipLength = animClip.length;
            }

            Sequence seq = DOTween.Sequence();
            seq.OnStart(() => { _state.Value = Define.UnitState.Attack; });
            seq.OnComplete(() => { _state.Value = Define.UnitState.Idle; });
            seq.AppendInterval(attackClipLength);
        }
    }

    public void PlaySlashVfx()
    {
        _slash.Play();
    }
}
