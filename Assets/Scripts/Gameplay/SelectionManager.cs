using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : Singleton<SelectionManager>
{
    private int _defaultLayer;
    private int _highlightLayer;

    // TODO : Effect ���� ��Ű��, Pooling �̿��ϱ�
    [SerializeField] private GameObject _selectGizmoPrefab;
    private float _selectGizmoHoverHeight = 2f; 

    private GameObject _currentTarget = null;
    private GameObject _clickedObject = null;

    public GameObject CurrentTarget => _currentTarget;
    public HexCoords? SelectedCoord { get; private set; } = null;

    private void Awake()
    {
        _defaultLayer = LayerMask.NameToLayer("Default");
        _highlightLayer = LayerMask.NameToLayer("Highlight");

        Managers.Input.MouseAction -= OnMouseClicked;
        Managers.Input.MouseAction += OnMouseClicked;
    }

    private void Update()
    {
        OnHoverObjects(LayerMask.GetMask("Default", "Highlight"));
    }

    void OnHoverObjects(int layerMask)
    {
        GameObject target = Managers.Input.GetPointerOverElement(null, layerMask);

        if (target != null)
        {
            if (_currentTarget != target)
            {
                if (_currentTarget != null)
                {
                    _currentTarget.ChangeLayerRecursively(_defaultLayer);

                    var targetUnit = target.GetComponent<Unit>();
                    if (targetUnit != null)
                    {
                        UnitInfo.Instance.Stop();
                        targetUnit.RequestShowUnitInfoServerRpc();
                    }
                }
                else UnitInfo.Instance.Stop();
            }

            _currentTarget = target;  
            _currentTarget.ChangeLayerRecursively(_highlightLayer);

            var CurrentUnit = _currentTarget.GetComponent<Unit>();
            if (CurrentUnit != null) CurrentUnit.RequestShowUnitInfoServerRpc();
            else UnitInfo.Instance.Stop();

        }
        else // target is null
        {
            if (_currentTarget != null) _currentTarget.ChangeLayerRecursively(_defaultLayer);
            _currentTarget = null;
        }
    }

    void OnMouseClicked(Define.MouseEvent evt, Define.MouseButton button)
    {
        switch (button)
        {
            case Define.MouseButton.Left:
                if (Managers.Input.IsPointerOverUIElement()) break;

                if (_currentTarget && (_currentTarget.layer == LayerMask.NameToLayer("Highlight")))
                {
                    if (evt == Define.MouseEvent.Down)
                    {
                        _clickedObject = _currentTarget;

                        if (_clickedObject.GetComponent<Unit>())
                        {
                            // ������ ���õȰ��� �����ϰ� �װ��� ���� ���õ� Unit�� �ٸ��ٸ�
                            if (SelectedCoord.HasValue && (SelectedCoord.Value != _clickedObject.GetComponent<Unit>().Coord))
                                Deselect();

                            Select(_clickedObject.GetComponent<Unit>().Coord);
                        }

                        if (_clickedObject.IsObjectTypeOf<Tile>()) 
                        {
                            var tile = _clickedObject.GetComponent<Tile>();

                            // ī�带 �������� ���� ���°� ���࿡ Ÿ�� ���� Unit�� �����Ѵٸ�
                            // (� Unit������ ������ �ʿ����� ����)
                            if (MyHandManager.Instance.SelectedIndex == -1 && !tile.Walkable)
                            {
                                if (SelectedCoord.HasValue && (SelectedCoord.Value != tile.Coord))
                                    Deselect();

                                Select(tile.Coord);
                            }
                            // ī�带 ������ ���¶��
                            else if (MyHandManager.Instance.SelectedIndex != -1)
                            {
                                var selectedIndex = MyHandManager.Instance.SelectedIndex;
                                GameMananger.Instance.PlayerHands.RequestPlayCardFromHandServerRpc(selectedIndex, tile.Coord);
                                Select(tile.Coord);
                            }
                        }
                    }

                    if (evt == Define.MouseEvent.Up) { _clickedObject = null; }

                    if (evt == Define.MouseEvent.Press)
                    {
                        if (SelectedCoord.HasValue)
                        {
                            bool condition1 = _currentTarget && _currentTarget.IsObjectTypeOf<Tile>() && _currentTarget.GetComponent<Tile>().Coord != SelectedCoord.Value;
                            bool condition2 = _currentTarget && _currentTarget.IsObjectTypeOf<Unit>() && _currentTarget.GetComponent<Unit>().Coord != SelectedCoord.Value;

                            if (condition1 || condition2) Deselect();
                        }
                    }
                }
                break;
            case Define.MouseButton.Right:
                // ���� target�� selected unit �Ǵ� unit�� �����ϴ� Ÿ���� �ƴ϶��
                if (SelectedCoord.HasValue)
                {
                    Deselect();
                }

                _selectGizmoPrefab.SetActive(false);

                break;
        }
    }

    private void Select(HexCoords coord)
    {
        SelectedCoord = coord;

        _selectGizmoPrefab.SetActive(true);
        _selectGizmoPrefab.transform.position = 
            MapGenerator.CoordsToWorldPos(coord) + new Vector3(0, _selectGizmoHoverHeight, 0);

        UnitManager.Instance.RequestShowWakableTilesServerRpc(coord);
    }

    public void Deselect() 
    {
        // SelectedCoord�� null�� �ƴ��� ����Ǿ��ִ�.
        UnitManager.Instance.RequestHideWalkableTilesServerRpc(SelectedCoord.Value);

        SelectedCoord = null;
        _selectGizmoPrefab.SetActive(false);
    }

    private void OnDestroy()
    {
        Managers.Input.MouseAction -= OnMouseClicked;
    }
}