using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseFollow : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _rotateSpeed = 4f;
    [SerializeField] private float _rotateMult = 2.5f;
    [SerializeField] private float _clampThres = 10.0f;
    [SerializeField] private float _hoverHeight = 10.0f;
    Vector3 _position = Vector3.zero;
    private Camera _camera;

    private Vector3 ClampRotation(Vector3 rotation, float angleX, float angleY)
    {
        float x = Mathf.Clamp(rotation.x, -angleX, angleX);
        float y = Mathf.Clamp(rotation.y, -angleY, angleY);
        return new Vector3(x, y, rotation.z);
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit info;

        if (Physics.Raycast(ray, out info, Mathf.Infinity))
        {
            _position = Vector3.Lerp(transform.position, info.point, _moveSpeed * Time.deltaTime);
            gameObject.transform.position = _position;

            // rotate 속도는 move 속도에 따라 결정
            Vector3 rotation = new Vector3(-(transform.position - info.point).y * _rotateMult, (transform.position - info.point).x * _rotateMult, 0);
            if (transform.position.z <= 90 - _hoverHeight/2) 
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(ClampRotation(rotation, _clampThres, _clampThres)), _rotateSpeed * Time.deltaTime);
        }
    }
}
