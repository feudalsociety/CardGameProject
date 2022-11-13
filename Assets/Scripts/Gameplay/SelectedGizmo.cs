using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedGizmo : MonoBehaviour
{
    Material _hdrMaterial;
    Color _initialColor;
    float _intensity;
    [SerializeField] public float TwinkleSpeed = 0.5f;
    [SerializeField] public float IntensityMax = 2.0f;
    [SerializeField] public float IntensityMin = 1.0f;

    private void Awake()
    {
        _hdrMaterial = GetComponent<Renderer>().material;
        _initialColor = _hdrMaterial.GetColor("_EmissionColor");
    }

    void Update()
    {
        _intensity = Mathf.PingPong(Time.time * TwinkleSpeed, IntensityMax - IntensityMin) + IntensityMin;
        float factor = Mathf.Pow(2, _intensity);
        Color color = new Color(_initialColor.r * factor, _initialColor.g * factor, _initialColor.b * factor, _initialColor.a);
        _hdrMaterial.SetColor("_EmissionColor", color);
    }
}
