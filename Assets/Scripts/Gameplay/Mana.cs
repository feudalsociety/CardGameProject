using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mana : MonoBehaviour
{
    private static Vector3 _rotation = new Vector3(1, 1, 0);
    private static float _rotateSpeed = 60.0f;
    private static float _accelAmount = 150.0f;
    private static float _decelSpeed = 1.0f;
    private float _speed = 0f;

    private void OnMouseDown()
    {
        _speed = _rotateSpeed + _accelAmount;
    }

    private void OnMouseOver()
    {
        if (_speed <= _rotateSpeed) _speed = _rotateSpeed;
    }

    // Update is called once per frame
    private void Update()
    {
        if(_speed >= 0) _speed -= _decelSpeed;
        if(_speed < 0f) _speed = 0f;

        transform.Rotate(_rotation * Time.deltaTime * _speed);
    }
}
