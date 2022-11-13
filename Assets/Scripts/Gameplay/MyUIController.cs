using UnityEngine;

[System.Serializable]
public class UITransform
{
    public readonly Vector3 Rotation;
    public readonly float OffsetFromTarget;
    public readonly float DistanceFromTarget;

    public UITransform(Vector3 rotation, float offsetFromTarget, float distancefromTarget)
    {
        Rotation = rotation;
        OffsetFromTarget = offsetFromTarget;
        DistanceFromTarget = distancefromTarget;
    }
}

public class MyUIController : Singleton<MyUIController>
{
    //This is Main Camera in the Scene
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _playerUICamera;
    [SerializeField] private GameObject _cameraTarget;

    // enemyUI
    [SerializeField] private UI_Enemy _enemyUI;

    public Camera PlayerCamera => _playerCamera;

    public bool CameraControl = true;

    [SerializeField] float _rotationSpeed = 1.5f, _rotationSmoothTime = 0.2f, _rotationMin = 30.0f, _rotationMax = 70.0f;
    [SerializeField] float _zoomSpeed = 250.0f, _zoomSmoothTime = 0.2f, _distanceMin = 100.0f, _distanceMax = 1300.0f;
    [SerializeField] float _panSpeed = 22.0f, _panSmoothTime = 0.25f;
    [SerializeField] float _moveRange = 1200.0f;
    [SerializeField] float _slideAmount = 45.0f;

    float _rotationX; float _rotationY;
    float _zoomDistance;
    Vector3 _targetPos;

    Vector3 _currentRotation;                // always smoothdamp to rotationX & rotationY
    float _distanceFromTarget;               // always smoothdamp to zoomDistance
    Vector3 _currentFocusPos;                // always smoothdamp to target


    Vector3 _rotationSmoothVelocity = Vector3.zero;
    float _zoomSmoothVelocity = 0.0f;
    Vector3 _panSmoothVelocity = Vector3.zero;

    private float _isCameraMoving = 0.0f;
    public bool IsCameraMoving => _isCameraMoving > 0;

    [SerializeField] float _cardSelectBlockTime = 0.6f;
    [SerializeField] public float IsMovingThreshold = 0.1f;

    private void Awake()
    {
        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.MouseAction += OnMouseEvent;

        Managers.Input.ScrollAction -= OnScrollEvent;
        Managers.Input.ScrollAction += OnScrollEvent;

    }

    private void Update()
    {
        if (_isCameraMoving > 0.0f) _isCameraMoving -= Time.deltaTime;
    }

    void LateUpdate()
    {
        UpdateCameraMovement();
    }

    public void SetMyPlayerUITransform(UITransform playerTransform)
    {
        // rotation
        _currentRotation = playerTransform.Rotation;
        _rotationX = playerTransform.Rotation.x;
        _rotationY = playerTransform.Rotation.y;

        // position
        _currentFocusPos = MapGenerator.Instance.GetCenterPosition()
           + new Vector3(
               -playerTransform.OffsetFromTarget * Mathf.Sin(playerTransform.Rotation.y * Mathf.Deg2Rad),
               0f,
               -playerTransform.OffsetFromTarget * Mathf.Cos(playerTransform.Rotation.y * Mathf.Deg2Rad));

        _targetPos = _currentFocusPos;
        _cameraTarget.transform.position = _targetPos;

        // distance
        _distanceFromTarget = playerTransform.DistanceFromTarget;
        _zoomDistance = _distanceFromTarget;
    }

    public void SetEnemyPlayerUITransform(UITransform playerTransform)
    {
        _enemyUI.SetEnemyTransform(playerTransform);
    }

    private void RotateCamera()
    {
        _rotationY += Input.GetAxis("Mouse X") * _rotationSpeed;
        _rotationX -= Input.GetAxis("Mouse Y") * _rotationSpeed;
    }

    private void ZoomCamera(float delta)
    {
        _isCameraMoving = _cardSelectBlockTime;
        _zoomDistance -= delta * _zoomSpeed;
    }

    private void PanCamera()
    {
        Vector3 movementX = _playerCamera.transform.right * Input.GetAxis("Mouse X") * _panSpeed;
        Vector3 movementY = Quaternion.Euler(0, _rotationY, 0) * Vector3.forward * Input.GetAxis("Mouse Y") * _panSpeed;
        if (Vector3.Distance(_targetPos - (movementX + movementY), MapGenerator.Instance.GetCenterPosition()) <= _moveRange)
        {
            _targetPos -= movementX + movementY;
        }
        else
        {
            float signedAngle = Vector3.SignedAngle(_targetPos - MapGenerator.Instance.GetCenterPosition(), -(movementX + movementY), Vector3.up);
            float direction = Mathf.Sign(signedAngle);
            // Rotate Around
            _cameraTarget.transform.RotateAround(MapGenerator.Instance.GetCenterPosition(), Vector3.up * direction, _slideAmount * Time.deltaTime);
            _targetPos = _cameraTarget.transform.position;
        }

        _cameraTarget.transform.position = _targetPos;
    }

    private void UpdateCameraMovement()
    {
        // Update rotation
        _rotationX = Mathf.Clamp(_rotationX, _rotationMin, _rotationMax);

        Vector3 nextRotation = new Vector3(_rotationX, _rotationY, 0);
        _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _rotationSmoothVelocity, _rotationSmoothTime);
        _playerCamera.transform.localEulerAngles = _currentRotation;

        // Update zoom
        _zoomDistance = Mathf.Clamp(_zoomDistance, _distanceMin, _distanceMax);
        _distanceFromTarget = Mathf.SmoothDamp(_distanceFromTarget, _zoomDistance, ref _zoomSmoothVelocity, _zoomSmoothTime);

        // Update pan
        _currentFocusPos = Vector3.SmoothDamp(_currentFocusPos, _targetPos, ref _panSmoothVelocity, _panSmoothTime);

        // Update camera position accordingly
        _playerCamera.transform.position = _currentFocusPos - _playerCamera.transform.forward * (_distanceFromTarget + UI_MyPlayer.DistanceFromCamera);
    }

    private void OnMouseEvent(Define.MouseEvent evt, Define.MouseButton button)
    {
        switch (button)
        {
            case Define.MouseButton.Left:
                if (Managers.Input.IsPointerOverUIElement()) break;

                if (!CameraControl) break;

                // selection 상태에서는 camera 못움직이게 막기
                if (MyHandManager.Instance.SelectedIndex != -1) break;

                if (Mathf.Abs(Input.GetAxis("Mouse X")) > IsMovingThreshold || Mathf.Abs(Input.GetAxis("Mouse Y")) > IsMovingThreshold)
                    _isCameraMoving = _cardSelectBlockTime;

                if (!Input.GetKey(KeyCode.LeftControl)) RotateCamera();
                else PanCamera();

                break;
            case Define.MouseButton.Right:
                break;
        }
    }

    private void OnScrollEvent(float delta)
    {
        if (!CameraControl) return;
        ZoomCamera(delta);
    }

    private void OnDestroy()
    {
        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.ScrollAction -= OnScrollEvent;
    }
}
