using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private Image _healthbarSprite;
    [SerializeField] private float _reduceSpeed = 2f;
    [SerializeField] private float _hoverHeight = 50f;
    private float _target = 1;
    private Camera _camera;

    public void Init(float unitHeight)
    {
        _camera = Camera.main;
        transform.localPosition = new Vector3(0, unitHeight + _hoverHeight, 0);
    }

    public void UpdateHealthbar(float maxHealth, float currentHealth)
    {
        _target = currentHealth / maxHealth;
    }

    public void SetHealthbarColor(Color color)
    {
        _healthbarSprite.color = color;
    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
        _healthbarSprite.fillAmount = Mathf.MoveTowards(_healthbarSprite.fillAmount, _target, _reduceSpeed * Time.deltaTime);
    }
}
