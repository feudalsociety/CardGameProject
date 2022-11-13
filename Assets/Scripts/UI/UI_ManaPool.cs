using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class UI_ManaPool : UI_Base
{
    [SerializeField] private GameObject[] _manas;
    [SerializeField] private Transform _manasParent;
    [SerializeField] private TMP_Text _manaText;

    private readonly int _manaCapacity = 10;

    // This only influence visuals 
    public int TotalManasThisTurn;
    public int FullManas;

    private int _totalManas;
    public int TotalManas
    {
        get { return _totalManas; }

        set
        {
            if(value >= _manaCapacity) _totalManas = _manaCapacity;
            else if(value <= 0) _totalManas = 0;
            else _totalManas = value;

            for(int i = 0; i < _manaCapacity; i++)
            {
                if (i < _totalManas) _manas[i].SetActive(true);
                else _manas[i].SetActive(false);
            }

            // Update Text
            _manaText.text = $"{_availableManas}/{_totalManas}";
        }
    }

    private int _availableManas;
    public int AvailableManas
    {
        get { return _availableManas; }

        set
        {
            if(value >= _totalManas) _availableManas = _totalManas;
            else if(value <= 0) _availableManas = 0;
            else _availableManas = value;

            for(int i = 0; i < TotalManas; i++)
            {
                if(i < _availableManas) _manas[i].SetActive(true);
                else _manas[i].SetActive(false);
            }

            // UpdateText
            _manaText.text = $"{_availableManas}/{_totalManas}";
        }
    }

    private void Awake()
    {
        SameDistanceChildren();
    }

    private void SameDistanceChildren()
    {
        Vector3 firstElementPos = _manas[0].transform.position;
        Vector3 lastElementPos = _manas[_manas.Length - 1].transform.position;

        // dividing by Children.Length - 1 
        float xDist = (lastElementPos.x - firstElementPos.x) / (float)(_manas.Length - 1);
        float yDist = (lastElementPos.y - firstElementPos.y) / (float)(_manas.Length - 1);
        float zDist = (lastElementPos.z - firstElementPos.z) / (float)(_manas.Length - 1);

        Vector3 Dist = new Vector3(xDist, yDist, zDist);

        for(int i = 1; i < _manas.Length; i++)
        {
            _manas[i].transform.position = _manas[i - 1].transform.position + Dist;
        }
    }

    public override void Init() { }

    private void Update()
    {
        if(Application.isEditor && !Application.isPlaying)
        {
            TotalManas = TotalManasThisTurn;
            AvailableManas = FullManas;
            SameDistanceChildren();
        }
    }
}
